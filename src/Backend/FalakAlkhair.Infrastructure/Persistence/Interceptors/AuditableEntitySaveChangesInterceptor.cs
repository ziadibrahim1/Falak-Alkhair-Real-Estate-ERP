using System.Text.Json;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Common;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FalakAlkhair.Infrastructure.Persistence.Interceptors;

/// <summary>
/// اعتراض موحّد على SaveChanges لتنفيذ أمرين تلقائيًا لكل كيان يرث BaseEntity:
/// 1) تعبئة CreatedAt/CreatedBy/UpdatedAt/UpdatedBy.
/// 2) كتابة سجل تدقيق (AuditLog) غير قابل للتعديل لكل عملية إنشاء/تعديل/حذف
///    على كيانات BaseAuditableEntity، متضمنًا القيم القديمة والجديدة بصيغة JSON.
/// هذا يغني عن كتابة منطق التدقيق يدويًا في كل Handler على حِدة.
/// </summary>
public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private List<AuditLog> _pendingAuditLogs = new();

    public AuditableEntitySaveChangesInterceptor(ICurrentUserService currentUserService, IDateTime dateTime)
    {
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
            _pendingAuditLogs = BuildAuditLogs(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
            _pendingAuditLogs = BuildAuditLogs(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PersistPendingAuditLogs(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await PersistPendingAuditLogsAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = _dateTime.Now;
                    entry.Entity.CreatedBy = _currentUserService.UserName;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = _dateTime.Now;
                    entry.Entity.UpdatedBy = _currentUserService.UserName;
                    break;
            }
        }
    }

    /// <summary>
    /// يبني سجلات التدقيق قبل SaveChanges لأن القيم الأصلية (OriginalValues)
    /// تختفي بعد نجاح الحفظ. تُكتب فعليًا لاحقًا في SavedChanges/SavedChangesAsync
    /// حتى لا تفشل عملية الحفظ الأساسية بسبب خطأ في التدقيق نفسه.
    /// </summary>
    private List<AuditLog> BuildAuditLogs(DbContext context)
    {
        var logs = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Create,
                EntityState.Deleted => AuditAction.Delete,
                _ => AuditAction.Update
            };

            // الحذف الناعم (IsDeleted=true) يظهر كـ Modified وليس Deleted فعليًا، نصنّفه كذلك للوضوح.
            if (entry.State == EntityState.Modified &&
                entry.Property(nameof(BaseAuditableEntity.IsDeleted)).IsModified &&
                (bool)(entry.Property(nameof(BaseAuditableEntity.IsDeleted)).CurrentValue ?? false))
            {
                action = AuditAction.Delete;
            }

            var changedProperties = entry.Properties.Where(p => p.IsModified || entry.State == EntityState.Added).ToList();

            logs.Add(new AuditLog
            {
                UserId = _currentUserService.UserId,
                UserName = _currentUserService.UserName,
                EntityType = entry.Entity.GetType().Name,
                EntityId = entry.Entity.Id.ToString(),
                Action = action,
                OldValues = entry.State == EntityState.Added ? null : Serialize(changedProperties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue)),
                NewValues = entry.State == EntityState.Deleted ? null : Serialize(changedProperties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)),
                AffectedColumns = string.Join(",", changedProperties.Select(p => p.Metadata.Name)),
                IpAddress = _currentUserService.IpAddress,
                UserAgent = _currentUserService.UserAgent,
                CompanyId = entry.Entity.CompanyId,
                BranchId = entry.Entity.BranchId,
                Timestamp = _dateTime.Now
            });
        }

        return logs;
    }

    private static string? Serialize(Dictionary<string, object?> values) =>
        values.Count == 0 ? null : JsonSerializer.Serialize(values);

    private void PersistPendingAuditLogs(DbContext? context)
    {
        if (context is null || _pendingAuditLogs.Count == 0) return;

        context.Set<AuditLog>().AddRange(_pendingAuditLogs);
        _pendingAuditLogs = new List<AuditLog>();
        context.SaveChanges();
    }

    private async Task PersistPendingAuditLogsAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is null || _pendingAuditLogs.Count == 0) return;

        context.Set<AuditLog>().AddRange(_pendingAuditLogs);
        _pendingAuditLogs = new List<AuditLog>();
        await context.SaveChangesAsync(cancellationToken);
    }
}
