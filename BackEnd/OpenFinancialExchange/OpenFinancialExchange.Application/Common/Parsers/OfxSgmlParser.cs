using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenFinancialExchange.Application.Common.Parsers;

public static partial class OfxSgmlParser
{
    public static OfxParsedData? Parse(string ofxData)
    {
        if (string.IsNullOrWhiteSpace(ofxData)) return null;

        // Extract STMTRS block
        var stmtrsMatch = StmtrsRegex().Match(ofxData);
        if (!stmtrsMatch.Success) return null;
        var stmtrs = stmtrsMatch.Groups[1].Value;

        // Extract BANKACCTFROM block
        var acctMatch = BankAcctFromRegex().Match(stmtrs);
        var acctBlock = acctMatch.Success ? acctMatch.Groups[1].Value : stmtrs;

        var bankId   = GetValue(acctBlock, "BANKID");
        var acctId   = GetValue(acctBlock, "ACCTID");
        var acctType = GetValue(acctBlock, "ACCTTYPE");
        var curDef   = GetValue(stmtrs, "CURDEF");

        if (string.IsNullOrWhiteSpace(bankId) || string.IsNullOrWhiteSpace(acctId))
            return null;

        // Extract BANKTRANLIST block
        var tranListMatch = BankTranListRegex().Match(stmtrs);
        var tranList = tranListMatch.Success ? tranListMatch.Groups[1].Value : string.Empty;

        var dtStart = ParseOfxDate(GetValue(tranList, "DTSTART"));
        var dtEnd   = ParseOfxDate(GetValue(tranList, "DTEND"));

        // Extract status info (in STMTTRNRS, before STMTRS)
        var beforeStmtrs = ofxData[..stmtrsMatch.Index];
        var trnUid        = GetValue(beforeStmtrs, "TRNUID").NullIfEmpty();
        var statusCode    = GetValue(beforeStmtrs, "CODE");
        var statusSev     = GetValue(beforeStmtrs, "SEVERITY").NullIfEmpty();

        // DTSERVER / LANGUAGE from SONRS
        var sonrsMatch = SonrsRegex().Match(ofxData);
        var sonrs      = sonrsMatch.Success ? sonrsMatch.Groups[1].Value : string.Empty;
        var dtServer   = ParseOfxDate(GetValue(sonrs, "DTSERVER"));
        var language   = GetValue(sonrs, "LANGUAGE").NullIfEmpty();

        // Parse transactions
        var transactions = new List<OfxTransactionData>();
        foreach (Match m in StmtTrnRegex().Matches(tranList))
        {
            var trn     = m.Groups[1].Value;
            var trnType = GetValue(trn, "TRNTYPE");
            var posted  = ParseOfxDate(GetValue(trn, "DTPOSTED"));
            var amtStr  = GetValue(trn, "TRNAMT");

            if (string.IsNullOrWhiteSpace(trnType) || posted is null) continue;
            if (!decimal.TryParse(amtStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var trnAmt)) continue;

            transactions.Add(new OfxTransactionData(
                TrnType:  trnType.Trim().ToUpperInvariant(),
                DtPosted: posted.Value,
                TrnAmt:   trnAmt,
                FitId:    GetValue(trn, "FITID").NullIfEmpty(),
                Name:     GetValue(trn, "NAME").NullIfEmpty(),
                Memo:     GetValue(trn, "MEMO").NullIfEmpty(),
                CheckNum: GetValue(trn, "CHECKNUM").NullIfEmpty()));
        }

        return new OfxParsedData(
            BankId:         bankId.Trim(),
            AcctId:         acctId.Trim(),
            AcctType:       string.IsNullOrWhiteSpace(acctType) ? "CHECKING" : acctType.Trim().ToUpperInvariant(),
            CurDef:         string.IsNullOrWhiteSpace(curDef) ? "BRL" : curDef.Trim().ToUpperInvariant(),
            DtStart:        dtStart,
            DtEnd:          dtEnd,
            TrnUid:         trnUid,
            StatusCode:     short.TryParse(statusCode, out var sc) ? sc : null,
            StatusSeverity: statusSev,
            DtServer:       dtServer,
            Language:       language,
            Transactions:   transactions);
    }

    // Extracts first value of <TAG>value (SGML leaf — no closing tag)
    private static string GetValue(string content, string tag)
        => GetTagRegex(tag).Match(content).Groups[1].Value.Trim();

    private static Regex GetTagRegex(string tag)
        => new($@"<{Regex.Escape(tag)}>\s*([^\r\n<]+)", RegexOptions.IgnoreCase);

    // OFX date format: 20260504000000[-03:EST]  or  20260501000000  or  00000000000000
    private static DateTime? ParseOfxDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var digits = DigitsOnlyRegex().Match(raw).Value;
        if (digits.Length < 8) return null;
        if (digits.StartsWith("000000")) return null; // placeholder

        if (digits.Length >= 14 &&
            DateTime.TryParseExact(digits[..14], "yyyyMMddHHmmss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt14))
            return dt14;

        if (DateTime.TryParseExact(digits[..8], "yyyyMMdd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt8))
            return dt8;

        return null;
    }

    // Source-generated regexes for performance
    [GeneratedRegex(@"<STMTRS>(.*?)</STMTRS>",      RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex StmtrsRegex();

    [GeneratedRegex(@"<BANKACCTFROM>(.*?)</BANKACCTFROM>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex BankAcctFromRegex();

    [GeneratedRegex(@"<BANKTRANLIST>(.*?)</BANKTRANLIST>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex BankTranListRegex();

    [GeneratedRegex(@"<STMTTRN>(.*?)</STMTTRN>",    RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex StmtTrnRegex();

    [GeneratedRegex(@"<SONRS>(.*?)</SONRS>",         RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex SonrsRegex();

    [GeneratedRegex(@"^\d+")]
    private static partial Regex DigitsOnlyRegex();
}

public sealed record OfxParsedData(
    string BankId,
    string AcctId,
    string AcctType,
    string CurDef,
    DateTime? DtStart,
    DateTime? DtEnd,
    string? TrnUid,
    short? StatusCode,
    string? StatusSeverity,
    DateTime? DtServer,
    string? Language,
    IReadOnlyList<OfxTransactionData> Transactions);

public sealed record OfxTransactionData(
    string TrnType,
    DateTime DtPosted,
    decimal TrnAmt,
    string? FitId,
    string? Name,
    string? Memo,
    string? CheckNum);

internal static class StringExtensions
{
    internal static string? NullIfEmpty(this string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s;
}
