using Microsoft.AspNetCore.Mvc;
using AssistantApi.Contracts;
using TradeService.Models;
using TradeService.Grains;

namespace AssistantApi.Controllers;

[ApiController]
[Route("api/[controller]")]
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

            var grain = _clusterClient.GetGrain<ITradingPairGrain>("main");
            var pairs = await grain.GetTradingPairsAsync();

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

            var grain = _clusterClient.GetGrain<ITradingPairGrain>("main");
            var pairs = await grain.GetTradingPairsByTypeAsync(type);

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

            var grain = _clusterClient.GetGrain<ITradingPairGrain>("main");
            var pair = await grain.GetTradingPairAsync(symbol);

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
    /// Обновить список торговых пар с Bybit API
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<OperationResult<bool, ApiErrorCode>>> RefreshTradingPairsAsync()
    {
        try
        {
            _logger.LogInformation("Refreshing trading pairs");

            var grain = _clusterClient.GetGrain<ITradingPairGrain>("main");
            var success = await grain.RefreshTradingPairsAsync();

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

            var grain = _clusterClient.GetGrain<ITradingPairGrain>("main");
            var lastUpdated = await grain.GetLastUpdatedAsync();

            return Ok(OperationResult<DateTime, ApiErrorCode>.Success(lastUpdated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get last updated time");
            return Ok(OperationResult<DateTime, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }
}