using FalakAlkhair.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Application.Common.Interfaces;

/// <summary>
/// عقد قاعدة البيانات الذي تعتمد عليه طبقة Application، ويُنفَّذ فعليًا في
/// طبقة Infrastructure عبر ApplicationDbContext. هذا الفصل يمنع طبقة
/// Application من الاعتماد المباشر على EF Core / SQL Server.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Company> Companies { get; }
    DbSet<Branch> Branches { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<NumberSequence> NumberSequences { get; }
    DbSet<Document> Documents { get; }

    DbSet<Owner> Owners { get; }
    DbSet<Property> Properties { get; }
    DbSet<Unit> Units { get; }
    DbSet<PropertyManagementAgreement> PropertyManagementAgreements { get; }

    DbSet<Tenant> Tenants { get; }
    DbSet<Lease> Leases { get; }
    DbSet<LeasePayment> LeasePayments { get; }
    DbSet<Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
