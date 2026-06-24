using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class OfxTransaction : Entity
{
    public long StatementId { get; private set; }
    public string TrnType { get; private set; } = null!;
    public DateTime DtPosted { get; private set; }
    public decimal TrnAmt { get; private set; }
    public string? FitId { get; private set; }
    public string? Name { get; private set; }
    public string? Memo { get; private set; }
    public string? CheckNum { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private OfxTransaction() : base(0) { }  // EF Core

    private OfxTransaction(long statementId, string trnType, DateTime dtPosted, decimal trnAmt,
        string? fitId, string? name, string? memo, string? checkNum) : base(0)
    {
        StatementId = statementId;
        TrnType = trnType;
        DtPosted = dtPosted;
        TrnAmt = trnAmt;
        FitId = fitId;
        Name = name;
        Memo = memo;
        CheckNum = checkNum;
        CreatedAt = DateTime.UtcNow;
    }

    private static readonly HashSet<string> ValidTrnTypes =
    [
        "CREDIT", "DEBIT", "INT", "DIV", "FEE", "SRVCHG", "DEP", "ATM", "POS",
        "XFER", "CHECK", "PAYMENT", "CASH", "DIRECTDEP", "DIRECTDEBIT", "REPEATPMT", "OTHER"
    ];

    public static Result<OfxTransaction> Create(
        long statementId, string trnType, DateTime dtPosted, decimal trnAmt,
        string? fitId, string? name, string? memo, string? checkNum)
    {
        if (statementId <= 0)
            return Result.Failure<OfxTransaction>(
                new Error("OfxTransaction.InvalidStatement", "A valid statement is required."));

        if (!ValidTrnTypes.Contains(trnType?.ToUpperInvariant() ?? string.Empty))
            return Result.Failure<OfxTransaction>(
                new Error("OfxTransaction.InvalidTrnType", "Invalid transaction type."));

        return Result.Success(new OfxTransaction(statementId, trnType!.ToUpperInvariant(), dtPosted,
            trnAmt, fitId, name, memo, checkNum));
    }
}
