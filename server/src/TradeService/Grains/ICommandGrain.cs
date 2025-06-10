using TradeService.Commands;
using TradeService.Models;

namespace TradeService.Grains;

/// <summary>
/// Централизованный Grain для обработки команд пользователя
/// Singleton grain (Key = 0)
/// </summary>
public interface ICommandGrain : IGrainWithIntegerKey
{
    /// <summary>
    /// Получить все торговые пары с биржи
    /// </summary>
    Task<List<TradingPair>> GetTradingPairs(GetTradingPairsCommand command);
    
    /// <summary>
    /// Запустить анализ торговой пары
    /// </summary>
    Task<bool> StartAnalysis(StartAnalysisCommand command);
    
    /// <summary>
    /// Остановить анализ торговой пары
    /// </summary>
    Task<bool> StopAnalysis(StopAnalysisCommand command);
    
    /// <summary>
    /// Получить список запущенных анализов
    /// </summary>
    Task<List<PairAnalysisInfo>> GetRunningAnalysis(GetRunningAnalysisCommand command);
    
    /// <summary>
    /// Получить детальные данные анализа
    /// </summary>
    Task<PriceAnalysisDetails> GetAnalysisDetails(GetAnalysisDetailsCommand command);
} 