using System.Text;

namespace FalakAlkhair.Application.Common.Utilities;

/// <summary>
/// مولّد CSV بسيط بدون أي اعتمادية خارجية (لا مكتبة PDF/Excel مطلوبة لتصدير
/// بيانات جدولية بسيطة) — يهرب الفواصل والاقتباسات والأسطر الجديدة بحسب RFC 4180.
/// </summary>
public static class CsvWriter
{
    public static string Write(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(',', headers.Select(Escape)));

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(',', row.Select(v => Escape(v?.ToString() ?? string.Empty))));
        }

        return sb.ToString();
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
