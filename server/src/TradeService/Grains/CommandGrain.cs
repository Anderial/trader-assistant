using TradeService.Commands;
using TradeService.Models;
using TradeService.Services;

namespace TradeService.Grains;

/// <summary>
/// Централизованный Grain для обработки команд пользователя
/// </summary>
public class CommandGrain : Grain, ICommandGrain
{
    private readonly IBybitApiService _bybitApiService;
    private readonly ILogger<CommandGrain> _logger;
    
    // Список активных анализов (в памяти)
    private readonly Dictionary<string, PairAnalysisInfo> _activeAnalysis = new();

    public CommandGrain(
        IBybitApiService bybitApiService,
        ILogger<CommandGrain> logger)
    {
        _bybitApiService = bybitApiService;
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CommandGrain activated");
        await base.OnActivateAsync(cancellationToken);
    }

    /// <summary>
    /// Получить все торговые пары с биржи Bybit
    /// </summary>
    public async Task<List<TradingPair>> GetTradingPairs(GetTradingPairsCommand command)
    {
        try
        {
            _logger.LogInformation("Processing GetTradingPairs command {CommandId}", command.Id);

            var allPairs = new List<TradingPair>();

            // Если указан конкретный тип, получаем только его
            if (command.PairType.HasValue)
            {
                var pairs = await _bybitApiService.GetTradingPairsAsync(command.PairType.Value);
                allPairs.AddRange(pairs);
            }
            else
            {
                // Получаем все типы торговых пар
                var spotPairs = await _bybitApiService.GetTradingPairsAsync(TradingPairType.Spot);
                var futuresPairs = await _bybitApiService.GetTradingPairsAsync(TradingPairType.Futures);
                var optionsPairs = await _bybitApiService.GetTradingPairsAsync(TradingPairType.Options);
                
                allPairs.AddRange(spotPairs);
                allPairs.AddRange(futuresPairs);  
                allPairs.AddRange(optionsPairs);
            }

            // Применяем фильтры
            var filteredPairs = allPairs.AsEnumerable();

            if (command.ActiveOnly)
            {
                filteredPairs = filteredPairs.Where(p => p.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(command.BaseAsset))
            {
                filteredPairs = filteredPairs.Where(p => 
                    p.BaseAsset.Equals(command.BaseAsset, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(command.QuoteAsset))
            {
                filteredPairs = filteredPairs.Where(p => 
                    p.QuoteAsset.Equals(command.QuoteAsset, StringComparison.OrdinalIgnoreCase));
            }

            var result = filteredPairs.ToList();

            _logger.LogInformation("GetTradingPairs command {CommandId} completed. Found {Count} pairs", 
                command.Id, result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetTradingPairs command {CommandId}", command.Id);
            throw;
        }
    }

    public async Task<bool> StartAnalysis(StartAnalysisCommand command)
    {
        try
        {
            _logger.LogInformation("Starting analysis for {Symbol}:{Type}", command.Symbol, command.Type);

            var pairKey = $"{command.Symbol}:{command.Type}";
            var tradingPairGrain = GrainFactory.GetGrain<ITradingPairGrain>(pairKey);

            // Инициализируем grain если нужно
            var tradingPair = new TradingPair
            {
                Symbol = command.Symbol,
                Type = command.Type,
                Exchange = ExchangeType.Bybit,
                IsActive = true
            };
            await tradingPairGrain.InitializeAsync(tradingPair);

            // Запускаем анализ
            var result = await tradingPairGrain.StartPriceAnalysisAsync();
            
            if (result)
            {
                // Добавляем в список активных анализов
                var analysisInfo = new PairAnalysisInfo
                {
                    PairKey = pairKey,
                    Symbol = command.Symbol,
                    Type = command.Type,
                    Status = AnalysisStatus.Running,
                    StartedAt = DateTime.UtcNow,
                    DataPointsCollected = 0
                };
                
                _activeAnalysis[pairKey] = analysisInfo;
                _logger.LogInformation("Added {PairKey} to active analysis list", pairKey);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start analysis for {Symbol}:{Type}", command.Symbol, command.Type);
            return false;
        }
    }

    public async Task<bool> StopAnalysis(StopAnalysisCommand command)
    {
        try
        {
            _logger.LogInformation("Stopping analysis for {PairKey}", command.PairKey);

            var tradingPairGrain = GrainFactory.GetGrain<ITradingPairGrain>(command.PairKey);
            var result = await tradingPairGrain.StopPriceAnalysisAsync();
            
            if (result)
            {
                // Удаляем из списка активных анализов
                if (_activeAnalysis.Remove(command.PairKey))
                {
                    _logger.LogInformation("Removed {PairKey} from active analysis list", command.PairKey);
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop analysis for {PairKey}", command.PairKey);
            return false;
        }
    }

    public async Task<List<PairAnalysisInfo>> GetRunningAnalysis(GetRunningAnalysisCommand command)
    {
        try
        {
            _logger.LogInformation("Getting running analysis list, found {Count} active analyses", _activeAnalysis.Count);
            
            // Обновляем статус каждого анализа
            var updatedAnalyses = new List<PairAnalysisInfo>();
            
            foreach (var kvp in _activeAnalysis.ToList())
            {
                try
                {
                    var tradingPairGrain = GrainFactory.GetGrain<ITradingPairGrain>(kvp.Key);
                    var currentInfo = await tradingPairGrain.GetAnalysisInfoAsync();
                    
                    // Обновляем информацию в кэше
                    _activeAnalysis[kvp.Key] = currentInfo;
                    updatedAnalyses.Add(currentInfo);
                    
                    // Если анализ остановлен, удаляем из списка
                    if (currentInfo.Status == AnalysisStatus.Stopped)
                    {
                        _activeAnalysis.Remove(kvp.Key);
                        _logger.LogInformation("Removed stopped analysis {PairKey} from active list", kvp.Key);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get analysis info for {PairKey}, removing from active list", kvp.Key);
                    _activeAnalysis.Remove(kvp.Key);
                }
            }
            
            return updatedAnalyses.Where(a => a.Status != AnalysisStatus.Stopped).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get running analysis");
            return new List<PairAnalysisInfo>();
        }
    }

    public async Task<PriceAnalysisDetails> GetAnalysisDetails(GetAnalysisDetailsCommand command)
    {
        try
        {
            _logger.LogInformation("Getting analysis details for {PairKey} from {FromTime} to {ToTime}", 
                command.PairKey, command.FromTime, command.ToTime);

            var tradingPairGrain = GrainFactory.GetGrain<ITradingPairGrain>(command.PairKey);
            return await tradingPairGrain.GetAnalysisDetailsAsync(command.FromTime, command.ToTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get analysis details for {PairKey}", command.PairKey);
            return new PriceAnalysisDetails { PairKey = command.PairKey };
        }
    }
} 