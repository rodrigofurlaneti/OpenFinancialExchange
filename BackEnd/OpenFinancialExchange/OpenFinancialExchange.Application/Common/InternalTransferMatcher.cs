using System.Globalization;
using System.Text;

namespace OpenFinancialExchange.Application.Common;

/// <summary>
/// Auto-categorização por palavra-chave. Recebe os pares (categoryId, keywords) e o
/// texto da transação (Name + Memo); devolve a categoria cuja palavra-chave aparece
/// na descrição. Quando várias casam, vence a MAIOR palavra (mais específica).
/// A comparação ignora acentos e maiúsculas/minúsculas.
/// </summary>
public static class CategoryKeywordMatcher
{
    public static long? Match(IEnumerable<(long Id, string? Keywords)> categories, string? name, string? memo)
    {
        var text = Normalize($"{name} {memo}");
        if (text.Length == 0)
            return null;

        long? bestId = null;
        var bestLen = 0;

        foreach (var (id, keywords) in categories)
        {
            foreach (var raw in SplitKeywords(keywords))
            {
                var kw = Normalize(raw);
                if (kw.Length > bestLen && text.Contains(kw, StringComparison.Ordinal))
                {
                    bestLen = kw.Length;
                    bestId = id;
                }
            }
        }

        return bestId;
    }

    public static IEnumerable<string> SplitKeywords(string? keywords)
        => (keywords ?? string.Empty)
            .Split(['\n', '\r', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string Normalize(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;

        var formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);

        return sb.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
    }
}
