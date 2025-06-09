using AssistantApi.Models;

namespace AssistantApi.Services;

public interface IMockDataService
{
    PortfolioOverview GetPortfolioOverview();
    List<FuturesPosition> GetFuturesPositions();
    List<SpotPosition> GetSpotPositions();
    List<Trade> GetRecentTrades(int limit = 50, int offset = 0);
    SystemStatus GetSystemStatus();
    PerformanceMetrics GetPerformanceMetrics();
    BalanceChartData GetBalanceChartData(string period = "24h");
    TradingMode GetTradingMode();
}

public class MockDataService : IMockDataService
{
    private string _currentTradingMode = "paper";

    public PortfolioOverview GetPortfolioOverview()
    {
        return new PortfolioOverview
        {
            FuturesBalance = 8247.85m,
            SpotBalance = 15220.79m,
            TotalBalance = 23468.64m,
            UsedMargin = 1581.13m,
            UnrealizedPnL = 287.42m,
            ActivePositions = 7,
            MarginRatio = 15.5m
        };
    }

    public List<FuturesPosition> GetFuturesPositions()
    {
        return new List<FuturesPosition>
        {
            new()
            {
                Symbol = "BTCUSDT",
                ContractType = "PERPETUAL",
                Direction = "LONG",
                Size = "0.25 BTC",
                EntryPrice = 43250.00m,
                CurrentPrice = 43485.00m,
                MarkPrice = 43482.50m,
                Leverage = "10x",
                MarginUsed = 1087.25m,
                LiquidationPrice = 39325.00m,
                FundingRate = "+0.0125%",
                NextFunding = "2ч 15м",
                PnL = 58.75m,
                PnLPercent = 0.54m,
                Duration = "12 мин",
                Confidence = 0.82m,
                IsProfit = true
            },
            new()
            {
                Symbol = "ETHUSDT",
                ContractType = "PERPETUAL",
                Direction = "SHORT",
                Size = "2.5 ETH",
                EntryPrice = 2520.00m,
                CurrentPrice = 2485.30m,
                MarkPrice = 2486.15m,
                Leverage = "15x",
                MarginUsed = 420.00m,
                LiquidationPrice = 2687.20m,
                FundingRate = "-0.0089%",
                NextFunding = "2ч 15м",
                PnL = 86.75m,
                PnLPercent = 1.38m,
                Duration = "45 мин",
                Confidence = 0.76m,
                IsProfit = true
            },
            new()
            {
                Symbol = "SOLUSDT",
                ContractType = "PERPETUAL",
                Direction = "LONG",
                Size = "15 SOL",
                EntryPrice = 98.50m,
                CurrentPrice = 97.25m,
                MarkPrice = 97.28m,
                Leverage = "20x",
                MarginUsed = 73.88m,
                LiquidationPrice = 93.58m,
                FundingRate = "+0.0245%",
                NextFunding = "2ч 15м",
                PnL = -18.75m,
                PnLPercent = -1.27m,
                Duration = "2ч 15м",
                Confidence = 0.65m,
                IsProfit = false
            }
        };
    }

    public List<SpotPosition> GetSpotPositions()
    {
        return new List<SpotPosition>
        {
            new()
            {
                Symbol = "BTC",
                FullName = "Bitcoin",
                Amount = "0.15 BTC",
                AvgBuyPrice = 42850.00m,
                CurrentPrice = 43485.00m,
                TotalValue = 6522.75m,
                PnL = 95.25m,
                PnLPercent = 1.48m,
                Allocation = 63.7m,
                IsProfit = true
            },
            new()
            {
                Symbol = "ETH",
                FullName = "Ethereum",
                Amount = "1.8 ETH",
                AvgBuyPrice = 2456.00m,
                CurrentPrice = 2485.30m,
                TotalValue = 4473.54m,
                PnL = 52.74m,
                PnLPercent = 1.19m,
                Allocation = 43.7m,
                IsProfit = true
            },
            new()
            {
                Symbol = "SOL",
                FullName = "Solana",
                Amount = "25 SOL",
                AvgBuyPrice = 99.20m,
                CurrentPrice = 97.25m,
                TotalValue = 2431.25m,
                PnL = -48.75m,
                PnLPercent = -1.97m,
                Allocation = 23.7m,
                IsProfit = false
            },
            new()
            {
                Symbol = "AVAX",
                FullName = "Avalanche",
                Amount = "45 AVAX",
                AvgBuyPrice = 38.50m,
                CurrentPrice = 39.85m,
                TotalValue = 1793.25m,
                PnL = 60.75m,
                PnLPercent = 3.51m,
                Allocation = 17.5m,
                IsProfit = true
            }
        };
    }

    public List<Trade> GetRecentTrades(int limit = 50, int offset = 0)
    {
        var allTrades = new List<Trade>
        {
            new()
            {
                Id = "TRD_20240115_001",
                Type = "futures",
                TypeName = "Фьючерс",
                Symbol = "BTCUSDT",
                Direction = "LONG",
                Leverage = "10x",
                Size = "0.30 BTC",
                EntryPrice = 42850.00m,
                ExitPrice = 43120.00m,
                InitialStake = 1285.50m,
                FinalStake = 1366.50m,
                NetProfit = 81.00m,
                GrossPnL = 81.00m,
                Fees = -2.50m,
                PnLPercent = 0.63m,
                Duration = "1ч 23м",
                ExitReason = "Take Profit",
                Timestamp = "14:32:15",
                Confidence = 0.78m,
                IsProfit = true
            },
            new()
            {
                Id = "TRD_20240115_002",
                Type = "futures",
                TypeName = "Фьючерс",
                Symbol = "ETHUSDT",
                Direction = "SHORT",
                Leverage = "15x",
                Size = "2.0 ETH",
                EntryPrice = 2567.50m,
                ExitPrice = 2520.00m,
                InitialStake = 342.33m,
                FinalStake = 437.33m,
                NetProfit = 95.00m,
                GrossPnL = 95.00m,
                Fees = -1.80m,
                PnLPercent = 1.85m,
                Duration = "2ч 45м",
                ExitReason = "Model Signal",
                Timestamp = "12:15:30",
                Confidence = 0.84m,
                IsProfit = true
            },
            new()
            {
                Id = "TRD_20240115_003",
                Type = "spot",
                TypeName = "Спот",
                Symbol = "ADA",
                Direction = "SELL",
                Leverage = "1x",
                Size = "1000 ADA",
                EntryPrice = 0.4850m,
                ExitPrice = 0.4795m,
                InitialStake = 485.00m,
                FinalStake = 479.50m,
                NetProfit = -5.50m,
                GrossPnL = -5.50m,
                Fees = -0.96m,
                PnLPercent = -1.13m,
                Duration = "35м",
                ExitReason = "Stop Loss",
                Timestamp = "11:45:22",
                Confidence = 0.62m,
                IsProfit = false
            }
        };

        return allTrades.Skip(offset).Take(limit).ToList();
    }

    public SystemStatus GetSystemStatus()
    {
        return new SystemStatus
        {
            WebSocketConnections = new WebSocketConnections
            {
                Active = 8,
                Total = 8,
                Status = "healthy",
                Latency = "12ms"
            },
            Database = new Database
            {
                Status = "connected",
                Latency = "2ms",
                Uptime = "99.98%"
            },
            BybitApi = new BybitApi
            {
                Status = "healthy",
                RateLimit = 85,
                MaxRate = 100,
                ResponseTime = "45ms"
            },
            MLModels = new MLModels
            {
                Active = 4,
                Total = 4,
                Accuracy = 0.673m,
                LastUpdate = "5 мин назад"
            },
            System = new SystemInfo
            {
                CpuUsage = 15.2m,
                MemoryUsage = 1.2m,
                MaxMemory = 4.0m,
                Uptime = "5д 12ч 30м"
            }
        };
    }

    public PerformanceMetrics GetPerformanceMetrics()
    {
        return new PerformanceMetrics
        {
            TotalReturn = 12.4m,
            WinRate = 67.3m,
            SharpeRatio = 1.85m,
            MaxDrawdown = -3.2m,
            TotalTrades = 186,
            AvgDuration = "2ч 15м"
        };
    }

    public BalanceChartData GetBalanceChartData(string period = "24h")
    {
        return new BalanceChartData
        {
            Labels = new[] { "00:00", "02:00", "04:00", "06:00", "08:00", "10:00", "12:00", "14:00", "16:00", "18:00", "20:00", "22:00", "24:00" },
            Datasets = new[]
            {
                new BalanceDataset
                {
                    Label = "Баланс портфеля",
                    Data = new[] { 10000m, 10015m, 9995m, 10025m, 10080m, 10125m, 10095m, 10150m, 10180m, 10220m, 10195m, 10235m, 10247m },
                    BorderColor = "#00d4aa",
                    BackgroundColor = "rgba(0, 212, 170, 0.1)",
                    BorderWidth = 2,
                    Fill = true,
                    Tension = 0.4m
                }
            }
        };
    }

    public TradingMode GetTradingMode()
    {
        return new TradingMode
        {
            Current = _currentTradingMode,
            PaperTradingStats = _currentTradingMode == "paper" ? new PaperTradingStats
            {
                TotalSimulatedTrades = 186,
                SimulatedBalance = 10247.85m,
                SimulatedProfit = 1247.85m,
                TestingDuration = "30 дней"
            } : null
        };
    }

    public void SetTradingMode(string mode)
    {
        _currentTradingMode = mode;
    }
}