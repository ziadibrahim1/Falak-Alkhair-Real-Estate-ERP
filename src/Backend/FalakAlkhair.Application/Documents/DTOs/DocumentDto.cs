using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Documents.DTOs;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = default!;
    public string EntityType { get; set; } = default!;
    public Guid EntityId { get; set; }
    public string FileName { get; set; } = default!;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = default!;
    public string? Notes { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public static DocumentDto FromEntity(Document document) => new()
    {
        Id = document.Id,
        DocumentType = document.DocumentType,
        EntityType = document.EntityType,
        EntityId = document.EntityId,
        FileName = document.FileName,
        FileSize = document.FileSize,
        MimeType = document.MimeType,
        Notes = document.Notes,
        ExpiryDate = document.ExpiryDate,
        CreatedAt = document.CreatedAt,
        CreatedBy = document.CreatedBy
    };
}
