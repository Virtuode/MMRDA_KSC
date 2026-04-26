using Microsoft.AspNetCore.Mvc;
using mmrdaconsent.API.DTOs;
using mmrdaconsent.API.Interfaces;

namespace mmrdaconsent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly IApplicationService _service;

    public ApplicationController(IApplicationService service) => _service = service;

    // ============================================================
    // ENROLLMENT PHASE (Step 3 & Search)
    // ============================================================

    /// <summary>Step 3 — Save enrollment personal details</summary>
    [HttpPut("enrollment/{enrollmentId}")]
    public async Task<IActionResult> UpdateEnrollment(int enrollmentId, [FromBody] CreateEnrollmentDto dto)
    {
        var result = await _service.UpdateEnrollmentAsync(enrollmentId, dto);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get enrollment by mobile</summary>
    [HttpGet("enrollment/mobile/{mobileNo}")]
    public async Task<IActionResult> GetByMobile(string mobileNo)
    {
        var result = await _service.GetEnrollmentByMobileAsync(mobileNo);
        if (result == null) return NotFound(new { success = false, message = "Not found." });
        return Ok(new { success = true, data = result });
    }

    // ============================================================
    // APPLICATION PHASE (Step 4 & Retrieval)
    // ============================================================

    /// <summary>Step 4 — Create application with land and location details</summary>
    [HttpPost]
    public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationDto dto)
    {
        var result = await _service.CreateApplicationAsync(dto);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get application by ID</summary>
    [HttpGet("{applicationId}")]
    public async Task<IActionResult> GetApplication(int applicationId)
    {
        var result = await _service.GetApplicationByIdAsync(applicationId);
        if (result == null) return NotFound(new { success = false, message = "Not found." });
        return Ok(new { success = true, data = result });
    }

    // ============================================================
    // FINALIZATION PHASE (Steps 5 )
    // ============================================================


    /// <summary>Step 5 — Final submission</summary>
    [HttpPost("submit")]
    public async Task<IActionResult> FinalSubmit([FromBody] FinalSubmitDto dto)
    {
        var result = await _service.FinalSubmitAsync(dto);
        if (!result.Success)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message, referenceNo = result.ReferenceNo });
    }
}