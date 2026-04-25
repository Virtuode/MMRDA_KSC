public record CreateConsentDto(int ApplicantId, int Age, string SelectedDeclaration);
public record ConsentResponseDto(int ConsentId, int ApplicantId, int Age, string SelectedDeclaration, DateTime SignedAt);
