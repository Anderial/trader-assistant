using TradeService.Models;

namespace TradeService.Commands;

/// <summary>
/// Команда для запуска анализа торговой пары
/// </summary>
[GenerateSerializer]
[Alias("TradeService.Commands.StartAnalysisCommand")]
public class StartAnalysisCommand : BaseCommand<bool>
{
    [Id(3)]
    public string Symbol { get; set; } = string.Empty;
    
    [Id(4)]
    public TradingPairType Type { get; set; }
    
    public StartAnalysisCommand()
    {
        CommandType = "StartAnalysis";
    }
}

/// <summary>
/// Команда для остановки анализа торговой пары
/// </summary>
[GenerateSerializer]
[Alias("TradeService.Commands.StopAnalysisCommand")]
public class StopAnalysisCommand : BaseCommand<bool>
{
    [Id(3)]
    public string PairKey { get; set; } = string.Empty;
    
    public StopAnalysisCommand()
    {
        CommandType = "StopAnalysis";
    }
}

/// <summary>
/// Команда для получения списка анализируемых пар
/// </summary>
[GenerateSerializer]
[Alias("TradeService.Commands.GetRunningAnalysisCommand")]
public class GetRunningAnalysisCommand : BaseCommand<List<PairAnalysisInfo>>
{
    public GetRunningAnalysisCommand()
    {
        CommandType = "GetRunningAnalysis";
    }
}

/// <summary>
/// Команда для получения детальных данных анализа
/// </summary>
[GenerateSerializer]
[Alias("TradeService.Commands.GetAnalysisDetailsCommand")]
public class GetAnalysisDetailsCommand : BaseCommand<PriceAnalysisDetails>
{
    [Id(3)]
    public string PairKey { get; set; } = string.Empty;
    
    [Id(4)]
    public DateTime FromTime { get; set; }
    
    [Id(5)]
    public DateTime ToTime { get; set; }
    
    public GetAnalysisDetailsCommand()
    {
        CommandType = "GetAnalysisDetails";
    }
} 