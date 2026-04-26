namespace mmrdaconsent.API.DTOs;

// ============================================================
// ENROLLMENT DTOS
// ============================================================

/// <summary>Step 3 — Data for creating or updating enrollment personal details</summary>
public record CreateEnrollmentDto(
    string FirstName,
    string? MiddleName,
    string LastName,
    string MobileNo,
    string? AadharNumber,
    string? Address1,
    string? Address2,
    int? StateID,
    string? State,
    int? CityID,
    string? City,
    int? DistrictID,
    string? District,
    int? TalukaID,
    string? Taluka,
    string? Pincode
);

/// <summary>Data returned after fetching or saving an enrollment</summary>
public record EnrollmentResponseDto(
    int EnrollmentID,
    string FirstName,
    string? MiddleName,
    string LastName,
    string MobileNo,
    string? AadharNumber,
    string? Address1,
    string? Address2,
    int? StateID,
    string? State,
    int? CityID,
    string? City,
    int? DistrictID,
    string? District,
    int? TalukaID,
    string? Taluka,
    string? Pincode
);