using FluentValidation;

namespace OpenFinancialExchange.Application.Transactions.Commands.Update;

public sealed class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0 when provided.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.Memo)
            .MaximumLength(500).WithMessage("Memo must not exceed 500 characters.")
            .When(x => x.Memo is not null);

        RuleFor(x => x.PayeeName)
            .MaximumLength(200).WithMessage("Payee name must not exceed 200 characters.")
            .When(x => x.PayeeName is not null);

        RuleFor(x => x.OperationSubtype)
            .MaximumLength(50).WithMessage("Operation subtype must not exceed 50 characters.")
            .When(x => x.OperationSubtype is not null);
    }
}
