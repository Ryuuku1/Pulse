using Microsoft.AspNetCore.Mvc;
using SolarMonitor.Api.Models;
using SolarMonitor.Application.Services;
using SolarMonitor.Domain.Entities;
using SolarMonitor.Domain.ValueObjects;

namespace SolarMonitor.Api.Controllers;

/// <summary>
/// Controller for managing solar plants/stations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlantsController : ControllerBase
{
    private readonly IPlantService _plantService;
    private readonly ILogger<PlantsController> _logger;

    public PlantsController(
        IPlantService plantService,
        ILogger<PlantsController> logger)
    {
        _plantService = plantService ?? throw new ArgumentNullException(nameof(plantService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all plants accessible by the current credentials.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<Plant>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<Plant>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<Plant>>>> GetPlants(CancellationToken cancellationToken)
    {
        var result = await _plantService.GetPlantsAsync(cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to get plants: {Error}", result.Error);
            return BadRequest(ApiResponse<IReadOnlyList<Plant>>.Fail(result.Error!));
        }

        return Ok(ApiResponse<IReadOnlyList<Plant>>.Ok(result.Value!));
    }

    /// <summary>
    /// Gets detailed summary for a specific plant including current metrics and energy summary.
    /// </summary>
    /// <param name="plantId">The plant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{plantId}")]
    [ProducesResponseType(typeof(ApiResponse<PlantSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PlantSummary>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PlantSummary>>> GetPlantSummary(
        string plantId,
        CancellationToken cancellationToken)
    {
        var result = await _plantService.GetPlantSummaryAsync(plantId, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to get plant summary for {PlantId}: {Error}", plantId, result.Error);
            return NotFound(ApiResponse<PlantSummary>.Fail(result.Error!));
        }

        return Ok(ApiResponse<PlantSummary>.Ok(result.Value!));
    }

    /// <summary>
    /// Gets all devices for a given plant.
    /// </summary>
    /// <param name="plantId">The plant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{plantId}/devices")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<Device>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<Device>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<Device>>>> GetDevices(
        string plantId,
        CancellationToken cancellationToken)
    {
        var result = await _plantService.GetDevicesAsync(plantId, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to get devices for plant {PlantId}: {Error}", plantId, result.Error);
            return NotFound(ApiResponse<IReadOnlyList<Device>>.Fail(result.Error!));
        }

        return Ok(ApiResponse<IReadOnlyList<Device>>.Ok(result.Value!));
    }
}
