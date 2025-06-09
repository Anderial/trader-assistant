namespace AssistantApi.Models;

public record SystemStatus
{
    public WebSocketConnections WebSocketConnections { get; init; } = new();
    public Database Database { get; init; } = new();
    public BybitApi BybitApi { get; init; } = new();
    public MLModels MLModels { get; init; } = new();
    public SystemInfo System { get; init; } = new();
}

public record WebSocketConnections
{
    public int Active { get; init; }
    public int Total { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Latency { get; init; } = string.Empty;
}

public record Database
{
    public string Status { get; init; } = string.Empty;
    public string Latency { get; init; } = string.Empty;
    public string Uptime { get; init; } = string.Empty;
}

public record BybitApi
{
    public string Status { get; init; } = string.Empty;
    public int RateLimit { get; init; }
    public int MaxRate { get; init; }
    public string ResponseTime { get; init; } = string.Empty;
}

public record MLModels
{
    public int Active { get; init; }
    public int Total { get; init; }
    public decimal Accuracy { get; init; }
    public string LastUpdate { get; init; } = string.Empty;
}

public record SystemInfo
{
    public decimal CpuUsage { get; init; }
    public decimal MemoryUsage { get; init; }
    public decimal MaxMemory { get; init; }
    public string Uptime { get; init; } = string.Empty;
}

public record PerformanceMetrics
{
    public decimal TotalReturn { get; init; }
    public decimal WinRate { get; init; }
    public decimal SharpeRatio { get; init; }
    public decimal MaxDrawdown { get; init; }
    public int TotalTrades { get; init; }
    public string AvgDuration { get; init; } = string.Empty;
}

public record BalanceChartData
{
    public string[] Labels { get; init; } = Array.Empty<string>();
    public BalanceDataset[] Datasets { get; init; } = Array.Empty<BalanceDataset>();
}

public record BalanceDataset
{
    public string Label { get; init; } = string.Empty;
    public decimal[] Data { get; init; } = Array.Empty<decimal>();
    public string BorderColor { get; init; } = string.Empty;
    public string BackgroundColor { get; init; } = string.Empty;
    public int BorderWidth { get; init; }
    public bool Fill { get; init; }
    public decimal Tension { get; init; }
}