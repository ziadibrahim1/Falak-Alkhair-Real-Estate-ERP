using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.UnitTests.TestHelpers;

/// <summary>
/// سياق قاعدة بيانات مبسّط (EF InMemory) لاختبار طبقة Application دون الحاجة
/// لإعداد ASP.NET Core Identity الكامل أو SQL Server حقيقي.
/// </summary>
public class TestDbContext : DbContext, IApplicationDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
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

    public static TestDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }
}

/// <summary>مولّد أرقام مبسّط للاختبار (تسلسلي في الذاكرة) بدل الاعتماد على SQL Server الحقيقي.</summary>
public class FakeNumberGeneratorService : INumberGeneratorService
{
    private readonly Dictionary<string, int> _counters = new();

    public Task<string> GenerateNextNumberAsync(string entityKey, Guid companyId, CancellationToken cancellationToken = default)
    {
        _counters.TryGetValue(entityKey, out var current);
        current++;
        _counters[entityKey] = current;
        return Task.FromResult($"{entityKey}-{current:D6}");
    }
}
