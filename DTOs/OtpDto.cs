namespace mmrdaconsent.API.DTOs;

public record SendOtpDto(string MobileNo);
public record VerifyOtpDto(string MobileNo, string OtpCode);
public record OtpResponseDto(bool Success, string Message);