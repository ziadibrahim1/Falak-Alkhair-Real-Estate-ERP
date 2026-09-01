namespace FalakAlkhair.Application.Common.Constants;

/// <summary>
/// الأدوار الأساسية المزروعة افتراضيًا (Seed) وعلاماتها IsSystemRole=true
/// (لا يمكن حذفها)، مع بقاء النظام قادرًا على إنشاء أدوار إضافية غير محمية.
/// </summary>
public static class SystemRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string SystemAdministrator = "SystemAdministrator";
    public const string GeneralManager = "GeneralManager";
    public const string RealEstateManager = "RealEstateManager";
    public const string PropertyManager = "PropertyManager";
    public const string SalesManager = "SalesManager";
    public const string LeasingManager = "LeasingManager";
    public const string AuctionManager = "AuctionManager";
    public const string MarketingManager = "MarketingManager";
    public const string Accountant = "Accountant";
    public const string CollectionOfficer = "CollectionOfficer";
    public const string MaintenanceManager = "MaintenanceManager";
    public const string MaintenanceEmployee = "MaintenanceEmployee";
    public const string RealEstateAgent = "RealEstateAgent";
    public const string PropertyOwner = "PropertyOwner";
    public const string Viewer = "Viewer";

    public static readonly IReadOnlyDictionary<string, string> ArabicNames = new Dictionary<string, string>
    {
        [SuperAdmin] = "المدير العام للنظام",
        [SystemAdministrator] = "مدير النظام",
        [GeneralManager] = "المدير العام",
        [RealEstateManager] = "مدير عقاري",
        [PropertyManager] = "مدير أملاك",
        [SalesManager] = "مدير مبيعات",
        [LeasingManager] = "مدير تأجير",
        [AuctionManager] = "مدير مزادات",
        [MarketingManager] = "مدير تسويق",
        [Accountant] = "محاسب",
        [CollectionOfficer] = "موظف تحصيل",
        [MaintenanceManager] = "مدير صيانة",
        [MaintenanceEmployee] = "فني صيانة",
        [RealEstateAgent] = "مسوّق عقاري",
        [PropertyOwner] = "مالك عقار",
        [Viewer] = "مشاهد فقط"
    };
}
