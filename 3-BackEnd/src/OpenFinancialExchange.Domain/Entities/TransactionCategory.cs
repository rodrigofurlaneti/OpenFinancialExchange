using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class TransactionCategory : AggregateRoot
{
    public string Code            { get; private set; } = default!;
    public string Description     { get; private set; } = default!;
    public string OperationType   { get; private set; } = default!;
    public string AccountingNature{ get; private set; } = default!;  // REVENUE | EXPENSE | TRANSFER
    public bool   IsActive        { get; private set; }

    private static readonly string[] AllowedNatures = ["REVENUE", "EXPENSE", "TRANSFER"];

    private TransactionCategory() { }

    public static Result<TransactionCategory> Create(
        string code,
        string description,
        string operationType,
        string accountingNature)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<TransactionCategory>(new Error("TransactionCategory.InvalidCode", "Code is required."));

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<TransactionCategory>(new Error("TransactionCategory.InvalidDescription", "Description is required."));

        if (!AllowedNatures.Contains(accountingNature))
            return Result.Failure<TransactionCategory>(new Error("TransactionCategory.InvalidNature",
                $"AccountingNature must be one of: {string.Join(", ", AllowedNatures)}."));

        var entity = new TransactionCategory
        {
            Code             = code.Trim().ToUpperInvariant(),
            Description      = description.Trim(),
            OperationType    = operationType.Trim(),
            AccountingNature = accountingNature,
            IsActive         = true,
            CreatedAt        = DateTime.UtcNow
        };

        entity.RaiseDomainEvent(new Events.TransactionCategoryCreatedEvent(Guid.NewGuid(), DateTime.UtcNow, entity.Code));
        return Result.Success(entity);
    }

    public Result Update(string description, string operationType, string accountingNature)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(new Error("TransactionCategory.InvalidDescription", "Description is required."));

        if (!AllowedNatures.Contains(accountingNature))
            return Result.Failure(new Error("TransactionCategory.InvalidNature",
                $"AccountingNature must be one of: {string.Join(", ", AllowedNatures)}."));

        Description      = description.Trim();
        OperationType    = operationType.Trim();
        AccountingNature = accountingNature;
        SetUpdatedAt(DateTime.UtcNow);
        return Result.Success();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt(DateTime.UtcNow);
    }
}
