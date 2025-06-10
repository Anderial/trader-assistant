using Orleans;

namespace TradeService.Models;

/// <summary>
/// Данные о изменении цены из WebSocket
/// </summary>
[GenerateSerializer]
[Alias("TradeService.Models.PriceTickData")]
public class PriceTickData
{
    [Id(0)]
    public string Symbol { get; set; } = string.Empty;
    
    [Id(1)]
    public decimal Price { get; set; }
    
    [Id(2)]
    public decimal Volume { get; set; }
    
    [Id(3)]
    public DateTime Timestamp { get; set; }
    
    [Id(4)]
    public decimal PriceChange24h { get; set; }
    
    [Id(5)]
    public decimal PriceChangePercent24h { get; set; }
    
    [Id(6)]
    public decimal HighPrice24h { get; set; }
    
    [Id(7)]
    public decimal LowPrice24h { get; set; }
}

/// <summary>
/// Статус анализа торговой пары
/// </summary>
public enum AnalysisStatus
{
    Stopped = 0,
    Starting = 1,
    Running = 2,
    Stopping = 3,
    Error = 4
}

/// <summary>
/// Информация об анализе торговой пары
/// </summary>
[GenerateSerializer]
[Alias("TradeService.Models.PairAnalysisInfo")]
public class PairAnalysisInfo
{
    [Id(0)]
    public string PairKey { get; set; } = string.Empty;
    
    [Id(1)]
    public string Symbol { get; set; } = string.Empty;
    
    [Id(2)]
    public TradingPairType Type { get; set; }
    
    [Id(3)]
    public AnalysisStatus Status { get; set; }
    
    [Id(4)]
    public DateTime StartedAt { get; set; }
    
    [Id(5)]
    public DateTime? LastUpdateAt { get; set; }
    
    [Id(6)]
    public int DataPointsCollected { get; set; }
    
    [Id(7)]
    public decimal? CurrentPrice { get; set; }
    
    [Id(8)]
    public decimal? PriceChangePercent { get; set; }
    
    [Id(9)]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Детальные данные анализа за временной период
/// </summary>
[GenerateSerializer]
[Alias("TradeService.Models.PriceAnalysisDetails")]
public class PriceAnalysisDetails
{
    [Id(0)]
    public string PairKey { get; set; } = string.Empty;
    
    [Id(1)]
    public DateTime FromTime { get; set; }
    
    [Id(2)]
    public DateTime ToTime { get; set; }
    
    [Id(3)]
    public List<PriceTickData> PriceTicks { get; set; } = new();
    
    [Id(4)]
    public decimal MinPrice { get; set; }
    
    [Id(5)]
    public decimal MaxPrice { get; set; }
    
    [Id(6)]
    public decimal AveragePrice { get; set; }
    
    [Id(7)]
    public decimal TotalVolume { get; set; }
    
    [Id(8)]
    public int TickCount { get; set; }
} 