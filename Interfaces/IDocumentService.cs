using mmrdaconsent.API.DTOs;
using Microsoft.AspNetCore.Http;

namespace mmrdaconsent.API.Interfaces;

public interface IDocumentService
{
    Task<DocumentUploadResponseDto> UploadDocumentAsync(int applicationId, IFormFile file, string documentType);
    Task<List<DocumentListItemDto>> GetDocumentsByApplicationAsync(int applicationId);
    Task<(byte[] FileBytes, string ContentType, string FileName)?> DownloadDocumentAsync(int documentId);
    Task<bool> DeleteDocumentAsync(int documentId);
    Task<bool> HasSignedConsentAsync(int applicationId);
}