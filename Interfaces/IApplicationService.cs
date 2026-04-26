using mmrdaconsent.API.DTOs;

namespace mmrdaconsent.API.Interfaces;

public interface IApplicationService
{
    // Enrollment
    Task<EnrollmentResponseDto> GetOrCreateEnrollmentAsync(string mobileNo);
    Task<EnrollmentResponseDto?> GetEnrollmentByMobileAsync(string mobileNo);
    Task<EnrollmentResponseDto> UpdateEnrollmentAsync(int enrollmentId, CreateEnrollmentDto dto);

    // Application
    Task<ApplicationResponseDto> CreateApplicationAsync(CreateApplicationDto dto);
    Task<ApplicationResponseDto?> GetApplicationByIdAsync(int applicationId);
    Task<FinalSubmitResponseDto> FinalSubmitAsync(FinalSubmitDto dto);

}