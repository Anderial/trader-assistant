namespace TradeService.Models;

[GenerateSerializer]
[Alias(nameof(TradingPair))]
public class TradingPair
{
    [Id(0)]
    public required string Symbol { get; set; }
    [Id(1)]
    public required string BaseCoin { get; set; }
    [Id(2)]
    public required string QuoteCoin { get; set; }
    [Id(3)]
    public required string Status { get; set; }
    [Id(4)]
    public required TradingPairType Type { get; set; }
    [Id(5)]
    public decimal MinOrderQty { get; set; }
    [Id(6)]
    public decimal MaxOrderQty { get; set; }
    [Id(7)]
    public decimal TickSize { get; set; }
    [Id(8)]
    public int LotSizeFilter { get; set; }
    [Id(9)]
    public bool IsTrading { get; set; }
    [Id(10)]
    public DateTime LastUpdated { get; set; }

    // Market data
    [Id(11)]
    public decimal LastPrice { get; set; }
    [Id(12)]
    public decimal Volume24H { get; set; }
    [Id(13)]
    public decimal PriceChange24H { get; set; }
    [Id(14)]
    public decimal PriceChangePercent24H { get; set; }
}

[GenerateSerializer]
[Alias(nameof(TradingPairType))]
public enum TradingPairType
{
    [Id(0)]
    Spot,
    [Id(1)]
    LinearFutures,
    [Id(2)]
    InverseFutures,
    [Id(3)]
    Option
}

[GenerateSerializer]
[Alias(nameof(TradingPairResponse))]
public class TradingPairResponse
{
    [Id(0)]
    public int RetCode { get; set; }
    [Id(1)]
    public string RetMsg { get; set; } = string.Empty;
    [Id(2)]
    public TradingPairResult Result { get; set; } = new();
}

[GenerateSerializer]
[Alias(nameof(TradingPairResult))]
public class TradingPairResult
{
    [Id(0)]
    public string Category { get; set; } = string.Empty;
    [Id(1)]
    public List<ByBitTradingPair> List { get; set; } = [];
}

[GenerateSerializer]
[Alias(nameof(ByBitTradingPair))]
public class ByBitTradingPair
{
    [Id(0)]
    public string Symbol { get; set; } = string.Empty;
    [Id(1)]
    public string BaseCoin { get; set; } = string.Empty;
    [Id(2)]
    public string QuoteCoin { get; set; } = string.Empty;
    [Id(3)]
    public string Status { get; set; } = string.Empty;
    [Id(4)]
    public LotSizeFilter LotSizeFilter { get; set; } = new();
    [Id(5)]
    public PriceFilter PriceFilter { get; set; } = new();
}

[GenerateSerializer]
[Alias("LotSizeFilter")]
public class LotSizeFilter
{
    [Id(0)]
    public string BasePrecision { get; set; } = string.Empty;
    [Id(1)]
    public string QuotePrecision { get; set; } = string.Empty;
    [Id(2)]
    public string MinOrderQty { get; set; } = string.Empty;
    [Id(3)]
    public string MaxOrderQty { get; set; } = string.Empty;
    [Id(4)]
    public string MinOrderAmt { get; set; } = string.Empty;
    [Id(5)]
    public string MaxOrderAmt { get; set; } = string.Empty;
}

[GenerateSerializer]
[Alias(nameof(PriceFilter))]
public class PriceFilter
{
    [Id(0)]
    public string TickSize { get; set; } = string.Empty;
}