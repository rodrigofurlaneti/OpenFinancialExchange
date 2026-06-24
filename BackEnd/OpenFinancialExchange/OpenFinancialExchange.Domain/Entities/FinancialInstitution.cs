using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class FinancialInstitution : AggregateRoot
{
    public string BankId { get; private set; } = null!;
    public string? OrgName { get; private set; }
    public string? Fid { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private FinancialInstitution() : base(0) { }  // EF Core

    private FinancialInstitution(string bankId, string? orgName, string? fid) : base(0)
    {
        BankId = bankId;
        OrgName = orgName;
        Fid = fid;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<FinancialInstitution> Create(string bankId, string? orgName, string? fid)
    {
        if (string.IsNullOrWhiteSpace(bankId))
            return Result.Failure<FinancialInstitution>(
                new Error("FinancialInstitution.EmptyBankId", "Bank ID is required."));

        if (bankId.Length > 20)
            return Result.Failure<FinancialInstitution>(
                new Error("FinancialInstitution.BankIdTooLong", "Bank ID must not exceed 20 characters."));

        return Result.Success(new FinancialInstitution(bankId.Trim(), orgName?.Trim(), fid?.Trim()));
    }

    public Result UpdateDetails(string? orgName, string? fid)
    {
        OrgName = orgName?.Trim();
        Fid = fid?.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
