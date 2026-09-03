using FalakAlkhair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Notifications.Queries.GetUnreadNotificationCount;

/// <summary>عدد الإشعارات غير المقروءة للمستخدم الحالي — لعرضه كشارة على أيقونة الجرس.</summary>
public record GetUnreadNotificationCountQuery : IRequest<int>;

public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadNotificationCountQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        return _context.Notifications
            .AsNoTracking()
            .Where(n => n.CompanyId == _currentUser.CompanyId && !n.IsDeleted && !n.IsRead
                && (n.UserId == null || n.UserId == _currentUser.UserId))
            .CountAsync(cancellationToken);
    }
}
