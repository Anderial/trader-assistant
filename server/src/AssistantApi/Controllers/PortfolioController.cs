using AssistantApi.Contracts;
using AssistantApi.Models;
using AssistantApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssistantApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly IMockDataService _mockDataService;

    public PortfolioController(IMockDataService mockDataService)
    {
        _mockDataService = mockDataService;
    }

    /// <summary>
    /// Получить общий обзор портфолио
    /// </summary>
    [HttpGet("overview")]
    public ActionResult<OperationResult<PortfolioOverview, ApiErrorCode>> GetOverview()
    {
        try
        {
            var overview = _mockDataService.GetPortfolioOverview();
            return Ok(OperationResult<PortfolioOverview, ApiErrorCode>.Success(overview));
        }
        catch
        {
            return StatusCode(500, OperationResult<PortfolioOverview, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Получить список активных фьючерсных позиций
    /// </summary>
    [HttpGet("positions/futures")]
    public ActionResult<OperationResult<List<FuturesPosition>, ApiErrorCode>> GetFuturesPositions()
    {
        try
        {
            var positions = _mockDataService.GetFuturesPositions();
            return Ok(OperationResult<List<FuturesPosition>, ApiErrorCode>.Success(positions));
        }
        catch
        {
            return StatusCode(500, OperationResult<List<FuturesPosition>, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Получить список активных спотовых позиций
    /// </summary>
    [HttpGet("positions/spot")]
    public ActionResult<OperationResult<List<SpotPosition>, ApiErrorCode>> GetSpotPositions()
    {
        try
        {
            var positions = _mockDataService.GetSpotPositions();
            return Ok(OperationResult<List<SpotPosition>, ApiErrorCode>.Success(positions));
        }
        catch
        {
            return StatusCode(500, OperationResult<List<SpotPosition>, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }
}