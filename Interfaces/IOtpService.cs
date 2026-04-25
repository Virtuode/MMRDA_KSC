namespace mmrdaconsent.API.Interfaces;

public interface IOtpService
{
    Task<bool> SendOtpAsync(string mobileNo);
    Task<bool> VerifyOtpAsync(string mobileNo, string otpCode);
}