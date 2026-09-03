namespace FalakAlkhair.Domain.Common.Enums;

/// <summary>الغرض من الإعلان العقاري: بيع أو إيجار.</summary>
public enum ListingType
{
    ForSale = 1,
    ForRent = 2
}

/// <summary>حالة الإعلان العقاري (Listing).</summary>
public enum ListingStatus
{
    Draft = 1,
    PendingReview = 2,
    Published = 3,
    Paused = 4,
    Expired = 5,
    Sold = 6,
    Rented = 7
}

/// <summary>قناة التسويق العقاري.</summary>
public enum MarketingChannel
{
    Website = 1,
    Google = 2,
    Instagram = 3,
    Snapchat = 4,
    TikTok = 5,
    Facebook = 6,
    WhatsApp = 7,
    PropertyPortals = 8,
    Offline = 9,
    Other = 99
}

/// <summary>حالة معاينة عقار/وحدة.</summary>
public enum ViewingStatus
{
    Scheduled = 1,
    Completed = 2,
    Cancelled = 3,
    NoShow = 4
}

/// <summary>حالة عرض الشراء (Offer) المقدَّم من مشترٍ.</summary>
public enum OfferStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3,
    Expired = 4,
    Withdrawn = 5
}

/// <summary>مرحلة معاملة البيع ضمن مسار المبيعات (Sales Pipeline).</summary>
public enum SaleStage
{
    Lead = 1,
    Qualified = 2,
    Viewing = 3,
    Offer = 4,
    Negotiation = 5,
    Reserved = 6,
    Contract = 7,
    Payment = 8,
    Completed = 9,
    Cancelled = 10
}
