using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Notifications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Notifications.Queries.GetMyNotifications;

/// <summary>
/// إشعارات المستخدم الحالي: الموجَّهة له تحديدًا (UserId مطابق) + الإشعارات
/// العامة على مستوى الشركة (UserId فارغ)، مرتَّبة زمنيًا (الأحدث أولًا).
/// </summary>
public class GetMyNotificationsQuery : ListQueryParams, IRequest<PaginatedList<NotificationDto>>
{
    public bool? UnreadOnly { get; init; }
}

public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, PaginatedList<NotificationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyNotificationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.CompanyId == _currentUser.CompanyId && !n.IsDeleted
                && (n.UserId == null || n.UserId == _currentUser.UserId));

        if (request.UnreadOnly == true)
        {
            query = query.Where(n => !n.IsRead);
        }

        query = query.OrderByDescending(n => n.CreatedAt);

        var projected = query.Select(n => NotificationDto.FromEntity(n));

        return await PaginatedList<NotificationDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
