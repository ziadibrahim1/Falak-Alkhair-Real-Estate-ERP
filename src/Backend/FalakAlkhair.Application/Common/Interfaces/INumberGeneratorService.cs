namespace FalakAlkhair.Application.Common.Interfaces;

/// <summary>
/// مولّد الأرقام المرجعية المركزي (PROP-000001، UNIT-000001 ...) الآمن تحت
/// التزامن العالي (Concurrent Users) عبر قفل صفّي على جدول NumberSequences.
/// </summary>
public interface INumberGeneratorService
{
    /// <summary>
    /// يولّد الرقم التالي لنوع كيان معيّن ضمن نطاق شركة محددة.
    /// entityKey أمثلة: "PROPERTY", "UNIT", "OWNER", "LEASE", "SALE", "MAINT", "AUCT", "PAY", "EXP", "LEAD", "PMA".
    /// </summary>
    Task<string> GenerateNextNumberAsync(string entityKey, Guid companyId, CancellationToken cancellationToken = default);
}
