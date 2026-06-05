using FluentValidation;

namespace OpenFinancialExchange.Application.TransactionCategories.Commands.Create;

public sealed class CreateTransactionCategoryCommandValidator
    : AbstractValidator<CreateTransactionCategoryCommand>
{
    private static readonly string[] AllowedNatures = ["REVENUE", "EXPENSE", "TRANSFER"];

    public CreateTransactionCategoryCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

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
