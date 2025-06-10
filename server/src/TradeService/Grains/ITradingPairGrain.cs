using Orleans;
using TradeService.Models;

namespace TradeService.Grains;

/// <summary>
/// Grain для работы с отдельной торговой парой
/// Ключ: "{Symbol}:{Type}" (например: "BTCUSDT:Spot")
/// </summary>
public interface ITradingPairGrain : IGrainWithStringKey
{
    /// <summary>
    /// Получить актуальные рыночные данные с биржи
    /// (цена, объем 24ч, изменение 24ч)
    /// </summary>
    Task<TradingPairMarketData> GetMarketDataAsync();
    
    /// <summary>
    /// Инициализировать Grain с базовой информацией о торговой паре
    /// </summary>
    Task InitializeAsync(TradingPair tradingPair);
    
    /// <summary>
    /// Получить время последнего обновления данных
    /// </summary>
    Task<DateTime?> GetLastUpdateTimeAsync();
    
    // === Анализ цен ===
    
    /// <summary>
    /// Запустить анализ цен в реальном времени (WebSocket)
    /// </summary>
    Task<bool> StartPriceAnalysisAsync();
    
    /// <summary>
    /// Остановить анализ цен
    /// </summary>
    Task<bool> StopPriceAnalysisAsync();
    
    /// <summary>
    /// Получить информацию о текущем анализе
    /// </summary>
    Task<PairAnalysisInfo> GetAnalysisInfoAsync();
    
    /// <summary>
    /// Получить детальные данные анализа за период
    /// </summary>
    Task<PriceAnalysisDetails> GetAnalysisDetailsAsync(DateTime fromTime, DateTime toTime);
    
    /// <summary>
    /// Проверить активен ли анализ
    /// </summary>
    Task<bool> IsAnalysisRunningAsync();
} 