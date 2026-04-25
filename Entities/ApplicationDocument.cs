namespace mmrdaconsent.API.Entities;

public class ApplicationDocument
{
    public int DocumentID { get; set; }
    public int ApplicationID { get; set; }

    // "SignedConsent" or "Supporting" — controls which upload slot this fills
    public string DocumentType { get; set; } = string.Empty;

    // Original name the user uploaded (e.g. "consent_signed.pdf")
    public string OriginalFileName { get; set; } = string.Empty;

    // GUID-based name stored on disk — prevents collisions and path traversal attacks
    public string StoredFileName { get; set; } = string.Empty;

    // Full absolute path on the server where the file lives
    public string FilePath { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public bool Deleted { get; set; } = false;
    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;
}