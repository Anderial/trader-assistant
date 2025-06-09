namespace AssistantApi.Models;

public record Trade
{
    public string Id { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty; // futures/spot
    public string TypeName { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty; // LONG/SHORT/BUY/SELL
    public string Leverage { get; init; } = string.Empty;
    public string Size { get; init; } = string.Empty;
    public decimal EntryPrice { get; init; }
    public decimal ExitPrice { get; init; }
    public decimal InitialStake { get; init; }
    public decimal FinalStake { get; init; }
    public decimal NetProfit { get; init; }
    public decimal GrossPnL { get; init; }
    public decimal Fees { get; init; }
    public decimal PnLPercent { get; init; }
    public string Duration { get; init; } = string.Empty;
    public string ExitReason { get; init; } = string.Empty;
    public string Timestamp { get; init; } = string.Empty;
    public decimal Confidence { get; init; }
    public bool IsProfit { get; init; }
}

public record TradingMode
{
    public string Current { get; init; } = string.Empty; // paper/live
    public PaperTradingStats? PaperTradingStats { get; init; }
}

public record PaperTradingStats
{
    public int TotalSimulatedTrades { get; init; }
    public decimal SimulatedBalance { get; init; }
    public decimal SimulatedProfit { get; init; }
    public string TestingDuration { get; init; } = string.Empty;
}

public record SwitchModeRequest
{
    public string Mode { get; init; } = string.Empty; // paper/live
    public bool Confirmation { get; init; }
}

public record ClosePositionRequest
{
    public string Type { get; init; } = string.Empty; // futures/spot
    public string Symbol { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}