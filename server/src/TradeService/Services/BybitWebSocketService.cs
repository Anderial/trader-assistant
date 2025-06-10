using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TradeService.Models;

namespace TradeService.Services;

/// <summary>
/// Реализация WebSocket сервиса для Bybit
/// </summary>
public class BybitWebSocketService : IBybitWebSocketService, IDisposable
{
    private readonly ILogger<BybitWebSocketService> _logger;
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly Dictionary<string, Func<BybitTickerData, Task>> _subscriptions = new();
    private const string BYBIT_WS_URL = "wss://stream.bybit.com/v5/public/spot";

    public bool IsConnected => _webSocket?.State == WebSocketState.Open;

    public BybitWebSocketService(ILogger<BybitWebSocketService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync()
    {
        try
        {
            _logger.LogInformation("Starting Bybit WebSocket connection...");
            
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
            
            await _webSocket.ConnectAsync(new Uri(BYBIT_WS_URL), _cancellationTokenSource.Token);
            
            _logger.LogInformation("Bybit WebSocket connected successfully");
            
            // Запускаем прослушивание в фоне
            _ = Task.Run(ListenForMessages, _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Bybit WebSocket connection");
            throw;
        }
    }

    public async Task StopAsync()
    {
        try
        {
            _logger.LogInformation("Stopping Bybit WebSocket connection...");
            
            _cancellationTokenSource?.Cancel();
            
            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stopping", CancellationToken.None);
            }
            
            _subscriptions.Clear();
            _logger.LogInformation("Bybit WebSocket connection stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping Bybit WebSocket connection");
        }
    }

    public async Task<bool> SubscribeToTickerAsync(string symbol, TradingPairType type, Func<BybitTickerData, Task> onDataReceived)
    {
        try
        {
            if (!IsConnected)
            {
                _logger.LogWarning("WebSocket not connected, cannot subscribe to {Symbol}", symbol);
                return false;
            }

            var topic = GetTickerTopic(symbol, type);
            _subscriptions[topic] = onDataReceived;

            var subscribeMessage = new
            {
                op = "subscribe",
                args = new[] { topic }
            };

            var json = JsonSerializer.Serialize(subscribeMessage);
            var buffer = Encoding.UTF8.GetBytes(json);
            
            await _webSocket!.SendAsync(
                new ArraySegment<byte>(buffer), 
                WebSocketMessageType.Text, 
                true, 
                _cancellationTokenSource!.Token);

            _logger.LogInformation("Subscribed to ticker for {Symbol}:{Type} (topic: {Topic})", symbol, type, topic);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to ticker for {Symbol}:{Type}", symbol, type);
            return false;
        }
    }

    public async Task<bool> UnsubscribeFromTickerAsync(string symbol, TradingPairType type)
    {
        try
        {
            var topic = GetTickerTopic(symbol, type);
            _subscriptions.Remove(topic);

            if (!IsConnected)
            {
                _logger.LogWarning("WebSocket not connected, but removed subscription for {Symbol}", symbol);
                return true;
            }

            var unsubscribeMessage = new
            {
                op = "unsubscribe",
                args = new[] { topic }
            };

            var json = JsonSerializer.Serialize(unsubscribeMessage);
            var buffer = Encoding.UTF8.GetBytes(json);
            
            await _webSocket!.SendAsync(
                new ArraySegment<byte>(buffer), 
                WebSocketMessageType.Text, 
                true, 
                _cancellationTokenSource!.Token);

            _logger.LogInformation("Unsubscribed from ticker for {Symbol}:{Type}", symbol, type);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe from ticker for {Symbol}:{Type}", symbol, type);
            return false;
        }
    }

    private async Task ListenForMessages()
    {
        var buffer = new byte[4096];
        
        try
        {
            while (_webSocket?.State == WebSocketState.Open && !_cancellationTokenSource!.Token.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), 
                    _cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessMessage(message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("WebSocket closed by server");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("WebSocket listening cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WebSocket message listening");
        }
    }

    private async Task ProcessMessage(string message)
    {
        try
        {
            _logger.LogDebug("Received WebSocket message: {Message}", message);
            
            using var document = JsonDocument.Parse(message);
            var root = document.RootElement;

            // Проверяем разные типы сообщений от Bybit
            if (root.TryGetProperty("success", out var successElement))
            {
                // Это ответ на подписку/отписку
                var success = successElement.GetBoolean();
                var op = root.TryGetProperty("op", out var opElement) ? opElement.GetString() : "unknown";
                _logger.LogInformation("Bybit WebSocket operation {Operation} result: {Success}", op, success);
                return;
            }

            // Проверяем, что это ticker данные
            if (root.TryGetProperty("topic", out var topicElement))
            {
                var topic = topicElement.GetString();
                if (topic != null && _subscriptions.TryGetValue(topic, out var callback))
                {
                    if (root.TryGetProperty("data", out var dataElement))
                    {
                        _logger.LogDebug("Processing ticker data for topic {Topic}: {Data}", topic, dataElement.GetRawText());
                        
                        var tickerData = ParseTickerData(dataElement, topic);
                        if (tickerData != null)
                        {
                            await callback(tickerData);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WebSocket message: {Message}", message);
        }
    }

    private BybitTickerData? ParseTickerData(JsonElement dataElement, string topic)
    {
        try
        {
            var symbol = ExtractSymbolFromTopic(topic);
            
            // Bybit может возвращать данные как объект или как массив с одним элементом
            JsonElement tickerElement = dataElement;
            if (dataElement.ValueKind == JsonValueKind.Array && dataElement.GetArrayLength() > 0)
            {
                tickerElement = dataElement[0];
            }
            
            return new BybitTickerData
            {
                Symbol = symbol,
                LastPrice = ParseDecimalSafe(tickerElement, "lastPrice"),
                Volume24h = ParseDecimalSafe(tickerElement, "volume24h"),
                PriceChange24h = ParseDecimalSafe(tickerElement, "price24hPcnt"),
                PriceChangePercent24h = ParseDecimalSafe(tickerElement, "price24hPcnt") * 100,
                HighPrice24h = ParseDecimalSafe(tickerElement, "highPrice24h"),
                LowPrice24h = ParseDecimalSafe(tickerElement, "lowPrice24h"),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse ticker data from topic {Topic}: {@DataElement}", topic, dataElement.GetRawText());
            return null;
        }
    }

    private decimal ParseDecimalSafe(JsonElement dataElement, string propertyName)
    {
        try
        {
            if (dataElement.TryGetProperty(propertyName, out var property))
            {
                var stringValue = property.GetString();
                if (!string.IsNullOrEmpty(stringValue))
                {
                    // Используем инвариантную культуру для парсинга
                    return decimal.Parse(stringValue, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            return 0m;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse decimal property {PropertyName} from ticker data", propertyName);
            return 0m;
        }
    }

    private string GetTickerTopic(string symbol, TradingPairType type)
    {
        // Bybit WebSocket topics для разных типов
        return type switch
        {
            TradingPairType.Spot => $"tickers.{symbol}",
            TradingPairType.Futures => $"tickers.{symbol}",
            TradingPairType.Options => $"tickers.{symbol}",
            _ => $"tickers.{symbol}"
        };
    }

    private string ExtractSymbolFromTopic(string topic)
    {
        // Извлекаем символ из топика типа "tickers.BTCUSDT"
        var parts = topic.Split('.');
        return parts.Length > 1 ? parts[1] : "";
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
} 