using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class LedgerBalance : Entity
{
    public int      StatementId { get; private set; }
    public string   BalanceType { get; private set; } = default!;  // LEDGER | AVAIL
    public decimal  Amount      { get; private set; }
    public DateTime AsOfDate    { get; private set; }

    private static readonly string[] AllowedTypes = ["LEDGER", "AVAIL"];

    private LedgerBalance() { }

    public static Result<LedgerBalance> Create(int statementId, string balanceType, decimal amount, DateTime asOfDate)
    {
        if (statementId <= 0)
            return Result.Failure<LedgerBalance>(new Error("LedgerBalance.InvalidStatement", "Valid Statement ID is required."));

        if (!AllowedTypes.Contains(balanceType))
            return Result.Failure<LedgerBalance>(new Error("LedgerBalance.InvalidType",
                $"BalanceType must be one of: {string.Join(", ", AllowedTypes)}."));

        var entity = new LedgerBalance
        {
            StatementId = statementId,
            BalanceType = balanceType,
            Amount      = amount,
            AsOfDate    = asOfDate,
            CreatedAt   = DateTime.UtcNow
        };

        return Result.Success(entity);
    }

    public Result Update(string balanceType, decimal amount, DateTime asOfDate)
    {
        if (!AllowedTypes.Contains(balanceType))
            return Result.Failure(new Error("LedgerBalance.InvalidType",
                $"BalanceType must be one of: {string.Join(", ", AllowedTypes)}."));

        BalanceType = balanceType;
        Amount      = amount;
        AsOfDate    = asOfDate;
        SetUpdatedAt(DateTime.UtcNow);
        return Result.Success();
    }
}
