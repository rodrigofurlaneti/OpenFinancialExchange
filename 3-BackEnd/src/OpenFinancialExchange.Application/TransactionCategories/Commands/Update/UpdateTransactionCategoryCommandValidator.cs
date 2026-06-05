using FluentValidation;

namespace OpenFinancialExchange.Application.TransactionCategories.Commands.Update;

public sealed class UpdateTransactionCategoryCommandValidator
    : AbstractValidator<UpdateTransactionCategoryCommand>
{
    private static readonly string[] AllowedNatures = ["REVENUE", "EXPENSE", "TRANSFER"];

    public UpdateTransactionCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");

        RuleFor(x => x.OperationType)
            .NotEmpty().WithMessage("OperationType is required.")
            .MaximumLength(100).WithMessage("OperationType must not exceed 100 characters.");

        RuleFor(x => x.AccountingNature)
            .NotEmpty().WithMessage("AccountingNature is required.")
            .Must(n => AllowedNatures.Contains(n))
            .WithMessage($"AccountingNature must be one of: {string.Join(", ", AllowedNatures)}.");
    }
}
