namespace FalakAlkhair.Domain.Common.Enums;

/// <summary>نوع العقار حسب طبيعة الاستخدام في السوق العقاري السعودي.</summary>
public enum PropertyType
{
    Building = 1,           // عمارة
    ResidentialComplex = 2, // مجمع سكني
    Villa = 3,               // فيلا
    Apartment = 4,           // شقة
    Office = 5,               // مكتب
    Shop = 6,                 // محل
    Showroom = 7,             // معرض
    Warehouse = 8,            // مستودع
    Land = 9,                 // أرض
    CommercialBuilding = 10,  // مبنى تجاري
    ResidentialBuilding = 11, // مبنى سكني
    Hotel = 12,                // فندق
    Farm = 13,                 // مزرعة
    Other = 99                 // أخرى
}

/// <summary>تصنيف العقار العام.</summary>
public enum PropertyCategory
{
    Residential = 1, // سكني
    Commercial = 2,  // تجاري
    Mixed = 3,       // مختلط
    Industrial = 4,  // صناعي
    Agricultural = 5,// زراعي
    Land = 6         // أرض
}

/// <summary>حالة العقار.</summary>
public enum PropertyStatus
{
    Active = 1,
    UnderManagement = 2,
    UnderConstruction = 3,
    Inactive = 4,
    Archived = 5
}

/// <summary>حالة الوحدة العقارية.</summary>
public enum UnitStatus
{
    Available = 1,        // متاحة
    Reserved = 2,          // محجوزة
    Rented = 3,             // مؤجرة
    Sold = 4,                // مباعة
    UnderMaintenance = 5,   // تحت الصيانة
    Inactive = 6,            // غير نشطة
    ListedForRent = 7,       // معروضة للإيجار
    ListedForSale = 8,       // معروضة للبيع
    InAuction = 9             // في مزاد
}

/// <summary>نوع الوحدة العقارية.</summary>
public enum UnitType
{
    Apartment = 1,
    Studio = 2,
    Office = 3,
    Shop = 4,
    Showroom = 5,
    Warehouse = 6,
    Villa = 7,
    Room = 8,
    Floor = 9,
    Other = 99
}

/// <summary>نوع التكييف.</summary>
public enum AcType
{
    Central = 1,   // مركزي
    Split = 2,     // سبليت
    Window = 3,    // شباك
    None = 4
}

/// <summary>حالة عقد إدارة الأملاك (Property Management Agreement).</summary>
public enum ManagementAgreementStatus
{
    Draft = 1,
    PendingApproval = 2,
    Active = 3,
    Expiring = 4,
    Expired = 5,
    Terminated = 6
}

/// <summary>نوع أتعاب إدارة الأملاك.</summary>
public enum CommissionType
{
    Percentage = 1,
    FixedAmount = 2,
    Tiered = 3
}
