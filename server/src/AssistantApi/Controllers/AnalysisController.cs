using Microsoft.AspNetCore.Mvc;
using AssistantApi.Contracts;
using TradeService.Models;
using TradeService.Grains;
using TradeService.Commands;

namespace AssistantApi.Controllers;

[ApiController]
[Route("api/analysis")]
public class AnalysisController : ControllerBase
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(IClusterClient clusterClient, ILogger<AnalysisController> logger)
    {
        _clusterClient = clusterClient;
        _logger = logger;
    }

    /// <summary>
    /// Запустить анализ торговой пары
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<OperationResult<bool, ApiErrorCode>>> StartAnalysisAsync([FromBody] StartAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Starting analysis for {Symbol}:{Type}", request.Symbol, request.Type);

            var commandGrain = _clusterClient.GetGrain<ICommandGrain>(0);
            var command = new StartAnalysisCommand
            {
                Symbol = request.Symbol,
                Type = request.Type
            };

            var result = await commandGrain.StartAnalysis(command);

            if (result)
            {
                return Ok(OperationResult<bool, ApiErrorCode>.Success(true));
            }
            else
            {
                return Ok(OperationResult<bool, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start analysis for {Symbol}:{Type}", request.Symbol, request.Type);
            return Ok(OperationResult<bool, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Остановить анализ торговой пары
    /// </summary>
    [HttpPost("stop")]
    public async Task<ActionResult<OperationResult<bool, ApiErrorCode>>> StopAnalysisAsync([FromBody] StopAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Stopping analysis for {PairKey}", request.PairKey);

            var commandGrain = _clusterClient.GetGrain<ICommandGrain>(0);
            var command = new StopAnalysisCommand
            {
                PairKey = request.PairKey
            };

            var result = await commandGrain.StopAnalysis(command);

            if (result)
            {
                return Ok(OperationResult<bool, ApiErrorCode>.Success(true));
            }
            else
            {
                return Ok(OperationResult<bool, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop analysis for {PairKey}", request.PairKey);
            return Ok(OperationResult<bool, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Получить список запущенных анализов
    /// </summary>
    [HttpGet("running")]
    public async Task<ActionResult<OperationResult<List<PairAnalysisInfo>, ApiErrorCode>>> GetRunningAnalysisAsync()
    {
        try
        {
            _logger.LogInformation("Getting running analysis list");

            var commandGrain = _clusterClient.GetGrain<ICommandGrain>(0);
            var command = new GetRunningAnalysisCommand();

            var result = await commandGrain.GetRunningAnalysis(command);

            return Ok(OperationResult<List<PairAnalysisInfo>, ApiErrorCode>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get running analysis");
            return Ok(OperationResult<List<PairAnalysisInfo>, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Получить детальные данные анализа за период
    /// </summary>
    [HttpGet("{pairKey}/details")]
    public async Task<ActionResult<OperationResult<PriceAnalysisDetails, ApiErrorCode>>> GetAnalysisDetailsAsync(
        string pairKey,
        [FromQuery] DateTime fromTime,
        [FromQuery] DateTime toTime)
    {
        try
        {
            _logger.LogInformation("Getting analysis details for {PairKey} from {FromTime} to {ToTime}", 
                pairKey, fromTime, toTime);

            var commandGrain = _clusterClient.GetGrain<ICommandGrain>(0);
            var command = new GetAnalysisDetailsCommand
            {
                PairKey = pairKey,
                FromTime = fromTime,
                ToTime = toTime
            };

            var result = await commandGrain.GetAnalysisDetails(command);

            return Ok(OperationResult<PriceAnalysisDetails, ApiErrorCode>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get analysis details for {PairKey}", pairKey);
            return Ok(OperationResult<PriceAnalysisDetails, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }
}

// Request models
public class StartAnalysisRequest
{
    public string Symbol { get; set; } = string.Empty;
    public TradingPairType Type { get; set; }
}

public class StopAnalysisRequest
{
    public string PairKey { get; set; } = string.Empty;
} 