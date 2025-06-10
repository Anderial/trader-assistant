using Orleans;
using Orleans.Streams;
using Orleans.Streams.Core;
using DistributedKit.Constants;
using TradeService.Models;
using TradeService.Services;

namespace TradeService.Grains;

/// <summary>
/// Grain для работы с отдельной торговой парой с поддержкой Orleans Streams и WebSocket
/// </summary>
public class TradingPairGrain : Grain, ITradingPairGrain, IStreamSubscriptionObserver
{
    private readonly IBybitApiService _bybitApiService;
    private readonly IWebSocketManager _webSocketManager;
    private readonly ILogger<TradingPairGrain> _logger;
    
    private TradingPair? _tradingPair;
    private TradingPairMarketData? _cachedMarketData;
    private DateTime? _lastUpdateTime;
    
    // Анализ цен
    private AnalysisStatus _analysisStatus = AnalysisStatus.Stopped;
    private DateTime? _analysisStartedAt;
    private List<PriceTickData> _priceHistory = new();
    private readonly int _maxHistorySize = 10000; // Максимум 10к точек в памяти
    
    // Orleans Streams
    private IAsyncStream<StreamCommand>? _commandStream;
    private StreamSubscriptionHandle<StreamCommand>? _commandSubscription;

    public TradingPairGrain(
        IBybitApiService bybitApiService,
        IWebSocketManager webSocketManager,
        ILogger<TradingPairGrain> logger)
    {
        _bybitApiService = bybitApiService;
        _webSocketManager = webSocketManager;
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);
        
        try
        {
            // Подписываемся на стрим команд для этого grain
            var streamProvider = this.GetStreamProvider(StreamConstants.InMemoryStreamProvider);
            var grainKey = this.GetPrimaryKeyString();
            _commandStream = streamProvider.GetStream<StreamCommand>("analysis-commands", grainKey);
            
            _commandSubscription = await _commandStream.SubscribeAsync(OnCommandReceived);
            
            _logger.LogInformation("TradingPairGrain activated and subscribed to command stream for key: {Key}", grainKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate TradingPairGrain for key: {Key}", this.GetPrimaryKeyString());
            throw;
        }
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        try
        {
            // Останавливаем анализ, если он запущен
            if (_analysisStatus == AnalysisStatus.Running)
            {
                await StopAnalysisInternal();
            }
            
            // Отписываемся от стрима команд
            if (_commandSubscription != null)
            {
                await _commandSubscription.UnsubscribeAsync();
                _commandSubscription = null;
            }
            
            _logger.LogInformation("TradingPairGrain deactivated for key: {Key}, reason: {Reason}", 
                this.GetPrimaryKeyString(), reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during TradingPairGrain deactivation for key: {Key}", this.GetPrimaryKeyString());
        }
        
        await base.OnDeactivateAsync(reason, cancellationToken);
    }

    /// <summary>
    /// Обработчик команд из Orleans Stream
    /// </summary>
    private async Task OnCommandReceived(StreamCommand command, StreamSequenceToken? token)
    {
        try
        {
            _logger.LogInformation("Received command {CommandType} for grain {Key}", 
                command.CommandType, this.GetPrimaryKeyString());

            switch (command)
            {
                case StartAnalysisStreamCommand startCmd:
                    await HandleStartAnalysisCommand(startCmd);
                    break;
                    
                case StopAnalysisStreamCommand stopCmd:
                    await HandleStopAnalysisCommand(stopCmd);
                    break;
                    
                default:
                    _logger.LogWarning("Unknown command type: {CommandType}", command.CommandType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command {CommandType} for grain {Key}", 
                command.CommandType, this.GetPrimaryKeyString());
        }
    }

    private async Task HandleStartAnalysisCommand(StartAnalysisStreamCommand command)
    {
        if (_analysisStatus == AnalysisStatus.Running)
        {
            _logger.LogWarning("Analysis already running for {Symbol}", command.Symbol);
            return;
        }

        // Инициализируем торговую пару, если не инициализирована
        if (_tradingPair == null || _tradingPair.Symbol != command.Symbol || _tradingPair.Type != command.Type)
        {
            _tradingPair = new TradingPair
            {
                Symbol = command.Symbol,
                Type = command.Type,
                BaseAsset = ExtractBaseAsset(command.Symbol),
                QuoteAsset = ExtractQuoteAsset(command.Symbol),
                IsActive = true
            };
        }

        await StartAnalysisInternal();
    }

    private async Task HandleStopAnalysisCommand(StopAnalysisStreamCommand command)
    {
        if (_analysisStatus != AnalysisStatus.Running)
        {
            _logger.LogInformation("Analysis not running for {PairKey}", command.PairKey);
            return;
        }

        await StopAnalysisInternal();
    }

    private async Task StartAnalysisInternal()
    {
        try
        {
            _analysisStatus = AnalysisStatus.Starting;
            _analysisStartedAt = DateTime.UtcNow;
            _priceHistory.Clear();

            _logger.LogInformation("Starting price analysis for {Symbol}:{Type}", 
                _tradingPair?.Symbol, _tradingPair?.Type);

            // Убеждаемся, что WebSocket подключен
            var webSocketService = _webSocketManager.GetWebSocketService();
            if (!webSocketService.IsConnected)
            {
                await _webSocketManager.StartAsync();
            }

            // Подписываемся на ticker данные
            var success = await webSocketService.SubscribeToTickerAsync(
                _tradingPair!.Symbol, 
                _tradingPair.Type, 
                OnTickerDataReceived);

            if (success)
            {
                _analysisStatus = AnalysisStatus.Running;
                _logger.LogInformation("Successfully started analysis for {Symbol}:{Type}", 
                    _tradingPair.Symbol, _tradingPair.Type);
            }
            else
            {
                _analysisStatus = AnalysisStatus.Error;
                _logger.LogError("Failed to subscribe to ticker for {Symbol}:{Type}", 
                    _tradingPair.Symbol, _tradingPair.Type);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start analysis for {Symbol}", _tradingPair?.Symbol);
            _analysisStatus = AnalysisStatus.Error;
        }
    }

    private async Task StopAnalysisInternal()
    {
        try
        {
            if (_analysisStatus == AnalysisStatus.Stopped)
            {
                return;
            }

            _logger.LogInformation("Stopping price analysis for {Symbol}:{Type}", 
                _tradingPair?.Symbol, _tradingPair?.Type);

            _analysisStatus = AnalysisStatus.Stopping;
            
            // Отписываемся от WebSocket
            if (_tradingPair != null)
            {
                var webSocketService = _webSocketManager.GetWebSocketService();
                await webSocketService.UnsubscribeFromTickerAsync(_tradingPair.Symbol, _tradingPair.Type);
            }
            
            _analysisStatus = AnalysisStatus.Stopped;
            
            _logger.LogInformation("Successfully stopped analysis for {Symbol}:{Type}", 
                _tradingPair?.Symbol, _tradingPair?.Type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop analysis for {Symbol}", _tradingPair?.Symbol);
            _analysisStatus = AnalysisStatus.Error;
        }
    }

    /// <summary>
    /// Обработчик ticker данных от WebSocket
    /// </summary>
    private async Task OnTickerDataReceived(BybitTickerData tickerData)
    {
        try
        {
            if (_analysisStatus != AnalysisStatus.Running)
            {
                return;
            }

            var priceTickData = new PriceTickData
            {
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(tickerData.Timestamp).DateTime,
                Price = tickerData.LastPrice,
                Volume = tickerData.Volume24h,
                PriceChange24h = tickerData.PriceChange24h,
                PriceChangePercent24h = tickerData.PriceChangePercent24h
            };

            // Добавляем в историю
            _priceHistory.Add(priceTickData);
            
            // Ограничиваем размер истории
            if (_priceHistory.Count > _maxHistorySize)
            {
                var removeCount = _priceHistory.Count - _maxHistorySize;
                _priceHistory.RemoveRange(0, removeCount);
            }

            _logger.LogDebug("Added tick data for {Symbol}: Price={Price}, Volume={Volume}", 
                _tradingPair?.Symbol, priceTickData.Price, priceTickData.Volume);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ticker data for {Symbol}", _tradingPair?.Symbol);
        }

        await Task.CompletedTask;
    }

    // === Публичные методы для работы с рыночными данными ===

    public async Task<TradingPairMarketData> GetMarketDataAsync()
    {
        try
        {
            if (_tradingPair == null)
            {
                _logger.LogWarning("Trading pair not initialized for grain key: {Key}", this.GetPrimaryKeyString());
                return CreateEmptyMarketData();
            }

            _logger.LogInformation("Getting market data for {Symbol}:{Type}", _tradingPair.Symbol, _tradingPair.Type);

            // Получаем рыночные данные с биржи
            var marketData = await _bybitApiService.GetMarketDataAsync(_tradingPair.Symbol, _tradingPair.Type);
            
            if (marketData != null)
            {
                _cachedMarketData = marketData;
                _lastUpdateTime = DateTime.UtcNow;
                
                _logger.LogInformation("Successfully updated market data for {Symbol}", _tradingPair.Symbol);
            }
            else
            {
                _logger.LogWarning("Failed to get market data for {Symbol}:{Type}", _tradingPair.Symbol, _tradingPair.Type);
                
                // Возвращаем кэшированные данные или пустые
                marketData = _cachedMarketData ?? CreateEmptyMarketData();
            }

            return marketData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting market data for {Symbol}:{Type}", 
                _tradingPair?.Symbol, _tradingPair?.Type);
            
            return _cachedMarketData ?? CreateEmptyMarketData();
        }
    }

    public Task InitializeAsync(TradingPair tradingPair)
    {
        _tradingPair = tradingPair;
        _logger.LogInformation("Initialized trading pair grain for {Symbol}:{Type}", 
            tradingPair.Symbol, tradingPair.Type);
        
        return Task.CompletedTask;
    }

    public Task<DateTime?> GetLastUpdateTimeAsync()
    {
        return Task.FromResult(_lastUpdateTime);
    }

    // === Публичные методы анализа (вызываются через API, отправляют команды в стрим) ===

    public async Task<bool> StartPriceAnalysisAsync()
    {
        try
        {
            if (_tradingPair == null)
            {
                _logger.LogWarning("Cannot start analysis - trading pair not initialized");
                return false;
            }

            var command = new StartAnalysisStreamCommand
            {
                Symbol = _tradingPair.Symbol,
                Type = _tradingPair.Type
            };

            if (_commandStream != null)
            {
                await _commandStream.OnNextAsync(command);
                _logger.LogInformation("Sent start analysis command to stream for {Symbol}:{Type}", 
                    _tradingPair.Symbol, _tradingPair.Type);
                return true;
            }
            else
            {
                _logger.LogError("Command stream not initialized");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send start analysis command for {Symbol}", _tradingPair?.Symbol);
            return false;
        }
    }

    public async Task<bool> StopPriceAnalysisAsync()
    {
        try
        {
            var pairKey = _tradingPair != null 
                ? $"{_tradingPair.Symbol}:{_tradingPair.Type}" 
                : this.GetPrimaryKeyString();

            var command = new StopAnalysisStreamCommand
            {
                PairKey = pairKey
            };

            if (_commandStream != null)
            {
                await _commandStream.OnNextAsync(command);
                _logger.LogInformation("Sent stop analysis command to stream for {PairKey}", pairKey);
                return true;
            }
            else
            {
                _logger.LogError("Command stream not initialized");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send stop analysis command for {Symbol}", _tradingPair?.Symbol);
            return false;
        }
    }

    public Task<PairAnalysisInfo> GetAnalysisInfoAsync()
    {
        var pairKey = _tradingPair != null 
            ? $"{_tradingPair.Symbol}:{_tradingPair.Type}" 
            : "";

        var info = new PairAnalysisInfo
        {
            PairKey = pairKey,
            Symbol = _tradingPair?.Symbol ?? "",
            Type = _tradingPair?.Type ?? TradingPairType.Spot,
            Status = _analysisStatus,
            StartedAt = _analysisStartedAt ?? DateTime.MinValue,
            LastUpdateAt = _priceHistory.LastOrDefault()?.Timestamp,
            DataPointsCollected = _priceHistory.Count,
            CurrentPrice = _priceHistory.LastOrDefault()?.Price,
            PriceChangePercent = _priceHistory.LastOrDefault()?.PriceChangePercent24h
        };

        return Task.FromResult(info);
    }

    public Task<PriceAnalysisDetails> GetAnalysisDetailsAsync(DateTime fromTime, DateTime toTime)
    {
        var pairKey = _tradingPair != null 
            ? $"{_tradingPair.Symbol}:{_tradingPair.Type}" 
            : "";

        var filteredTicks = _priceHistory
            .Where(t => t.Timestamp >= fromTime && t.Timestamp <= toTime)
            .OrderBy(t => t.Timestamp)
            .ToList();

        var details = new PriceAnalysisDetails
        {
            PairKey = pairKey,
            FromTime = fromTime,
            ToTime = toTime,
            PriceTicks = filteredTicks,
            TickCount = filteredTicks.Count
        };

        if (filteredTicks.Count > 0)
        {
            details.MinPrice = filteredTicks.Min(t => t.Price);
            details.MaxPrice = filteredTicks.Max(t => t.Price);
            details.AveragePrice = filteredTicks.Average(t => t.Price);
            details.TotalVolume = filteredTicks.Sum(t => t.Volume);
        }

        return Task.FromResult(details);
    }

    public Task<bool> IsAnalysisRunningAsync()
    {
        return Task.FromResult(_analysisStatus == AnalysisStatus.Running);
    }

    // === Вспомогательные методы ===

    private TradingPairMarketData CreateEmptyMarketData()
    {
        return new TradingPairMarketData
        {
            Symbol = _tradingPair?.Symbol ?? string.Empty,
            Type = _tradingPair?.Type ?? TradingPairType.Spot,
            BaseAsset = _tradingPair?.BaseAsset ?? string.Empty,
            QuoteAsset = _tradingPair?.QuoteAsset ?? string.Empty,
            IsActive = _tradingPair?.IsActive ?? false,
            LastUpdateTime = _lastUpdateTime,
            CurrentPrice = null,
            Volume24h = null,
            PriceChange24h = null,
            PriceChangePercent24h = null,
            HighPrice24h = null,
            LowPrice24h = null
        };
    }

    private string ExtractBaseAsset(string symbol)
    {
        // Простая логика для извлечения базового актива из символа
        // Например, для BTCUSDT возвращает BTC
        return symbol.Length > 3 ? symbol[..^4] : symbol;
    }

    private string ExtractQuoteAsset(string symbol)
    {
        // Простая логика для извлечения квотируемого актива из символа
        // Например, для BTCUSDT возвращает USDT
        return symbol.Length > 4 ? symbol[^4..] : "USDT";
    }

    // === IStreamSubscriptionObserver ===
    public Task OnCompletedAsync()
    {
        _logger.LogInformation("Command stream completed for grain {Key}", this.GetPrimaryKeyString());
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        _logger.LogError(ex, "Command stream error for grain {Key}", this.GetPrimaryKeyString());
        return Task.CompletedTask;
    }

    public Task OnNextAsync(object item, StreamSequenceToken? token = null)
    {
        // Не используется, так как мы подписываемся через SubscribeAsync с типизированным обработчиком
        return Task.CompletedTask;
    }

    public Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
    {
        _logger.LogInformation("Subscribed to command stream for grain {Key}", this.GetPrimaryKeyString());
        return Task.CompletedTask;
    }
} 