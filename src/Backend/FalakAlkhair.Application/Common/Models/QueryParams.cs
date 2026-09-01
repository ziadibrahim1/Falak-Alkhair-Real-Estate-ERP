namespace FalakAlkhair.Application.Common.Models;

/// <summary>
/// معاملات موحّدة للبحث/الفلترة/الترتيب/التقسيم لكل استعلامات القوائم
/// (Search + Filter + Sort + Pagination) بدل تكرارها في كل Query.
/// </summary>
public class ListQueryParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    /// <summary>نص بحث عام (Global Search) داخل حقول الكيان المسموح البحث بها.</summary>
    public string? SearchTerm { get; set; }

    /// <summary>اسم الحقل المستخدم للترتيب.</summary>
    public string? SortBy { get; set; }

    public bool SortDescending { get; set; }
}
