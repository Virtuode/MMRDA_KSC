using Microsoft.AspNetCore.Mvc;
using mmrdaconsent.API.Interfaces;

namespace mmrdaconsent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _service;

    public DocumentController(IDocumentService service) => _service = service;

    // ── UPLOAD ────────────────────────────────────────────────────────────

    /// <summary>
    /// Upload a PDF document for an application.
    /// documentType must be either "SignedConsent" or "Supporting".
    /// Only one SignedConsent is kept per application — re-uploading replaces the previous.
    /// </summary>
    [HttpPost("upload/{applicationId}")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB hard limit at the HTTP layer
    public async Task<IActionResult> Upload(
        int applicationId,
        IFormFile file,
        [FromQuery] string documentType = "SignedConsent")
    {
        // Validate documentType is one of the allowed values
        if (documentType != "SignedConsent" && documentType != "Supporting")
            return BadRequest(new { success = false, message = "documentType must be 'SignedConsent' or 'Supporting'." });

        try
        {
            var result = await _service.UploadDocumentAsync(applicationId, file, documentType);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ── LIST ──────────────────────────────────────────────────────────────

    /// <summary>Get all active documents for an application</summary>
    [HttpGet("application/{applicationId}")]
    public async Task<IActionResult> GetByApplication(int applicationId)
    {
        var result = await _service.GetDocumentsByApplicationAsync(applicationId);
        return Ok(new { success = true, data = result });
    }

    // ── DOWNLOAD ──────────────────────────────────────────────────────────

    /// <summary>Download a document by its DocumentID</summary>
    [HttpGet("download/{documentId}")]
    public async Task<IActionResult> Download(int documentId)
    {
        var result = await _service.DownloadDocumentAsync(documentId);

        if (result == null)
            return NotFound(new { success = false, message = "Document not found." });

        var (fileBytes, contentType, fileName) = result.Value;

        // Content-Disposition: inline → browser opens PDF in tab
        // Change to "attachment" if you want a forced download instead
        return File(fileBytes, contentType, fileName);
    }

    // ── DELETE ────────────────────────────────────────────────────────────

    /// <summary>Soft-delete a document (keeps file on disk for audit trail)</summary>
    [HttpDelete("{documentId}")]
    public async Task<IActionResult> Delete(int documentId)
    {
        var deleted = await _service.DeleteDocumentAsync(documentId);

        if (!deleted)
            return NotFound(new { success = false, message = "Document not found." });

        return Ok(new { success = true, message = "Document removed." });
    }
}