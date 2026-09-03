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

    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<MarketingCampaign> MarketingCampaigns => Set<MarketingCampaign>();
    public DbSet<Viewing> Viewings => Set<Viewing>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<Sale> Sales => Set<Sale>();

    public DbSet<MaintenanceEmployee> MaintenanceEmployees => Set<MaintenanceEmployee>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<MaintenanceQuotation> MaintenanceQuotations => Set<MaintenanceQuotation>();
    public DbSet<MaintenanceQuotationItem> MaintenanceQuotationItems => Set<MaintenanceQuotationItem>();

    public DbSet<Auction> Auctions => Set<Auction>();
    public DbSet<AuctionAuditLog> AuctionAuditLogs => Set<AuctionAuditLog>();

    public DbSet<Notification> Notifications => Set<Notification>();

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

/// <summary>ينفّذ INotificationService بإضافة الصف مباشرة لسياق الاختبار، بنفس سلوك التنفيذ الحقيقي.</summary>
public class FakeNotificationService : INotificationService
{
    private readonly TestDbContext _context;

    public FakeNotificationService(TestDbContext context)
    {
        _context = context;
    }

    public void Notify(Guid companyId, Guid? branchId, Guid? userId, FalakAlkhair.Domain.Common.Enums.NotificationType type, string title, string message, string? link = null)
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

/// <summary>تخزين ملفات وهمي في الذاكرة لاختبارات المستندات، بدل الكتابة الفعلية على القرص.</summary>
public class FakeFileStorageService : IFileStorageService
{
    private readonly Dictionary<string, byte[]> _files = new();

    public async Task<(string RelativePath, long FileSize)> SaveAsync(Stream content, string fileName, string subPath, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, cancellationToken);
        var bytes = ms.ToArray();
        var relativePath = $"{subPath}/{Guid.NewGuid():N}_{fileName}";
        _files[relativePath] = bytes;
        return (relativePath, bytes.LongLength);
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken)
    {
        if (!_files.TryGetValue(relativePath, out var bytes))
        {
            throw new FalakAlkhair.Application.Common.Exceptions.NotFoundException("File", relativePath);
        }

        return Task.FromResult<Stream>(new MemoryStream(bytes));
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken)
    {
        _files.Remove(relativePath);
        return Task.CompletedTask;
    }
}
