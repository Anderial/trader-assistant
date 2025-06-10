namespace TradeService.Models;

/// <summary>
/// Универсальная модель для OHLCV данных
/// </summary>
public class MarketData
{
    public string Id { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public ExchangeType Exchange { get; set; }
    public TimeFrame TimeFrame { get; set; }
    public DateTime Timestamp { get; set; }
    
    // OHLCV Data
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    
    // Additional fields for crypto
    public decimal? QuoteVolume { get; set; } // Volume в quote currency
    public int TradeCount { get; set; } // Количество сделок
    public decimal? TakerBuyVolume { get; set; } // Volume покупок агрессивных
    public decimal? TakerBuyQuoteVolume { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Модель стакана заявок
/// </summary>
public class OrderBook
{
    public string Symbol { get; set; } = string.Empty;
    public ExchangeType Exchange { get; set; }
    public DateTime Timestamp { get; set; }
    public long UpdateId { get; set; } // Для синхронизации обновлений
    
    public List<OrderBookLevel> Bids { get; set; } = new(); // Заявки на покупку
    public List<OrderBookLevel> Asks { get; set; } = new(); // Заявки на продажу
    
    // Calculated properties
    public decimal BestBid => Bids.FirstOrDefault()?.Price ?? 0;
    public decimal BestAsk => Asks.FirstOrDefault()?.Price ?? 0;
    public decimal Spread => BestAsk - BestBid;
    public decimal SpreadPercentage => BestBid > 0 ? (Spread / BestBid) * 100 : 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Уровень стакана заявок
/// </summary>
public class OrderBookLevel
{
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
}

/// <summary>
/// Модель отдельной сделки (Trade/Execution)
/// </summary>
public class Trade
{
    public string Id { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public ExchangeType Exchange { get; set; }
    public DateTime Timestamp { get; set; }
    
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public decimal QuoteQuantity { get; set; }
    public TradeSide Side { get; set; }
    public bool IsBuyerMaker { get; set; } // Покупатель был maker
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum TradeSide
{
    Buy = 1,
    Sell = 2
}

/// <summary>
/// Агрегированные данные для анализа
/// </summary>
public class MarketDataSnapshot
{
    public string Symbol { get; set; } = string.Empty;
    public ExchangeType Exchange { get; set; }
    public DateTime Timestamp { get; set; }
    
    // Current prices
    public decimal LastPrice { get; set; }
    public decimal BestBid { get; set; }
    public decimal BestAsk { get; set; }
    
    // 24h statistics  
    public decimal PriceChange24h { get; set; }
    public decimal PriceChangePercent24h { get; set; }
    public decimal Volume24h { get; set; }
    public decimal QuoteVolume24h { get; set; }
    public decimal High24h { get; set; }
    public decimal Low24h { get; set; }
    
    // Market microstructure
    public decimal WeightedAvgPrice { get; set; }
    public int TradeCount24h { get; set; }
    public decimal BidQueueDepth { get; set; } // Объём заявок на покупку (топ 10 уровней)
    public decimal AskQueueDepth { get; set; } // Объём заявок на продажу (топ 10 уровней)
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Диапазон времени для запросов данных
/// </summary>
public class TimeRange
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeFrame TimeFrame { get; set; }
    
    public TimeRange() { }
    
    public TimeRange(DateTime start, DateTime end, TimeFrame timeFrame)
    {
        StartTime = start;
        EndTime = end;
        TimeFrame = timeFrame;
    }
    
    /// <summary>
    /// Создать диапазон "последние N дней"
    /// </summary>
    public static TimeRange LastDays(int days, TimeFrame timeFrame = TimeFrame.H1)
    {
        var end = DateTime.UtcNow;
        var start = end.AddDays(-days);
        return new TimeRange(start, end, timeFrame);
    }
    
    /// <summary>
    /// Проверить валидность диапазона
    /// </summary>
    public bool IsValid => StartTime < EndTime && EndTime <= DateTime.UtcNow;
    
    /// <summary>
    /// Получить количество ожидаемых свечей
    /// </summary>
    public int GetExpectedCandleCount()
    {
        var duration = EndTime - StartTime;
        var minutesPerCandle = (int)TimeFrame;
        return (int)(duration.TotalMinutes / minutesPerCandle);
    }
} 