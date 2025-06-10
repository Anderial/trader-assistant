using Orleans;

namespace TradeService.Models;

/// <summary>
/// Базовая команда для стримов
/// </summary>
[GenerateSerializer]
[Alias("TradeService.Models.StreamCommand")]
public abstract class StreamCommand
{
    [Id(0)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Id(1)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [Id(2)]
    public string CommandType { get; set; } = string.Empty;
}

/// <summary>
/// Команда запуска анализа через стрим
/// </summary>
[GenerateSerializer]
[Alias("TradeService.Models.StartAnalysisStreamCommand")]
public class StartAnalysisStreamCommand : StreamCommand
{
    [Id(3)]
    public string Symbol { get; set; } = string.Empty;
    
    [Id(4)]
    public TradingPairType Type { get; set; }
    
    public StartAnalysisStreamCommand()
    {
        CommandType = "StartAnalysisStream";
    }
}

/// <summary>
/// Команда остановки анализа через стрим
/// </summary>
[GenerateSerializer]
[Alias("TradeService.Models.StopAnalysisStreamCommand")]
public class StopAnalysisStreamCommand : StreamCommand
{
    [Id(3)]
    public string PairKey { get; set; } = string.Empty;
    
    public StopAnalysisStreamCommand()
    {
        CommandType = "StopAnalysisStream";
    }
}

/// <summary>
/// Данные WebSocket от Bybit
/// </summary>
[GenerateSerializer]
[Alias("TradeService.Models.BybitWebSocketData")]
public class BybitWebSocketData
{
    [Id(0)]
    public string Topic { get; set; } = string.Empty;
    
    [Id(1)]
    public string Type { get; set; } = string.Empty;
    
    [Id(2)]
    public long Timestamp { get; set; }
    
    [Id(3)]
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// Ticker данные от Bybit WebSocket
/// </summary>
[GenerateSerializer]
[Alias("TradeService.Models.BybitTickerData")]
public class BybitTickerData
{
    [Id(0)]
    public string Symbol { get; set; } = string.Empty;
    
    [Id(1)]
    public decimal LastPrice { get; set; }
    
    [Id(2)]
    public decimal Volume24h { get; set; }
    
    [Id(3)]
    public decimal PriceChange24h { get; set; }
    
    [Id(4)]
    public decimal PriceChangePercent24h { get; set; }
    
    [Id(5)]
    public decimal HighPrice24h { get; set; }
    
    [Id(6)]
    public decimal LowPrice24h { get; set; }
    
    [Id(7)]
    public long Timestamp { get; set; }
} 