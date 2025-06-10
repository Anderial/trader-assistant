using TradeService.Models;

namespace TradeService.Commands;

/// <summary>
/// Команда для получения всех торговых пар с биржи Bybit
/// </summary>
[GenerateSerializer]
[Alias(nameof(GetTradingPairsCommand))]
public class GetTradingPairsCommand : BaseCommand<List<TradingPair>>
{
    public GetTradingPairsCommand()
    {
        CommandType = nameof(GetTradingPairsCommand);
    }
    
    /// <summary>
    /// Тип торговых пар для получения (опционально)
    /// Если null, получаем все типы
    /// </summary>
    [Id(3)]
    public TradingPairType? PairType { get; set; }
    
    /// <summary>
    /// Фильтр по базовой валюте (опционально)
    /// </summary>
    [Id(4)]
    public string? BaseAsset { get; set; }
    
    /// <summary>
    /// Фильтр по котируемой валюте (опционально)  
    /// </summary>
    [Id(5)]
    public string? QuoteAsset { get; set; }
    
    /// <summary>
    /// Получить только активные пары
    /// </summary>
    [Id(6)]
    public bool ActiveOnly { get; set; } = true;
} 