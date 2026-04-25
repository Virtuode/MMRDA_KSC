namespace mmrdaconsent.API.DTOs;

/// <summary>Returned after a successful upload</summary>
public record DocumentUploadResponseDto(
    int DocumentID,
    int ApplicationID,
    string DocumentType,
    string OriginalFileName,
    long FileSizeBytes,
    DateTime UploadedAt
);

/// <summary>Returned when listing documents for an application</summary>
public record DocumentListItemDto(
    int DocumentID,
    string DocumentType,
    string OriginalFileName,
    long FileSizeBytes,
    DateTime UploadedAt
);