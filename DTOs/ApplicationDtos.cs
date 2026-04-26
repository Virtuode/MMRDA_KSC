namespace mmrdaconsent.API.DTOs;

// ============================================================
// INPUT DTOS (Requests)
// ============================================================

/// <summary>Step 4 — Data for creating a new application</summary>
public record CreateApplicationDto(
    int EnrollmentID,
    string? GatNo,
    string? SurveyNo,
    decimal? TotalAreaHecter,
    int? DistrictID,
    int? TalukaID,
    int? VillageID,
    int? CompensationTypeID
);


public record FinalSubmitDto(int ApplicationID);



// ============================================================
// OUTPUT DTOS (Responses)
// ============================================================

/// <summary>Full application data returned to the client</summary>
public record ApplicationResponseDto(
    int ApplicationID,
    int EnrollmentID,
    string? ReferenceNo,
    List<LandDetailDto> LandDetails,
    List<LocationDetailDto> LocationDetails
);

/// <summary>Status result of the final submission process</summary>
public record FinalSubmitResponseDto(
    bool Success,
    string Message,
    string? ReferenceNo
);


// ============================================================
// SUPPORTING DATA DTOS
// ============================================================

public record LandDetailDto(
    int LandDetailID,
    string? GatNo,
    string? SurveyNo,
    decimal? TotalAreaHecter
);

public record LocationDetailDto(
    int LocationDetailID,
    int? DistrictID,
    string? District,
    int? TalukaID,
    string? Taluka,
    int? VillageID,
    string? Village
);