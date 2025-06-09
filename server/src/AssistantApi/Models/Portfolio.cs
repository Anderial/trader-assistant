namespace AssistantApi.Models;

public record PortfolioOverview
{
    public decimal FuturesBalance { get; init; }
    public decimal SpotBalance { get; init; }
    public decimal TotalBalance { get; init; }
    public decimal UsedMargin { get; init; }
    public decimal UnrealizedPnL { get; init; }
    public int ActivePositions { get; init; }
    public decimal MarginRatio { get; init; }
}

public record FuturesPosition
{
    public string Symbol { get; init; } = string.Empty;
    public string ContractType { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty; // LONG/SHORT
    public string Size { get; init; } = string.Empty;
    public decimal EntryPrice { get; init; }
    public decimal CurrentPrice { get; init; }
    public decimal MarkPrice { get; init; }
    public string Leverage { get; init; } = string.Empty;
    public decimal MarginUsed { get; init; }
    public decimal LiquidationPrice { get; init; }
    public string FundingRate { get; init; } = string.Empty;
    public string NextFunding { get; init; } = string.Empty;
    public decimal PnL { get; init; }
    public decimal PnLPercent { get; init; }
    public string Duration { get; init; } = string.Empty;
    public decimal Confidence { get; init; }
    public bool IsProfit { get; init; }
}

public record SpotPosition
{
    public string Symbol { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Amount { get; init; } = string.Empty;
    public decimal AvgBuyPrice { get; init; }
    public decimal CurrentPrice { get; init; }
    public decimal TotalValue { get; init; }
    public decimal PnL { get; init; }
    public decimal PnLPercent { get; init; }
    public decimal Allocation { get; init; }
    public bool IsProfit { get; init; }
}