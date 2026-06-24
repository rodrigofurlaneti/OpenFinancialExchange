using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class OfxStatement : AggregateRoot
{
    private readonly List<OfxTransaction> _transactions = [];
    private readonly List<OfxBalance> _balances = [];

    public long ImportId { get; private set; }
    public long BankAccountId { get; private set; }
    public string? TrnUid { get; private set; }
    public string CurDef { get; private set; } = null!;
    public DateTime? DtServer { get; private set; }
    public string? Language { get; private set; }
    public short? StatusCode { get; private set; }
    public string? StatusSeverity { get; private set; }
    public DateTime? DtStart { get; private set; }
    public DateTime? DtEnd { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<OfxTransaction> Transactions => _transactions.AsReadOnly();
    public IReadOnlyCollection<OfxBalance> Balances => _balances.AsReadOnly();

    private OfxStatement() : base(0) { }  // EF Core

    private OfxStatement(long importId, long bankAccountId, string? trnUid, string curDef,
        DateTime? dtServer, string? language, short? statusCode, string? statusSeverity,
        DateTime? dtStart, DateTime? dtEnd) : base(0)
    {
        ImportId = importId;
        BankAccountId = bankAccountId;
        TrnUid = trnUid;
        CurDef = curDef;
        DtServer = dtServer;
        Language = language;
        StatusCode = statusCode;
        StatusSeverity = statusSeverity;
        DtStart = dtStart;
        DtEnd = dtEnd;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<OfxStatement> Create(
        long importId, long bankAccountId, string? trnUid, string curDef,
        DateTime? dtServer, string? language, short? statusCode, string? statusSeverity,
        DateTime? dtStart, DateTime? dtEnd)
    {
        if (importId <= 0)
            return Result.Failure<OfxStatement>(
                new Error("OfxStatement.InvalidImport", "A valid import is required."));

        if (bankAccountId <= 0)
            return Result.Failure<OfxStatement>(
                new Error("OfxStatement.InvalidAccount", "A valid bank account is required."));

        if (string.IsNullOrWhiteSpace(curDef) || curDef.Length > 3)
            return Result.Failure<OfxStatement>(
                new Error("OfxStatement.InvalidCurDef", "Currency must be a 3-character ISO code."));

        return Result.Success(new OfxStatement(importId, bankAccountId, trnUid, curDef.ToUpperInvariant(),
            dtServer, language, statusCode, statusSeverity, dtStart, dtEnd));
    }
}
