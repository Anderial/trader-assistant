using Orleans;

namespace TradeService.Models;

/// <summary>
/// Рыночные данные торговой пары
/// </summary>
[GenerateSerializer]
[Alias("TradeService.Models.TradingPairMarketData")]
public class TradingPairMarketData
{
    [Id(0)]
    public string Symbol { get; set; } = string.Empty;
    
    [Id(1)]
    public TradingPairType Type { get; set; }
    
    [Id(2)]
    public decimal? CurrentPrice { get; set; }
    
    [Id(3)]
    public decimal? Volume24h { get; set; }
    
    [Id(4)]
    public decimal? PriceChange24h { get; set; }
    
    [Id(5)]
    public decimal? PriceChangePercent24h { get; set; }
    
    [Id(6)]
    public decimal? HighPrice24h { get; set; }
    
    [Id(7)]
    public decimal? LowPrice24h { get; set; }
    
    [Id(8)]
    public DateTime? LastUpdateTime { get; set; }
    
    [Id(9)]
    public bool IsActive { get; set; }
    
    [Id(10)]
    public string BaseAsset { get; set; } = string.Empty;
    
    [Id(11)]
    public string QuoteAsset { get; set; } = string.Empty;
} 