using FalakAlkhair.Application.Notifications.Commands.MarkAllNotificationsRead;
using FalakAlkhair.Application.Notifications.Commands.MarkNotificationRead;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FalakAlkhair.UnitTests.Handlers;

public class NotificationCommandHandlerTests
{
    [Fact]
    public async Task MarkNotificationRead_Should_Set_IsRead_And_ReadAt()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();

        var notification = new Notification
        {
            CompanyId = currentUser.CompanyId!.Value,
            UserId = currentUser.UserId,
            Type = NotificationType.System,
            Title = "تنبيه",
            Message = "رسالة تجريبية",
            IsRead = false
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new MarkNotificationReadCommandHandler(context, currentUser);
        await handler.Handle(new MarkNotificationReadCommand(notification.Id), CancellationToken.None);

        var updated = await context.Notifications.FindAsync(notification.Id);
        updated!.IsRead.Should().BeTrue();
        updated.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAllNotificationsRead_Should_Mark_Only_Current_User_And_Broadcast_Notifications()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var otherUserId = Guid.NewGuid();

        var mine = new Notification { CompanyId = currentUser.CompanyId!.Value, UserId = currentUser.UserId, Type = NotificationType.System, Title = "لي", Message = "م", IsRead = false };
        var broadcast = new Notification { CompanyId = currentUser.CompanyId!.Value, UserId = null, Type = NotificationType.System, Title = "عام", Message = "م", IsRead = false };
        var othersOnly = new Notification { CompanyId = currentUser.CompanyId!.Value, UserId = otherUserId, Type = NotificationType.System, Title = "لغيري", Message = "م", IsRead = false };
        context.Notifications.AddRange(mine, broadcast, othersOnly);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new MarkAllNotificationsReadCommandHandler(context, currentUser);
        await handler.Handle(new MarkAllNotificationsReadCommand(), CancellationToken.None);

        (await context.Notifications.FindAsync(mine.Id))!.IsRead.Should().BeTrue();
        (await context.Notifications.FindAsync(broadcast.Id))!.IsRead.Should().BeTrue();
        (await context.Notifications.FindAsync(othersOnly.Id))!.IsRead.Should().BeFalse();
    }
}
