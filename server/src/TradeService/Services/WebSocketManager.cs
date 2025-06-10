using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradeService.Models;

namespace TradeService.Services;

/// <summary>
/// Менеджер WebSocket соединений для всего приложения
/// </summary>
public interface IWebSocketManager
{
    /// <summary>
    /// Запустить WebSocket соединение
    /// </summary>
    Task StartAsync();
    
    /// <summary>
    /// Остановить WebSocket соединение
    /// </summary>
    Task StopAsync();
    
    /// <summary>
    /// Получить WebSocket сервис
    /// </summary>
    IBybitWebSocketService GetWebSocketService();
}

/// <summary>
/// Фоновый сервис для управления WebSocket соединениями
/// </summary>
public class WebSocketManager : BackgroundService, IWebSocketManager
{
    private readonly IBybitWebSocketService _webSocketService;
    private readonly ILogger<WebSocketManager> _logger;

    public WebSocketManager(IBybitWebSocketService webSocketService, ILogger<WebSocketManager> logger)
    {
        _webSocketService = webSocketService;
        _logger = logger;
    }

    public IBybitWebSocketService GetWebSocketService() => _webSocketService;

    public async Task StartAsync()
    {
        if (!_webSocketService.IsConnected)
        {
            await _webSocketService.StartAsync();
            _logger.LogInformation("WebSocket manager started");
        }
    }

    public async Task StopAsync()
    {
        if (_webSocketService.IsConnected)
        {
            await _webSocketService.StopAsync();
            _logger.LogInformation("WebSocket manager stopped");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WebSocket manager background service starting");
        
        try
        {
            await StartAsync();
            
            // Поддерживаем соединение живым
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_webSocketService.IsConnected)
                {
                    _logger.LogWarning("WebSocket connection lost, attempting to reconnect...");
                    try
                    {
                        await _webSocketService.StartAsync();
                        _logger.LogInformation("WebSocket connection restored");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to reconnect WebSocket");
                    }
                }
                
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("WebSocket manager background service cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebSocket manager background service error");
        }
        finally
        {
            await StopAsync();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopAsync();
        await base.StopAsync(cancellationToken);
    }
} 