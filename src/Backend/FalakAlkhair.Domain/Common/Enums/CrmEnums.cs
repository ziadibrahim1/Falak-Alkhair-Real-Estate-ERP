namespace FalakAlkhair.Domain.Common.Enums;

/// <summary>حالة المسوّق العقاري.</summary>
public enum AgentStatus
{
    Active = 1,
    Suspended = 2,
    Inactive = 3
}

/// <summary>الغرض من الشراء لدى المشتري.</summary>
public enum BuyerPurpose
{
    PersonalUse = 1, // سكن شخصي
    Investment = 2   // استثمار
}

/// <summary>الحالة التمويلية للمشتري.</summary>
public enum FinancingStatus
{
    Cash = 1,           // كاش
    BankFinancing = 2,  // تمويل بنكي
    RealEstateFund = 3, // صندوق عقاري / تمويل عقاري
    Undetermined = 99
}

/// <summary>حالة تفويض البيع (Sale Mandate) لدى البائع.</summary>
public enum ListingMandateStatus
{
    Draft = 1,
    Active = 2,
    Expired = 3,
    Cancelled = 4,
    Completed = 5
}

/// <summary>مصدر العميل المحتمل (Lead).</summary>
public enum LeadSource
{
    Website = 1,
    Referral = 2,
    WalkIn = 3,
    Campaign = 4,
    Portal = 5,
    Other = 99
}

/// <summary>نوع العميل المحتمل حسب طبيعة اهتمامه.</summary>
public enum LeadType
{
    Buyer = 1,
    Tenant = 2,
    Owner = 3,
    Seller = 4,
    Investor = 5,
    Vendor = 6
}

/// <summary>حالة العميل المحتمل ضمن دورة التحويل.</summary>
public enum LeadStatus
{
    New = 1,
    Contacted = 2,
    Qualified = 3,
    Converted = 4,
    Lost = 5
}

/// <summary>أولوية متابعة العميل المحتمل.</summary>
public enum LeadPriority
{
    Low = 1,
    Medium = 2,
    High = 3
}

/// <summary>مصدر العمولة (نوع المعاملة التي نتجت عنها).</summary>
public enum CommissionSourceType
{
    Lease = 1,
    Sale = 2,
    Auction = 3
}

/// <summary>حالة صرف العمولة للمسوّق.</summary>
public enum CommissionStatus
{
    Pending = 1,
    Approved = 2,
    Paid = 3,
    Cancelled = 4
}
