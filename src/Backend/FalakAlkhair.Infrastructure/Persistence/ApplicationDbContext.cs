using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Infrastructure.Persistence;

/// <summary>
/// سياق قاعدة البيانات الرئيسي. يبني فوق IdentityDbContext لدمج مستخدمي/أدوار
/// النظام مع بقية الكيانات العقارية ضمن نفس قاعدة البيانات (SQL Server).
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
    public DbSet<Document> Documents => Set<Document>();

    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<PropertyManagementAgreement> PropertyManagementAgreements => Set<PropertyManagementAgreement>();

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<LeasePayment> LeasePayments => Set<LeasePayment>();
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Buyer> Buyers => Set<Buyer>();
    public DbSet<Seller> Sellers => Set<Seller>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Commission> Commissions => Set<Commission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // إعادة تسمية جداول Identity الافتراضية لتنسجم مع تسمية بقية جداول النظام.
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        // Global Query Filter لاستبعاد السجلات المحذوفة (Soft Delete) تلقائيًا من كل الاستعلامات.
        builder.Entity<Owner>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Property>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Unit>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<PropertyManagementAgreement>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Document>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Lease>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<LeasePayment>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Payment>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Agent>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Buyer>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Seller>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Lead>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Commission>().HasQueryFilter(e => !e.IsDeleted);
    }
}
