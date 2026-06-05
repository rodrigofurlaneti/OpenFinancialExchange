using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class Account : Entity
{
    public int     ImportId        { get; private set; }
    public int     BankId          { get; private set; }
    public string? BranchNumber    { get; private set; }
    public string  AccountNumber   { get; private set; } = default!;
    public string  AccountType     { get; private set; } = default!;  // CHECKING | SAVINGS | MONEYMRKT | CREDITLINE
    public string  DefaultCurrency { get; private set; } = default!;

    private static readonly string[] AllowedTypes = ["CHECKING", "SAVINGS", "MONEYMRKT", "CREDITLINE"];

    private Account() { }

    public static Result<Account> Create(
        int importId, int bankId,
        string? branchNumber, string accountNumber,
        string accountType, string defaultCurrency)
    {
        if (importId <= 0)
            return Result.Failure<Account>(new Error("Account.InvalidImport", "Valid Import ID is required."));

        if (bankId <= 0)
            return Result.Failure<Account>(new Error("Account.InvalidBank", "Valid Bank ID is required."));

        if (string.IsNullOrWhiteSpace(accountNumber))
            return Result.Failure<Account>(new Error("Account.InvalidNumber", "Account number is required."));

        if (!AllowedTypes.Contains(accountType))
            return Result.Failure<Account>(new Error("Account.InvalidType",
                $"AccountType must be one of: {string.Join(", ", AllowedTypes)}."));

        var entity = new Account
        {
            ImportId        = importId,
            BankId          = bankId,
            BranchNumber    = branchNumber?.Trim(),
            AccountNumber   = accountNumber.Trim(),
            AccountType     = accountType,
            DefaultCurrency = (string.IsNullOrWhiteSpace(defaultCurrency) ? "BRL" : defaultCurrency).Trim().ToUpperInvariant(),
            CreatedAt       = DateTime.UtcNow
        };

        return Result.Success(entity);
    }

    public Result Update(string? branchNumber, string accountNumber, string accountType, string defaultCurrency)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return Result.Failure(new Error("Account.InvalidNumber", "Account number is required."));

        if (!AllowedTypes.Contains(accountType))
            return Result.Failure(new Error("Account.InvalidType",
                $"AccountType must be one of: {string.Join(", ", AllowedTypes)}."));

        BranchNumber    = branchNumber?.Trim();
        AccountNumber   = accountNumber.Trim();
        AccountType     = accountType;
        DefaultCurrency = defaultCurrency.Trim().ToUpperInvariant();
        SetUpdatedAt(DateTime.UtcNow);
        return Result.Success();
    }
}
