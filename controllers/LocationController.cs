using Microsoft.AspNetCore.Mvc;
using mmrdaconsent.API.Interfaces;

namespace mmrdaconsent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _service;

    public LocationController(ILocationService service) => _service = service;

    [HttpGet("states")]
    public async Task<IActionResult> GetStates()
        => Ok(await _service.GetStatesAsync());

    [HttpGet("districts/{stateId}")]
    public async Task<IActionResult> GetDistricts(int stateId)
        => Ok(await _service.GetDistrictsByStateAsync(stateId));

    [HttpGet("cities/{stateId}")]
    public async Task<IActionResult> GetCities(int stateId)
        => Ok(await _service.GetCitiesByStateAsync(stateId));

    [HttpGet("talukas/{districtId}")]
    public async Task<IActionResult> GetTalukas(int districtId)
        => Ok(await _service.GetTalukasByDistrictAsync(districtId));

    // FIXED: route param renamed to districtId, calls corrected method
    [HttpGet("villages/{districtId}")]
    public async Task<IActionResult> GetVillages(int districtId)
        => Ok(await _service.GetVillagesByDistrictAsync(districtId));
}