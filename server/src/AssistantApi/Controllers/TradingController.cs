using AssistantApi.Contracts;
using AssistantApi.Models;
using AssistantApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssistantApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradingController : ControllerBase
{
    private readonly IMockDataService _mockDataService;

    public TradingController(IMockDataService mockDataService)
    {
        _mockDataService = mockDataService;
    }

    /// <summary>
    /// Получить историю сделок
    /// </summary>
    [HttpGet("history")]
    public ActionResult<OperationResult<List<Trade>, ApiErrorCode>> GetTradingHistory([FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        try
        {
            if (limit <= 0 || limit > 1000)
                return BadRequest(OperationResult<List<Trade>, ApiErrorCode>.Failed((ApiErrorCode.Validation_Failed, "Limit должен быть от 1 до 1000")));

            var trades = _mockDataService.GetRecentTrades(limit, offset);
            return Ok(OperationResult<List<Trade>, ApiErrorCode>.Success(trades));
        }
        catch
        {
            return StatusCode(500, OperationResult<List<Trade>, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Получить текущий режим торговли
    /// </summary>
    [HttpGet("mode")]
    public ActionResult<OperationResult<TradingMode, ApiErrorCode>> GetTradingMode()
    {
        try
        {
            var mode = _mockDataService.GetTradingMode();
            return Ok(OperationResult<TradingMode, ApiErrorCode>.Success(mode));
        }
        catch
        {
            return StatusCode(500, OperationResult<TradingMode, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Переключить режим торговли (paper/live)
    /// </summary>
    [HttpPost("mode/switch")]
    public ActionResult<OperationResult<ApiErrorCode>> SwitchTradingMode([FromBody] SwitchModeRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Mode) || request.Mode != "paper" && request.Mode != "live")
                return BadRequest(OperationResult<ApiErrorCode>.Failed((ApiErrorCode.Validation_Failed, "Mode должен быть 'paper' или 'live'")));

            if (!request.Confirmation)
                return BadRequest(OperationResult<ApiErrorCode>.Failed((ApiErrorCode.Validation_Failed, "Требуется подтверждение")));

            if (_mockDataService is MockDataService mockService)
            {
                mockService.SetTradingMode(request.Mode);
            }

            return Ok(OperationResult<ApiErrorCode>.Success());
        }
        catch
        {
            return StatusCode(500, OperationResult<ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Приостановить торговлю
    /// </summary>
    [HttpPost("pause")]
    public ActionResult<OperationResult<ApiErrorCode>> PauseTrading()
    {
        try
        {
            // В реальной системе здесь будет логика приостановки торговли
            return Ok(OperationResult<ApiErrorCode>.Success());
        }
        catch
        {
            return StatusCode(500, OperationResult<ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Возобновить торговлю
    /// </summary>
    [HttpPost("resume")]
    public ActionResult<OperationResult<ApiErrorCode>> ResumeTrading()
    {
        try
        {
            // В реальной системе здесь будет логика возобновления торговли
            return Ok(OperationResult<ApiErrorCode>.Success());
        }
        catch
        {
            return StatusCode(500, OperationResult<ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Emergency Stop - закрыть все позиции
    /// </summary>
    [HttpPost("emergency-stop")]
    public ActionResult<OperationResult<ApiErrorCode>> EmergencyStop()
    {
        try
        {
            // В реальной системе здесь будет логика закрытия всех позиций
            return Ok(OperationResult<ApiErrorCode>.Success());
        }
        catch
        {
            return StatusCode(500, OperationResult<ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }
}