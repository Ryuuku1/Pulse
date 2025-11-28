using Microsoft.AspNetCore.Mvc;
using SolarMonitor.Api.Models;
using SolarMonitor.Application.Services;
using SolarMonitor.Domain.ValueObjects;

namespace SolarMonitor.Api.Controllers;

/// <summary>
/// Controller for accessing real-time and historical metrics data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MetricsController : ControllerBase
{
    private readonly IMetricsService _metricsService;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(
        IMetricsService metricsService,
        ILogger<MetricsController> logger)
    {
        _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the latest real-time metrics for a plant.
    /// </summary>
    /// <param name="plantId">The plant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("plants/{plantId}/realtime")]
    [ProducesResponseType(typeof(ApiResponse<RealtimeMetrics>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RealtimeMetrics>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RealtimeMetrics>>> GetPlantRealtimeMetrics(
        string plantId,
        CancellationToken cancellationToken)
    {
        var result = await _metricsService.GetPlantRealtimeMetricsAsync(plantId, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to get real-time metrics for plant {PlantId}: {Error}", plantId, result.Error);
            return NotFound(ApiResponse<RealtimeMetrics>.Fail(result.Error!));
        }

        return Ok(ApiResponse<RealtimeMetrics>.Ok(result.Value!));
    }

    /// <summary>
    /// Gets historical time-series data for a plant.
    /// </summary>
    /// <param name="plantId">The plant ID.</param>
    /// <param name="metricType">The type of metric (e.g., Power, Energy).</param>
    /// <param name="from">Start of time range (ISO 8601).</param>
    /// <param name="to">End of time range (ISO 8601).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("plants/{plantId}/timeseries")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TimeseriesPoint>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TimeseriesPoint>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TimeseriesPoint>>>> GetPlantTimeseries(
        string plantId,
        [FromQuery] MetricType metricType,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        var result = await _metricsService.GetPlantHistoricalDataAsync(
            plantId,
            metricType,
            from,
            to,
            cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to get timeseries for plant {PlantId}: {Error}", plantId, result.Error);
            return BadRequest(ApiResponse<IReadOnlyList<TimeseriesPoint>>.Fail(result.Error!));
        }

        return Ok(ApiResponse<IReadOnlyList<TimeseriesPoint>>.Ok(result.Value!));
    }

    /// <summary>
    /// Gets the latest real-time metrics for a device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("devices/{deviceId}/realtime")]
    [ProducesResponseType(typeof(ApiResponse<RealtimeMetrics>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RealtimeMetrics>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RealtimeMetrics>>> GetDeviceRealtimeMetrics(
        string deviceId,
        CancellationToken cancellationToken)
    {
        var result = await _metricsService.GetDeviceRealtimeMetricsAsync(deviceId, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to get real-time metrics for device {DeviceId}: {Error}", deviceId, result.Error);
            return NotFound(ApiResponse<RealtimeMetrics>.Fail(result.Error!));
        }

        return Ok(ApiResponse<RealtimeMetrics>.Ok(result.Value!));
    }
}
