using System.ComponentModel.DataAnnotations;

namespace TradeService.Models;

/// <summary>
/// Универсальная модель торговой пары
/// </summary>
[GenerateSerializer]
[Alias(nameof(TradingPair))]
public class TradingPair
{
    [Id(0)]
    public string Id { get; set; } = string.Empty;
    [Id(1)]
    public string Symbol { get; set; } = string.Empty; // BTCUSDT, ETHUSDT
    [Id(2)]
    public string BaseAsset { get; set; } = string.Empty; // BTC, ETH
    [Id(3)]
    public string QuoteAsset { get; set; } = string.Empty; // USDT, BUSD
    [Id(4)]
    public ExchangeType Exchange { get; set; }
    [Id(5)]
    public TradingPairType Type { get; set; }
    [Id(6)]
    public TradingPairStatus Status { get; set; }
    [Id(7)]
    public DateTime CreatedAt { get; set; }
    [Id(8)]
    public DateTime? LastUpdated { get; set; }
    
    // Trading Configuration
    [Id(9)]
    public TradingConfig Config { get; set; } = new();
    
    // Market Info
    [Id(10)]
    public decimal? LastPrice { get; set; }
    [Id(11)]
    public decimal? PriceChange24h { get; set; }
    [Id(12)]
    public decimal? Volume24h { get; set; }
    [Id(13)]
    public bool IsActive { get; set; } = true;
}

public enum ExchangeType
{
    Bybit = 1,
    Binance = 2,
    OKX = 3,
    Mock = 99 // For testing
}

public enum TradingPairType
{
    Spot = 1,
    Futures = 2,
    Options = 3
}

public enum TradingPairStatus
{
    Inactive = 0,           // Не активна
    DataCollection = 1,     // Сбор данных
    Training = 2,           // Обучение модели
    PaperTrading = 3,       // Тестовая торговля
    ReadyForLive = 4,       // Готова к реальной торговле
    LiveTrading = 5,        // Реальная торговля
    Paused = 6,            // Приостановлена
    Error = 7              // Ошибка
}

/// <summary>
/// Конфигурация торговой пары
/// </summary>
[GenerateSerializer]
[Alias(nameof(TradingConfig))]
public class TradingConfig
{
    // Risk Management
    [Id(0)]
    public decimal RiskPercentage { get; set; } = 0.10m; // 10% риск по умолчанию
    [Id(1)]
    public decimal MaxPositionSize { get; set; } = 1000m; // $1000 максимум
    [Id(2)]
    public decimal StopLossPercentage { get; set; } = 0.02m; // 2% stop loss
    
    // Strategy Settings
    [Id(3)]
    public List<StrategyType> EnabledStrategies { get; set; } = new();
    [Id(4)]
    public bool AutoTransitionToLive { get; set; } = false;
    
    // Paper Trading Requirements
    [Id(5)]
    public int MinTradingDays { get; set; } = 30;
    [Id(6)]
    public decimal MinSharpeRatio { get; set; } = 1.5m;
    [Id(7)]
    public decimal MaxDrawdownPercentage { get; set; } = 0.15m; // 15%
    
    // ML Settings
    [Id(8)]
    public int HistoricalDataDays { get; set; } = 90; // 3 месяца по умолчанию
    [Id(9)]
    public List<TimeFrame> EnabledTimeFrames { get; set; } = new() { TimeFrame.M5, TimeFrame.M15, TimeFrame.H1 };
    
    [Id(10)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Id(11)]
    public DateTime? UpdatedAt { get; set; }
}

public enum StrategyType
{
    Scalping = 1,
    SwingTrading = 2,
    MeanReversion = 3,
    Momentum = 4,
    Arbitrage = 5
}

public enum TimeFrame
{
    M1 = 1,    // 1 minute
    M5 = 5,    // 5 minutes  
    M15 = 15,  // 15 minutes
    M30 = 30,  // 30 minutes
    H1 = 60,   // 1 hour
    H4 = 240,  // 4 hours
    D1 = 1440  // 1 day
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

// Модели для получения тикеров (рыночных данных)

[GenerateSerializer]
[Alias(nameof(TickerResponse))]
public class TickerResponse
{
    [Id(0)]
    public int RetCode { get; set; }
    [Id(1)]
    public string RetMsg { get; set; } = string.Empty;
    [Id(2)]
    public TickerResult Result { get; set; } = new();
}

[GenerateSerializer]
[Alias(nameof(TickerResult))]
public class TickerResult
{
    [Id(0)]
    public string Category { get; set; } = string.Empty;
    [Id(1)]
    public List<BybitTicker> List { get; set; } = [];
}

[GenerateSerializer]
[Alias(nameof(BybitTicker))]
public class BybitTicker
{
    [Id(0)]
    public string Symbol { get; set; } = string.Empty;
    [Id(1)]
    public string LastPrice { get; set; } = string.Empty;
    [Id(2)]
    public string PrevPrice24h { get; set; } = string.Empty;
    [Id(3)]
    public string Price24hPcnt { get; set; } = string.Empty;
    [Id(4)]
    public string HighPrice24h { get; set; } = string.Empty;
    [Id(5)]
    public string LowPrice24h { get; set; } = string.Empty;
    [Id(6)]
    public string Volume24h { get; set; } = string.Empty;
    [Id(7)]
    public string Turnover24h { get; set; } = string.Empty;
}