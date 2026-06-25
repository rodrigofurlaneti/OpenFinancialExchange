using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class OfxBalance : Entity
{
    public long UserId { get; private set; }
    public long StatementId { get; private set; }
    public string BalanceType { get; private set; } = null!;
    public decimal BalAmt { get; private set; }
    public DateTime DtAsOf { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private OfxBalance() : base(0) { }  // EF Core

    private OfxBalance(long userId, long statementId, string balanceType, decimal balAmt, DateTime dtAsOf) : base(0)
    {
        UserId = userId;
        StatementId = statementId;
        BalanceType = balanceType;
        BalAmt = balAmt;
        DtAsOf = dtAsOf;
        CreatedAt = DateTime.UtcNow;
    }

    private static readonly HashSet<string> ValidTypes = ["LEDGER", "AVAILABLE"];

    public static Result<OfxBalance> Create(long userId, long statementId, string balanceType, decimal balAmt, DateTime dtAsOf)
    {
        if (userId <= 0)
            return Result.Failure<OfxBalance>(
                new Error("OfxBalance.InvalidUser", "A valid user is required."));

        if (statementId <= 0)
            return Result.Failure<OfxBalance>(
                new Error("OfxBalance.InvalidStatement", "A valid statement is required."));

        if (!ValidTypes.Contains(balanceType?.ToUpperInvariant() ?? string.Empty))
            return Result.Failure<OfxBalance>(
                new Error("OfxBalance.InvalidType", "Balance type must be LEDGER or AVAILABLE."));

        return Result.Success(new OfxBalance(userId, statementId, balanceType!.ToUpperInvariant(), balAmt, dtAsOf));
    }
}
