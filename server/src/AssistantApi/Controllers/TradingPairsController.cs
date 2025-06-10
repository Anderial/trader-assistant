using Microsoft.AspNetCore.Mvc;
using AssistantApi.Contracts;
using TradeService.Models;
using TradeService.Grains;
using TradeService.Commands;

namespace AssistantApi.Controllers;

[ApiController]
[Route("api/trading-pairs")]
public class TradingPairsController : ControllerBase
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<TradingPairsController> _logger;

    public TradingPairsController(IClusterClient clusterClient, ILogger<TradingPairsController> logger)
    {
        _clusterClient = clusterClient;
        _logger = logger;
    }

    /// <summary>
    /// Получить все торговые пары
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<OperationResult<List<TradingPair>, ApiErrorCode>>> GetTradingPairsAsync()
    {
        try
        {
            _logger.LogInformation("Getting all trading pairs");

            var commandGrain = _clusterClient.GetGrain<ICommandGrain>(0);
            var command = new GetTradingPairsCommand { ActiveOnly = true };
            var pairs = await commandGrain.GetTradingPairs(command);

            return Ok(OperationResult<List<TradingPair>, ApiErrorCode>.Success(pairs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get trading pairs");
            return Ok(OperationResult<List<TradingPair>, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Получить торговые пары по типу (spot, linear, etc.)
    /// </summary>
    [HttpGet("type/{type}")]
    public async Task<ActionResult<OperationResult<List<TradingPair>, ApiErrorCode>>> GetTradingPairsByTypeAsync(TradingPairType type)
    {
        try
        {
            _logger.LogInformation("Getting trading pairs for type: {Type}", type);

            var commandGrain = _clusterClient.GetGrain<ICommandGrain>(0);
            var command = new GetTradingPairsCommand { PairType = type, ActiveOnly = true };
            var pairs = await commandGrain.GetTradingPairs(command);

            return Ok(OperationResult<List<TradingPair>, ApiErrorCode>.Success(pairs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get trading pairs by type: {Type}", type);
            return Ok(OperationResult<List<TradingPair>, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Получить информацию о конкретной торговой паре
    /// </summary>
    [HttpGet("{symbol}")]
    public async Task<ActionResult<OperationResult<TradingPair?, ApiErrorCode>>> GetTradingPairAsync(string symbol)
    {
        try
        {
            _logger.LogInformation("Getting trading pair: {Symbol}", symbol);

            var commandGrain = _clusterClient.GetGrain<ICommandGrain>(0);
            var command = new GetTradingPairsCommand { ActiveOnly = false };
            var pairs = await commandGrain.GetTradingPairs(command);
            
            var pair = pairs.FirstOrDefault(p => p.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));

            if (pair == null)
            {
                return Ok(OperationResult<TradingPair?, ApiErrorCode>.Failed(ApiErrorCode.Position_Not_Found));
            }

            return Ok(OperationResult<TradingPair?, ApiErrorCode>.Success(pair));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get trading pair: {Symbol}", symbol);
            return Ok(OperationResult<TradingPair?, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Обновить список торговых пар с Bybit API (просто получаем свежие данные)
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<OperationResult<bool, ApiErrorCode>>> RefreshTradingPairsAsync()
    {
        try
        {
            _logger.LogInformation("Refreshing trading pairs");

            var commandGrain = _clusterClient.GetGrain<ICommandGrain>(0);
            var command = new GetTradingPairsCommand { ActiveOnly = false };
            var pairs = await commandGrain.GetTradingPairs(command);

            // Если получили пары - значит успешно
            var success = pairs?.Count > 0;

            if (!success)
            {
                return Ok(OperationResult<bool, ApiErrorCode>.Failed(ApiErrorCode.External_API_Unavailable));
            }

            return Ok(OperationResult<bool, ApiErrorCode>.Success(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh trading pairs");
            return Ok(OperationResult<bool, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Получить время последнего обновления торговых пар
    /// </summary>
    [HttpGet("last-updated")]
    public async Task<ActionResult<OperationResult<DateTime, ApiErrorCode>>> GetLastUpdatedAsync()
    {
        try
        {
            _logger.LogInformation("Getting last updated time for trading pairs");

            // Возвращаем текущее время, так как данные всегда свежие из API
            var lastUpdated = DateTime.UtcNow;

            return Ok(OperationResult<DateTime, ApiErrorCode>.Success(lastUpdated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get last updated time");
            return Ok(OperationResult<DateTime, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Получить рыночные данные для указанных торговых пар
    /// </summary>
    [HttpPost("market-data")]
    public async Task<ActionResult<OperationResult<List<TradingPairMarketData>, ApiErrorCode>>> GetMarketDataAsync([FromBody] List<string> pairKeys)
    {
        try
        {
            _logger.LogInformation("Getting market data for {Count} trading pairs", pairKeys?.Count ?? 0);

            if (pairKeys == null || pairKeys.Count == 0)
            {
                return Ok(OperationResult<List<TradingPairMarketData>, ApiErrorCode>.Failed(ApiErrorCode.Validation_Failed));
            }

            var results = new List<TradingPairMarketData>();

            // Параллельно получаем данные для всех пар
            var tasks = pairKeys.Select(async pairKey =>
            {
                try
                {
                    _logger.LogDebug("Processing pair key: {PairKey}", pairKey);

                    // Парсим ключ "{Symbol}:{Type}"
                    var parts = pairKey.Split(':');
                    if (parts.Length != 2)
                    {
                        _logger.LogWarning("Invalid pair key format: {PairKey}", pairKey);
                        return null;
                    }

                    var symbol = parts[0];
                    if (!Enum.TryParse<TradingPairType>(parts[1], out var type))
                    {
                        _logger.LogWarning("Invalid trading pair type: {Type}", parts[1]);
                        return null;
                    }

                    // Получаем или создаем Grain для этой торговой пары
                    var tradingPairGrain = _clusterClient.GetGrain<ITradingPairGrain>(pairKey);

                    // Инициализируем Grain базовой информацией (если нужно)
                    var tradingPair = new TradingPair
                    {
                        Symbol = symbol,
                        Type = type,
                        Exchange = ExchangeType.Bybit,
                        IsActive = true
                    };
                    await tradingPairGrain.InitializeAsync(tradingPair);

                    // Получаем рыночные данные
                    var marketData = await tradingPairGrain.GetMarketDataAsync();
                    return marketData;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get market data for pair: {PairKey}", pairKey);
                    return null;
                }
            });

            var marketDataResults = await Task.WhenAll(tasks);
            results.AddRange(marketDataResults.Where(r => r != null)!);

            _logger.LogInformation("Successfully retrieved market data for {Count} pairs", results.Count);

            return Ok(OperationResult<List<TradingPairMarketData>, ApiErrorCode>.Success(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get market data");
            return Ok(OperationResult<List<TradingPairMarketData>, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }
}