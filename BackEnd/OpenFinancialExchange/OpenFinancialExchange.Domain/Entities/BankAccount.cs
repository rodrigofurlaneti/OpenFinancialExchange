using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class BankAccount : AggregateRoot
{
    public long UserId { get; private set; }
    public long FinancialInstitutionId { get; private set; }
    public string BankId { get; private set; } = null!;
    public string? BranchId { get; private set; }
    public string AcctId { get; private set; } = null!;
    public string AcctType { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private BankAccount() : base(0) { }  // EF Core

    private BankAccount(long userId, long financialInstitutionId, string bankId, string? branchId, string acctId, string acctType) : base(0)
    {
        UserId = userId;
        FinancialInstitutionId = financialInstitutionId;
        BankId = bankId;
        BranchId = branchId;
        AcctId = acctId;
        AcctType = acctType;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private static readonly HashSet<string> ValidAcctTypes =
        ["CHECKING", "SAVINGS", "MONEYMRKT", "CREDITLINE", "CD", "OTHER"];

    public static Result<BankAccount> Create(
        long userId, long financialInstitutionId, string bankId, string? branchId, string acctId, string acctType)
    {
        if (userId <= 0)
            return Result.Failure<BankAccount>(
                new Error("BankAccount.InvalidUser", "A valid user is required."));

        if (financialInstitutionId <= 0)
            return Result.Failure<BankAccount>(
                new Error("BankAccount.InvalidInstitution", "A valid financial institution is required."));

        if (string.IsNullOrWhiteSpace(bankId))
            return Result.Failure<BankAccount>(
                new Error("BankAccount.EmptyBankId", "Bank ID is required."));

        if (string.IsNullOrWhiteSpace(acctId))
            return Result.Failure<BankAccount>(
                new Error("BankAccount.EmptyAcctId", "Account ID is required."));

        if (!ValidAcctTypes.Contains(acctType?.ToUpperInvariant() ?? string.Empty))
            return Result.Failure<BankAccount>(
                new Error("BankAccount.InvalidAcctType", $"Account type must be one of: {string.Join(", ", ValidAcctTypes)}."));

        return Result.Success(new BankAccount(
            userId, financialInstitutionId, bankId.Trim(), branchId?.Trim(), acctId.Trim(), acctType!.ToUpperInvariant()));
    }

    public Result UpdateDetails(string? branchId, string acctType)
    {
        if (!ValidAcctTypes.Contains(acctType?.ToUpperInvariant() ?? string.Empty))
            return Result.Failure(new Error("BankAccount.InvalidAcctType", "Invalid account type."));

        BranchId = branchId?.Trim();
        AcctType = acctType!.ToUpperInvariant();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
