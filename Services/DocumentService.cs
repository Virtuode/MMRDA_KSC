using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using mmrdaconsent.API.Data;
using mmrdaconsent.API.DTOs;
using mmrdaconsent.API.Entities;
using mmrdaconsent.API.Interfaces;

namespace mmrdaconsent.API.Services;

public class DocumentStorageSettings
{
    public string BasePath { get; set; } = string.Empty;
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB default
}

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _context;
    private readonly DocumentStorageSettings _settings;
    private readonly ILogger<DocumentService> _logger;

    // Only PDF uploads are accepted for this project
    private static readonly string[] AllowedContentTypes = ["application/pdf"];
    private static readonly string[] AllowedExtensions = [".pdf"];

    public DocumentService(
        AppDbContext context,
        IOptions<DocumentStorageSettings> options,
        ILogger<DocumentService> logger)
    {
        _context = context;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<DocumentUploadResponseDto> UploadDocumentAsync(
        int applicationId, IFormFile file, string documentType)
    {
        // ── 1. Validate application exists and is not yet submitted ──
        var application = await _context.Applications.FindAsync(applicationId)
            ?? throw new KeyNotFoundException($"Application {applicationId} not found.");

        if (application.IsSubmitted)
            throw new InvalidOperationException("Cannot upload documents to a submitted application.");

        // ── 2. Validate file ──
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file was provided.");

        if (file.Length > _settings.MaxFileSizeBytes)
            throw new ArgumentException($"File size exceeds the {_settings.MaxFileSizeBytes / 1024 / 1024} MB limit.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Only PDF files are accepted.");

        if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new ArgumentException("Invalid file content type. Only PDF is accepted.");

        // ── 3. Build storage path ──
        // Folder structure: BasePath / applicationId / documentType /
        var folderPath = Path.Combine(
            _settings.BasePath,
            applicationId.ToString(),
            documentType);

        Directory.CreateDirectory(folderPath);

        // Use a GUID for the stored filename — prevents collisions and path traversal
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullFilePath = Path.Combine(folderPath, storedFileName);

        // ── 4. If a SignedConsent already exists, soft-delete the old one ──
        // Only one SignedConsent is allowed per application at a time
        if (documentType == "SignedConsent")
        {
            var existing = await _context.ApplicationDocuments
                .Where(d => d.ApplicationID == applicationId
                         && d.DocumentType == "SignedConsent"
                         && d.IsActive && !d.Deleted)
                .ToListAsync();

            foreach (var old in existing)
            {
                old.IsActive = false;
                old.Deleted = true;
                old.LastModifiedOn = DateTime.Now;
            }
        }

        // ── 5. Save file to disk ──
        await using (var stream = new FileStream(fullFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // ── 6. Save DB record ──
        var document = new ApplicationDocument
        {
            ApplicationID = applicationId,
            DocumentType = documentType,
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            FilePath = fullFilePath,
            FileSizeBytes = file.Length,
            ContentType = file.ContentType,
            IsActive = true,
            Deleted = false,
            CreatedOn = DateTime.Now
        };

        _context.ApplicationDocuments.Add(document);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "[Document] Uploaded DocumentID={ID} Type={Type} for ApplicationID={AppID}",
            document.DocumentID, documentType, applicationId);

        return new DocumentUploadResponseDto(
            document.DocumentID,
            document.ApplicationID,
            document.DocumentType,
            document.OriginalFileName,
            document.FileSizeBytes,
            document.CreatedOn);
    }

    public async Task<List<DocumentListItemDto>> GetDocumentsByApplicationAsync(int applicationId)
    {
        return await _context.ApplicationDocuments
            .Where(d => d.ApplicationID == applicationId && d.IsActive && !d.Deleted)
            .OrderByDescending(d => d.CreatedOn)
            .Select(d => new DocumentListItemDto(
                d.DocumentID,
                d.DocumentType,
                d.OriginalFileName,
                d.FileSizeBytes,
                d.CreatedOn))
            .ToListAsync();
    }

    public async Task<(byte[] FileBytes, string ContentType, string FileName)?> DownloadDocumentAsync(int documentId)
    {
        var document = await _context.ApplicationDocuments
            .FirstOrDefaultAsync(d => d.DocumentID == documentId && d.IsActive && !d.Deleted);

        if (document == null) return null;

        if (!File.Exists(document.FilePath))
        {
            _logger.LogWarning("[Document] File missing on disk for DocumentID={ID} Path={Path}",
                documentId, document.FilePath);
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(document.FilePath);
        return (bytes, document.ContentType, document.OriginalFileName);
    }

    public async Task<bool> DeleteDocumentAsync(int documentId)
    {
        var document = await _context.ApplicationDocuments.FindAsync(documentId);

        if (document == null || document.Deleted) return false;

        // Soft delete in DB — keep the file on disk for audit trail
        document.IsActive = false;
        document.Deleted = true;
        document.LastModifiedOn = DateTime.Now;

        await _context.SaveChangesAsync();

        _logger.LogInformation("[Document] Soft-deleted DocumentID={ID}", documentId);
        return true;
    }

    public async Task<bool> HasSignedConsentAsync(int applicationId)
    {
        return await _context.ApplicationDocuments
            .AnyAsync(d => d.ApplicationID == applicationId
                        && d.DocumentType == "SignedConsent"
                        && d.IsActive && !d.Deleted);
    }
}