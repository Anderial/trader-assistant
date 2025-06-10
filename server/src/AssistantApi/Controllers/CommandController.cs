using Microsoft.AspNetCore.Mvc;
using TradeService.Commands;
using TradeService.Grains;
using TradeService.Models;

namespace AssistantApi.Controllers;

/// <summary>
/// Контроллер для выполнения команд через централизованный CommandGrain
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CommandController : ControllerBase
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<CommandController> _logger;

    public CommandController(IClusterClient clusterClient, ILogger<CommandController> logger)
    {
        _clusterClient = clusterClient;
        _logger = logger;
    }

    /// <summary>
    /// Получить все торговые пары с биржи Bybit
    /// </summary>
    /// <param name="pairType">Тип торговых пар (опционально)</param>
    /// <param name="baseAsset">Базовая валюта (опционально)</param>
    /// <param name="quoteAsset">Котируемая валюта (опционально)</param>
    /// <param name="activeOnly">Только активные пары</param>
    /// <returns>Список торговых пар</returns>
    [HttpGet("trading-pairs")]
    public async Task<ActionResult<List<TradingPair>>> GetTradingPairs(
        [FromQuery] TradingPairType? pairType = null,
        [FromQuery] string? baseAsset = null,
        [FromQuery] string? quoteAsset = null,
        [FromQuery] bool activeOnly = true)
    {
        try
        {
            _logger.LogInformation("Getting trading pairs via CommandGrain");

            // Получаем CommandGrain (singleton с key = 0)
            var commandGrain = _clusterClient.GetGrain<ICommandGrain>(0);

            // Создаем команду
            var command = new GetTradingPairsCommand
            {
                PairType = pairType,
                BaseAsset = baseAsset,
                QuoteAsset = quoteAsset,
                ActiveOnly = activeOnly
            };

            // Выполняем команду
            var result = await commandGrain.GetTradingPairs(command);

            _logger.LogInformation("Successfully retrieved {Count} trading pairs", result.Count);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trading pairs");
            return StatusCode(500, $"Ошибка получения торговых пар: {ex.Message}");
        }
    }
} 