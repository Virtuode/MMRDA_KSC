using Microsoft.EntityFrameworkCore;
using mmrdaconsent.API.Data;
using mmrdaconsent.API.DTOs;
using mmrdaconsent.API.Entities;
using mmrdaconsent.API.Interfaces;

namespace mmrdaconsent.API.Services;

public class ApplicationService : IApplicationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ApplicationService> _logger;

    public ApplicationService(AppDbContext context, ILogger<ApplicationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ── ENROLLMENT ───────────────────────────────────────────────────────

    public async Task<EnrollmentResponseDto> GetOrCreateEnrollmentAsync(string mobileNo)
    {
        var existing = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.MobileNo == mobileNo && !e.Deleted);

        if (existing != null)
            return MapEnrollment(existing);

        var newEnrollment = new Enrollment
        {
            MobileNo = mobileNo,
            IsActive = true,
            Deleted = false,
            CreatedOn = DateTime.Now
        };

        _context.Enrollments.Add(newEnrollment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("[Enrollment] Created new enrollment for {Mobile}", mobileNo);
        return MapEnrollment(newEnrollment);
    }

    public async Task<EnrollmentResponseDto?> GetEnrollmentByMobileAsync(string mobileNo)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.MobileNo == mobileNo && !e.Deleted);

        return enrollment == null ? null : MapEnrollment(enrollment);
    }

    public async Task<EnrollmentResponseDto> UpdateEnrollmentAsync(int enrollmentId, CreateEnrollmentDto dto)
    {
        var enrollment = await _context.Enrollments.FindAsync(enrollmentId)
            ?? throw new KeyNotFoundException($"Enrollment {enrollmentId} not found.");

        // FIXED: resolve State and City names so the varchar columns are populated
        string? stateName = null;
        string? cityName = null;

        if (dto.StateID.HasValue)
            stateName = await _context.States
                .Where(s => s.StateID == dto.StateID.Value)
                .Select(s => s.StateName)
                .FirstOrDefaultAsync();

        if (dto.CityID.HasValue)
            cityName = await _context.Cities
                .Where(c => c.CityID == dto.CityID.Value)
                .Select(c => c.CityName)
                .FirstOrDefaultAsync();

        enrollment.FirstName = dto.FirstName;
        enrollment.MiddleName = dto.MiddleName;
        enrollment.LastName = dto.LastName;
        enrollment.MobileNo = dto.MobileNo;
        enrollment.AadharNumber = dto.AadharNumber;
        enrollment.Address1 = dto.Address1;
        enrollment.Address2 = dto.Address2;
        enrollment.StateID = dto.StateID;
        enrollment.State = stateName;       // FIXED: was never set
        enrollment.CityID = dto.CityID;
        enrollment.City = cityName;         // FIXED: was never set
        enrollment.Pincode = dto.Pincode;
        enrollment.LastModifiedOn = DateTime.Now;

        await _context.SaveChangesAsync();

        _logger.LogInformation("[Enrollment] Updated EnrollmentID={ID}", enrollmentId);
        return MapEnrollment(enrollment);
    }

    // ── APPLICATION ──────────────────────────────────────────────────────

    public async Task<ApplicationResponseDto> CreateApplicationAsync(CreateApplicationDto dto)
    {
        var enrollment = await _context.Enrollments.FindAsync(dto.EnrollmentID)
            ?? throw new KeyNotFoundException($"Enrollment {dto.EnrollmentID} not found.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var application = new Application
            {
                EnrollmentID = dto.EnrollmentID,
                ReferenceNo = GenerateReferenceNo(),
                IsActive = true,
                Deleted = false,
                CreatedOn = DateTime.Now
            };
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            var land = new LandDetail
            {
                ApplicationID = application.ApplicationID,
                GatNo = dto.GatNo,
                SurveyNo = dto.SurveyNo,
                TotalAreaHecter = dto.TotalAreaHecter,
                IsActive = true,
                Deleted = false,
                CreatedOn = DateTime.Now
            };
            _context.LandDetails.Add(land);

            // FIXED: resolve District / Taluka / Village names before saving
            string? districtName = null;
            string? talukaName = null;
            string? villageName = null;

            if (dto.DistrictID.HasValue)
                districtName = await _context.Districts
                    .Where(d => d.DistrictID == dto.DistrictID.Value)
                    .Select(d => d.DistrictName)
                    .FirstOrDefaultAsync();

            if (dto.TalukaID.HasValue)
                talukaName = await _context.Talukas
                    .Where(t => t.TalukaID == dto.TalukaID.Value)
                    .Select(t => t.TalukaName)
                    .FirstOrDefaultAsync();

            if (dto.VillageID.HasValue)
                villageName = await _context.Villages
                    .Where(v => v.VillageID == dto.VillageID.Value)
                    .Select(v => v.VillageName)
                    .FirstOrDefaultAsync();

            var location = new LocationDetail
            {
                ApplicationID = application.ApplicationID,
                DistrictID = dto.DistrictID,
                District = districtName,    // FIXED: was always null
                TalukaID = dto.TalukaID,
                Taluka = talukaName,        // FIXED: was always null
                VillageID = dto.VillageID,
                Village = villageName,      // FIXED: was always null
                CompensationTypeID = dto.CompensationTypeID,
                IsActive = true,
                Deleted = false,
                CreatedOn = DateTime.Now
            };
            _context.LocationDetails.Add(location);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("[Application] Created ApplicationID={ID}, Ref={Ref}",
                application.ApplicationID, application.ReferenceNo);

            return await GetApplicationByIdAsync(application.ApplicationID)
                ?? throw new Exception("Failed to retrieve created application.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "[Application] Transaction rolled back for EnrollmentID={ID}", dto.EnrollmentID);
            throw;
        }
    }

    public async Task<ApplicationResponseDto?> GetApplicationByIdAsync(int applicationId)
    {
        var app = await _context.Applications
            .Include(a => a.LandDetails.Where(l => !l.Deleted))
            .Include(a => a.LocationDetails.Where(l => !l.Deleted))
            .FirstOrDefaultAsync(a => a.ApplicationID == applicationId && !a.Deleted);

        return app == null ? null : MapApplication(app);
    }

    // ── CONSENT ──────────────────────────────────────────────────────────

    public async Task<ApplicationResponseDto> SaveConsentAsync(SaveConsentDto dto)
    {
        var application = await _context.Applications.FindAsync(dto.ApplicationID)
            ?? throw new KeyNotFoundException($"Application {dto.ApplicationID} not found.");

        if (application.IsSubmitted)
            throw new InvalidOperationException("Cannot modify a submitted application.");

        application.Age = dto.Age;
        application.SelectedDeclaration = dto.SelectedDeclaration;
        application.ConsentSignedAt = DateTime.Now;
        application.IsConsentGiven = true;
        application.LastModifiedOn = DateTime.Now;

        await _context.SaveChangesAsync();

        var result = await GetApplicationByIdAsync(dto.ApplicationID);

        if (result == null)
        {
            _logger.LogWarning("Application not found for ID={ID}", dto.ApplicationID);
            return null!;
        }

        return result;
    }




    // ── Private Helpers ──────────────────────────────────────────────────

    private static string GenerateReferenceNo()
    {
        string datePart = DateTime.Now.ToString("yyyyMMdd");
        string randomPart = Guid.NewGuid().ToString("N")[..8].ToUpper();
        return $"MMRDA-{datePart}-{randomPart}";
    }

    private static EnrollmentResponseDto MapEnrollment(Enrollment e) => new(
        e.EnrollmentID, e.FirstName, e.MiddleName, e.LastName,
        e.MobileNo, e.AadharNumber, e.Address1, e.Address2,
        e.StateID, e.CityID, e.Pincode
    );

    private static ApplicationResponseDto MapApplication(Application a) => new(
        a.ApplicationID,
        a.EnrollmentID,
        a.ReferenceNo,
        a.IsConsentGiven,
        a.IsSubmitted,
        a.SubmittedAt,
        a.LandDetails.Select(l => new LandDetailDto(
            l.LandDetailID, l.GatNo, l.SurveyNo, l.TotalAreaHecter
        )).ToList(),
        a.LocationDetails.Select(l => new LocationDetailDto(
            l.LocationDetailID, l.DistrictID, l.District,
            l.TalukaID, l.Taluka, l.VillageID, l.Village
        )).ToList()
    );

    // ── FINAL SUBMIT ─────────────────────────────────────────────────────
    public async Task<FinalSubmitResponseDto> FinalSubmitAsync(FinalSubmitDto dto)
    {
        var application = await _context.Applications
            .Include(a => a.Enrollment)
            .Include(a => a.LandDetails.Where(l => !l.Deleted))
            .Include(a => a.LocationDetails.Where(l => !l.Deleted))
            .FirstOrDefaultAsync(a => a.ApplicationID == dto.ApplicationID && !a.Deleted)
            ?? throw new KeyNotFoundException($"Application {dto.ApplicationID} not found.");

        var errors = new List<string>();

        if (!application.IsConsentGiven)
            errors.Add("Consent has not been given.");

        if (application.IsSubmitted)
            errors.Add("Application is already submitted.");

        if (!application.LandDetails.Any())
            errors.Add("No land details found.");

        if (!application.LocationDetails.Any())
            errors.Add("No location details found.");

        // NEW: block submission if the signed consent PDF was never uploaded
        var hasSignedDocument = await _context.ApplicationDocuments
            .AnyAsync(d => d.ApplicationID == dto.ApplicationID
                        && d.DocumentType == "SignedConsent"
                        && d.IsActive && !d.Deleted);

        if (!hasSignedDocument)
            errors.Add("Signed consent document has not been uploaded.");

        if (errors.Any())
            return new FinalSubmitResponseDto(false, string.Join(" | ", errors), null);

        application.IsSubmitted = true;
        application.SubmittedAt = DateTime.Now;
        application.StageID = 1;
        application.LastModifiedOn = DateTime.Now;

        await _context.SaveChangesAsync();

        _logger.LogInformation("[Submit] ApplicationID={ID} submitted. Ref={Ref}",
            dto.ApplicationID, application.ReferenceNo);

        return new FinalSubmitResponseDto(true, "Application submitted successfully.", application.ReferenceNo);
    }
}