using AssistantApi.Contracts;
using AssistantApi.Models;
using AssistantApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssistantApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    private readonly IMockDataService _mockDataService;

    public PositionsController(IMockDataService mockDataService)
    {
        _mockDataService = mockDataService;
    }

    /// <summary>
    /// Закрыть позицию
    /// </summary>
    [HttpPost("close")]
    public ActionResult<OperationResult<ApiErrorCode>> ClosePosition([FromBody] ClosePositionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Type) || request.Type != "futures" && request.Type != "spot")
                return BadRequest(OperationResult<ApiErrorCode>.Failed((ApiErrorCode.Validation_Failed, "Type должен быть 'futures' или 'spot'")));

            if (string.IsNullOrEmpty(request.Symbol))
                return BadRequest(OperationResult<ApiErrorCode>.Failed((ApiErrorCode.Validation_Failed, "Symbol обязателен")));

            // Проверяем существование позиции
            if (request.Type == "futures")
            {
                var futuresPositions = _mockDataService.GetFuturesPositions();
                if (!futuresPositions.Any(p => p.Symbol == request.Symbol))
                    return BadRequest(OperationResult<ApiErrorCode>.Failed((ApiErrorCode.Position_Not_Found, $"Фьючерсная позиция {request.Symbol} не найдена")));
            }
            else
            {
                var spotPositions = _mockDataService.GetSpotPositions();
                if (!spotPositions.Any(p => p.Symbol == request.Symbol))
                    return BadRequest(OperationResult<ApiErrorCode>.Failed((ApiErrorCode.Position_Not_Found, $"Спотовая позиция {request.Symbol} не найдена")));
            }

            // В реальной системе здесь будет логика закрытия позиции
            return Ok(OperationResult<ApiErrorCode>.Success());
        }
        catch
        {
            return StatusCode(500, OperationResult<ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Закрыть все позиции
    /// </summary>
    [HttpPost("close-all")]
    public ActionResult<OperationResult<ApiErrorCode>> CloseAllPositions([FromQuery] string? type = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(type) && type != "futures" && type != "spot" && type != "all")
                return BadRequest(OperationResult<ApiErrorCode>.Failed((ApiErrorCode.Validation_Failed, "Type должен быть 'futures', 'spot' или 'all'")));

            // В реальной системе здесь будет логика закрытия всех позиций
            return Ok(OperationResult<ApiErrorCode>.Success());
        }
        catch
        {
            return StatusCode(500, OperationResult<ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }
}