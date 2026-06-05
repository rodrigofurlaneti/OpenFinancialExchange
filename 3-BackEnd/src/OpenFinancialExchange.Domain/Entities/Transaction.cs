using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class Transaction : AggregateRoot
{
    public int      StatementId          { get; private set; }
    public int?     CategoryId           { get; private set; }
    public string   TransactionType      { get; private set; } = default!;
    public string   PostedDateRaw        { get; private set; } = default!;
    public DateOnly PostedDate           { get; private set; }
    public string?  TimeZone             { get; private set; }
    public decimal  Amount               { get; private set; }
    public string   FITID                { get; private set; } = default!;
    public string?  CheckNumber          { get; private set; }
    public string?  Memo                 { get; private set; }
    public decimal  AbsoluteAmount       { get; private set; }
    public string   MovementNature       { get; private set; } = default!;  // CREDIT | DEBIT
    public string?  PayeeName            { get; private set; }
    public string?  TransactionDateMemo  { get; private set; }
    public string?  OperationSubtype     { get; private set; }
    public bool     IsReconciled         { get; private set; }
    public DateTime? ReconciledAt        { get; private set; }

    private static readonly string[] AllowedTypes =
    [
        "CREDIT","DEBIT","INT","DIV","FEE","SRVCHG",
        "DEP","ATM","POS","XFER","CHECK","PAYMENT",
        "CASH","DIRECTDEP","DIRECTDEBIT","REPEATPMT","OTHER"
    ];

    private Transaction() { }

    public static Result<Transaction> Create(
        int statementId, int? categoryId,
        string transactionType, string postedDateRaw, DateOnly postedDate,
        string? timeZone, decimal amount, string fitid,
        string? checkNumber, string? memo,
        string? payeeName, string? transactionDateMemo, string? operationSubtype)
    {
        if (statementId <= 0)
            return Result.Failure<Transaction>(new Error("Transaction.InvalidStatement", "Valid Statement ID is required."));

        if (!AllowedTypes.Contains(transactionType))
            return Result.Failure<Transaction>(new Error("Transaction.InvalidType",
                $"TransactionType must be one of: {string.Join(", ", AllowedTypes)}."));

        if (string.IsNullOrWhiteSpace(fitid))
            return Result.Failure<Transaction>(new Error("Transaction.InvalidFITID", "FITID is required."));

        var entity = new Transaction
        {
            StatementId         = statementId,
            CategoryId          = categoryId,
            TransactionType     = transactionType,
            PostedDateRaw       = postedDateRaw.Trim(),
            PostedDate          = postedDate,
            TimeZone            = timeZone?.Trim(),
            Amount              = amount,
            FITID               = fitid.Trim(),
            CheckNumber         = checkNumber?.Trim(),
            Memo                = memo?.Trim(),
            AbsoluteAmount      = Math.Abs(amount),
            MovementNature      = amount >= 0 ? "CREDIT" : "DEBIT",
            PayeeName           = payeeName?.Trim(),
            TransactionDateMemo = transactionDateMemo?.Trim(),
            OperationSubtype    = operationSubtype?.Trim(),
            IsReconciled        = false,
            CreatedAt           = DateTime.UtcNow
        };

        entity.RaiseDomainEvent(new Events.TransactionCreatedEvent(Guid.NewGuid(), DateTime.UtcNow, entity.FITID, entity.Amount));
        return Result.Success(entity);
    }

    public Result Update(int? categoryId, string? memo, string? payeeName, string? operationSubtype)
    {
        CategoryId       = categoryId;
        Memo             = memo?.Trim();
        PayeeName        = payeeName?.Trim();
        OperationSubtype = operationSubtype?.Trim();
        SetUpdatedAt(DateTime.UtcNow);
        return Result.Success();
    }

    public Result Reconcile()
    {
        if (IsReconciled)
            return Result.Failure(new Error("Transaction.AlreadyReconciled", "Transaction is already reconciled."));

        IsReconciled = true;
        ReconciledAt = DateTime.UtcNow;
        SetUpdatedAt(DateTime.UtcNow);
        return Result.Success();
    }

    public Result Unreconcile()
    {
        IsReconciled = false;
        ReconciledAt = null;
        SetUpdatedAt(DateTime.UtcNow);
        return Result.Success();
    }
}
