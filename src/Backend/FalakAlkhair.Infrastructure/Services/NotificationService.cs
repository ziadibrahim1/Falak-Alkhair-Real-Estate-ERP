using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;

    public NotificationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public void Notify(Guid companyId, Guid? branchId, Guid? userId, NotificationType type, string title, string message, string? link = null)
    {
        _context.Notifications.Add(new Notification
        {
            CompanyId = companyId,
            BranchId = branchId,
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Link = link,
            IsRead = false
        });
    }
}
