using Microsoft.AspNetCore.Mvc;
using mmrdaconsent.API.Interfaces;

namespace mmrdaconsent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _service;

    public LocationController(ILocationService service) => _service = service;

    // ============================================================
    // STATE & DISTRICT LOOKUPS
    // ============================================================

    /// <summary>Fetch all available states</summary>
    [HttpGet("states")]
    public async Task<IActionResult> GetStates()
        => Ok(await _service.GetStatesAsync());

    /// <summary>Fetch all districts belonging to a specific state</summary>
    [HttpGet("districts/{stateId}")]
    public async Task<IActionResult> GetDistricts(int stateId)
        => Ok(await _service.GetDistrictsByStateAsync(stateId));

    // ============================================================
    // CITY & REGIONAL LOOKUPS
    // ============================================================

    /// <summary>Fetch all cities belonging to a specific state</summary>
    [HttpGet("cities/{stateId}")]
    public async Task<IActionResult> GetCities(int stateId)
        => Ok(await _service.GetCitiesByStateAsync(stateId));

    /// <summary>Fetch all talukas belonging to a specific district</summary>
    [HttpGet("talukas/{districtId}")]
    public async Task<IActionResult> GetTalukas(int districtId)
        => Ok(await _service.GetTalukasByDistrictAsync(districtId));

    // ============================================================
    // LOCAL LEVEL LOOKUPS
    // ============================================================

    /// <summary>Fetch all villages belonging to a specific taluka</summary>
    [HttpGet("villages/{talukaId}")]
    public async Task<IActionResult> GetVillages(int talukaId)
        => Ok(await _service.GetVillagesByTalukaAsync(talukaId));
}