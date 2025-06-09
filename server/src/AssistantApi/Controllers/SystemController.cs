using AssistantApi.Contracts;
using AssistantApi.Models;
using AssistantApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssistantApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    private readonly IMockDataService _mockDataService;

    public SystemController(IMockDataService mockDataService)
    {
        _mockDataService = mockDataService;
    }

    /// <summary>
    /// Получить статус системы
    /// </summary>
    [HttpGet("status")]
    public ActionResult<OperationResult<SystemStatus, ApiErrorCode>> GetSystemStatus()
    {
        try
        {
            var status = _mockDataService.GetSystemStatus();
            return Ok(OperationResult<SystemStatus, ApiErrorCode>.Success(status));
        }
        catch
        {
            return StatusCode(500, OperationResult<SystemStatus, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Получить метрики производительности
    /// </summary>
    [HttpGet("performance")]
    public ActionResult<OperationResult<PerformanceMetrics, ApiErrorCode>> GetPerformanceMetrics()
    {
        try
        {
            var metrics = _mockDataService.GetPerformanceMetrics();
            return Ok(OperationResult<PerformanceMetrics, ApiErrorCode>.Success(metrics));
        }
        catch
        {
            return StatusCode(500, OperationResult<PerformanceMetrics, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }

    /// <summary>
    /// Получить данные для графика баланса
    /// </summary>
    [HttpGet("balance-chart")]
    public ActionResult<OperationResult<BalanceChartData, ApiErrorCode>> GetBalanceChart([FromQuery] string period = "24h")
    {
        try
        {
            if (period != "24h" && period != "7d" && period != "30d")
                return BadRequest(OperationResult<BalanceChartData, ApiErrorCode>.Failed((ApiErrorCode.Validation_Failed, "Period должен быть '24h', '7d' или '30d'")));

            var chartData = _mockDataService.GetBalanceChartData(period);
            return Ok(OperationResult<BalanceChartData, ApiErrorCode>.Success(chartData));
        }
        catch
        {
            return StatusCode(500, OperationResult<BalanceChartData, ApiErrorCode>.Failed(ApiErrorCode.Unknown_Error));
        }
    }
}