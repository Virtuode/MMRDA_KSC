public record CreateLandDto(int ApplicantId, string GateNumber, string SurveyNumber, string TotalAreaHectare);
public record LandResponseDto(int LandId, int ApplicantId, string GateNumber, string SurveyNumber, string TotalAreaHectare);
