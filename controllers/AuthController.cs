using Microsoft.AspNetCore.Mvc;
using mmrdaconsent.API.DTOs;
using mmrdaconsent.API.Interfaces;

namespace mmrdaconsent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly IApplicationService _appService;

    public AuthController(IOtpService otpService, IApplicationService appService)
    {
        _otpService = otpService;
        _appService = appService;
    }

    /// <summary>Step 1 — Send OTP to mobile number</summary>
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MobileNo) || dto.MobileNo.Length != 10)
            return BadRequest(new { success = false, message = "Invalid mobile number." });

        var sent = await _otpService.SendOtpAsync(dto.MobileNo);
        if (!sent)
            return StatusCode(503, new { success = false, message = "SMS gateway unavailable." });

        return Ok(new { success = true, message = "OTP sent successfully." });
    }

    /// <summary>Step 2 — Verify OTP and get/create enrollment</summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var valid = await _otpService.VerifyOtpAsync(dto.MobileNo, dto.OtpCode);
        if (!valid)
            return BadRequest(new { success = false, message = "Invalid or expired OTP." });

        // Auto create enrollment if first time
        var enrollment = await _appService.GetOrCreateEnrollmentAsync(dto.MobileNo);

        return Ok(new
        {
            success = true,
            message = "OTP verified successfully.",
            enrollment
        });
    }
}