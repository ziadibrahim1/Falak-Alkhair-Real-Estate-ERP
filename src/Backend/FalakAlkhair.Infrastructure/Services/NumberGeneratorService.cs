using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FalakAlkhair.Infrastructure.Services;

/// <summary>
/// مولّد الأرقام المرجعية. يعتمد على عبارة SQL Server ذرّية واحدة
/// (UPDATE ... OUTPUT) لتفادي أي Race Condition بين مستخدمين يعملان في نفس
/// اللحظة على نفس النوع، دون الحاجة لقفل صريح على مستوى التطبيق.
/// إن لم يكن للنوع/الشركة عدّاد بعد، يُنشأ عبر MERGE ذرّي ثم تُعاد المحاولة.
/// </summary>
public class NumberGeneratorService : INumberGeneratorService
{
    private static readonly Dictionary<string, string> Prefixes = new()
    {
        ["PROPERTY"] = "PROP",
        ["UNIT"] = "UNIT",
        ["OWNER"] = "OWNER",
        ["TEN"] = "TEN",
        ["PMA"] = "PMA",
        ["LEASE"] = "LEASE",
        ["SALE"] = "SALE",
        ["MAINT"] = "MAINT",
        ["AUCT"] = "AUCT",
        ["PAY"] = "PAY",
        ["EXP"] = "EXP",
        ["LEAD"] = "LEAD",
        ["AGENT"] = "AGENT",
        ["BUYER"] = "BUYER",
        ["SELLER"] = "SELLER",
        ["COMM"] = "COMM",
        ["LIST"] = "LIST",
        ["CAMP"] = "CAMP",
        ["VIEW"] = "VIEW",
        ["OFFER"] = "OFFER"
    };

    private readonly ApplicationDbContext _context;

    public NumberGeneratorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateNextNumberAsync(string entityKey, Guid companyId, CancellationToken cancellationToken = default)
    {
        entityKey = entityKey.ToUpperInvariant();
        var prefix = Prefixes.TryGetValue(entityKey, out var p) ? p : entityKey;

        var result = await TryIncrementAsync(entityKey, companyId, cancellationToken);

        if (result is null)
        {
            await EnsureSequenceExistsAsync(entityKey, companyId, prefix, cancellationToken);
            result = await TryIncrementAsync(entityKey, companyId, cancellationToken)
                ?? throw new InvalidOperationException($"تعذّر توليد رقم مرجعي للنوع {entityKey}.");
        }

        var (current, seqPrefix, padding) = result.Value;
        return $"{seqPrefix}-{current.ToString().PadLeft(padding, '0')}";
    }

    private async Task<(long CurrentNumber, string Prefix, int Padding)?> TryIncrementAsync(
        string entityKey, Guid companyId, CancellationToken cancellationToken)
    {
        var rows = await _context.Database.SqlQueryRaw<NumberSequenceResult>(
            @"UPDATE NumberSequences
              SET CurrentNumber = CurrentNumber + 1
              OUTPUT INSERTED.CurrentNumber AS CurrentNumber, INSERTED.Prefix AS Prefix, INSERTED.PaddingLength AS Padding
              WHERE EntityKey = {0} AND CompanyId = {1}",
            entityKey, companyId).ToListAsync(cancellationToken);

        var row = rows.FirstOrDefault();
        return row is null ? null : (row.CurrentNumber, row.Prefix, row.Padding);
    }

    private async Task EnsureSequenceExistsAsync(string entityKey, Guid companyId, string prefix, CancellationToken cancellationToken)
    {
        try
        {
            // ملاحظة: التمرير الصريح لـ CancellationToken كوسيط منفصل ضروري هنا — تمريره
            // كعنصر أخير ضمن params object[] (كما كان سابقًا) يجعل EF Core يحاول ربطه
            // كمعامل SQL فيفشل بخطأ "no store type mapping for CancellationToken".
            await _context.Database.ExecuteSqlRawAsync(
                @"IF NOT EXISTS (SELECT 1 FROM NumberSequences WHERE EntityKey = {0} AND CompanyId = {1})
                  INSERT INTO NumberSequences (Id, EntityKey, Prefix, CurrentNumber, PaddingLength, CompanyId)
                  VALUES (NEWID(), {0}, {2}, 0, 6, {1})",
                new object[] { entityKey, companyId, prefix },
                cancellationToken);
        }
        catch (SqlException)
        {
            // في حال حدوث تعارض تزامن نادر أثناء الإنشاء المبدئي، الصف أصبح موجودًا بالفعل
            // بفضل الفهرس الفريد (CompanyId, EntityKey) — نتجاهل الخطأ ونكمل بالمحاولة التالية.
        }
    }

    private class NumberSequenceResult
    {
        public long CurrentNumber { get; set; }
        public string Prefix { get; set; } = default!;
        public int Padding { get; set; }
    }
}
