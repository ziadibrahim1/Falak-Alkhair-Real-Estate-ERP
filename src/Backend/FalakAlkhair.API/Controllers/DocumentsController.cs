using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Documents.Commands.DeleteDocument;
using FalakAlkhair.Application.Documents.Commands.UploadDocument;
using FalakAlkhair.Application.Documents.Queries.GetDocumentDownload;
using FalakAlkhair.Application.Documents.Queries.GetDocumentsByEntity;
using FalakAlkhair.Application.Documents.Queries.GetDocumentsList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>
/// رفع/عرض/تنزيل/حذف المستندات المرتبطة بأي كيان (Polymorphic عبر EntityType/EntityId).
/// التنزيل عبر نقطة محمية بصلاحيات ونطاق شركة فقط — لا ملفات ثابتة عامة (راجع IFileStorageService).
/// </summary>
[RequestSizeLimit(20 * 1024 * 1024)]
public class DocumentsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.DocumentView)]
    public async Task<IActionResult> GetList([FromQuery] GetDocumentsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("by-entity")]
    [Authorize(Policy = "Permission:" + Permissions.DocumentView)]
    public async Task<IActionResult> GetByEntity([FromQuery] string entityType, [FromQuery] Guid entityId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetDocumentsByEntityQuery(entityType, entityId), cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}/download")]
    [Authorize(Policy = "Permission:" + Permissions.DocumentView)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetDocumentDownloadQuery(id), cancellationToken);
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.DocumentManage)]
    public async Task<IActionResult> Upload([FromForm] UploadDocumentRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("لم يُرفَق أي ملف."));
        }

        await using var stream = request.File.OpenReadStream();
        var id = await Mediator.Send(new UploadDocumentCommand
        {
            FileContent = stream,
            FileName = request.File.FileName,
            MimeType = request.File.ContentType,
            FileSize = request.File.Length,
            DocumentType = request.DocumentType,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Notes = request.Notes,
            ExpiryDate = request.ExpiryDate
        }, cancellationToken);

        return Ok(ApiResponse<Guid>.Ok(id, "تم رفع المستند بنجاح."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.DocumentManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteDocumentCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "تم حذف المستند بنجاح."));
    }
}

public class UploadDocumentRequest
{
    public IFormFile? File { get; set; }
    public string DocumentType { get; set; } = default!;
    public string EntityType { get; set; } = default!;
    public Guid EntityId { get; set; }
    public string? Notes { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
