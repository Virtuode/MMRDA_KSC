using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using mmrdaconsent.API.Data;
using mmrdaconsent.API.Entities;
using mmrdaconsent.API.Interfaces;


namespace mmrdaconsent.API.Services;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly TataSmsSettings _settings;
    private readonly AppDbContext _context;
    private readonly ILogger<OtpService> _logger;

    private const int OtpExpiryMinutes = 2;

    public OtpService(
        IMemoryCache cache,
        HttpClient httpClient,
        IOptions<TataSmsSettings> options,
        AppDbContext context,
        ILogger<OtpService> logger)
    {
        _cache = cache;
        _httpClient = httpClient;
        _settings = options.Value;
        _context = context;
        _logger = logger;
    }

    public async Task<bool> SendOtpAsync(string mobileNo)
    {
        // 1. Generate OTP
        string otp = new Random().Next(100000, 999999).ToString();

        // 2. Invalidate any existing unused OTPs for this number
        var existingOtps = await _context.OTPs
            .Where(o => o.MobileNo == mobileNo && o.IsActive)
            .ToListAsync();

        foreach (var old in existingOtps)
        {
            old.IsActive = false;

        }

        // 3. Save new OTP to DB
        var otpEntity = new OTP
        {
            MobileNo = mobileNo,
            OtpCode = otp,
            ValidTo = DateTime.Now.AddMinutes(OtpExpiryMinutes),
            IsActive = true,
            CreatedOn = DateTime.Now,
            CreatedBy = null,


        };
        _context.OTPs.Add(otpEntity);
        await _context.SaveChangesAsync();

        // 4. Also cache in memory for fast verification
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(OtpExpiryMinutes));
        _cache.Set($"OTP_{mobileNo}", otp, cacheOptions);

        _logger.LogInformation("[OTP] Generated for {Mobile}: {OTP}", mobileNo, otp);

        // 5. Send via Tata SMS Gateway
        return await SendSmsAsync(mobileNo, otp);
    }

    public async Task<bool> VerifyOtpAsync(string mobileNo, string otpCode)
    {
        // 1. Check memory cache first (fastest)
        if (_cache.TryGetValue($"OTP_{mobileNo}", out string? cachedOtp))
        {
            if (cachedOtp == otpCode)
            {
                _cache.Remove($"OTP_{mobileNo}");
                await MarkOtpUsedInDbAsync(mobileNo, otpCode);
                return true;
            }
            return false;
        }

        // 2. Fallback: check DB (in case server restarted and cache was lost)
        var dbOtp = await _context.OTPs
            .Where(o => o.MobileNo == mobileNo
                     && o.OtpCode == otpCode
                     && o.IsActive
                     && o.ValidTo >= DateTime.Now)
            .OrderByDescending(o => o.CreatedOn)
            .FirstOrDefaultAsync();

        if (dbOtp == null) return false;

        dbOtp.IsActive = false;
        dbOtp.LastModifiedOn = DateTime.Now;
        await _context.SaveChangesAsync();

        return true;
    }

    // ── Private Helpers ──────────────────────────────────────────────────

    private async Task MarkOtpUsedInDbAsync(string mobileNo, string otpCode)
    {
        var dbOtp = await _context.OTPs
            .Where(o => o.MobileNo == mobileNo
                     && o.OtpCode == otpCode
                     && o.IsActive)
            .OrderByDescending(o => o.CreatedOn)
            .FirstOrDefaultAsync();

        if (dbOtp != null)
        {
            dbOtp.IsActive = false;
            dbOtp.LastModifiedOn = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    private async Task<bool> SendSmsAsync(string mobileNo, string otp)
    {
        try
        {
            string messageText = $"आपला लॉगइन OTP आहे {otp}. कृपया कोणासोबतही शेअर करू नका. - KSC नवनगर प्रकल्प";
            string encodedMsg = Uri.EscapeDataString(messageText);
            string formattedNo = mobileNo.StartsWith("0") ? mobileNo : $"91{mobileNo}";

            string requestUrl = $"{_settings.BaseUrl}" +
                $"?recipient={formattedNo}" +
                $"&dr=false" +
                $"&msg={encodedMsg}" +
                $"&user={_settings.User}" +
                $"&pswd={_settings.Password}" +
                $"&sender={_settings.SenderId}" +
                $"&PE_ID={_settings.PeId}" +
                $"&Template_ID={_settings.LoginTemplateId}";

            var response = await _httpClient.GetAsync(requestUrl);
            string body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // _logger.LogInformation("[SMS] Sent successfully to {Mobile}. Response: {Body}", mobileNo, body);
                return true;
            }

            // _logger.LogWarning("[SMS] Failed to send OTP to {Mobile}. Status: {Status}. Body: {Body}",
            //     mobileNo, response.StatusCode, body);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SMS] Exception while sending to {Mobile}", mobileNo);
            return false;
        }
    }
}