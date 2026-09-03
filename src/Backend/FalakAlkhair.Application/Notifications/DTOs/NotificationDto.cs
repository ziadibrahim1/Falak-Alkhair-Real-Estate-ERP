using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Notifications.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public int Type { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public static NotificationDto FromEntity(Notification notification) => new()
    {
        Id = notification.Id,
        Type = (int)notification.Type,
        Title = notification.Title,
        Message = notification.Message,
        Link = notification.Link,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt
    };
}
