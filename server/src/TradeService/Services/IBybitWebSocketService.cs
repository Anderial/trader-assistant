using TradeService.Models;

namespace TradeService.Services;

/// <summary>
/// Сервис для работы с WebSocket Bybit
/// </summary>
public interface IBybitWebSocketService
{
    /// <summary>
    /// Подписаться на ticker данные торговой пары
    /// </summary>
    Task<bool> SubscribeToTickerAsync(string symbol, TradingPairType type, Func<BybitTickerData, Task> onDataReceived);
    
    /// <summary>
    /// Отписаться от ticker данных торговой пары
    /// </summary>
    Task<bool> UnsubscribeFromTickerAsync(string symbol, TradingPairType type);
    
    /// <summary>
    /// Проверить подключение к WebSocket
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Запустить WebSocket подключение
    /// </summary>
    Task StartAsync();
    
    /// <summary>
    /// Остановить WebSocket подключение
    /// </summary>
    Task StopAsync();
} 