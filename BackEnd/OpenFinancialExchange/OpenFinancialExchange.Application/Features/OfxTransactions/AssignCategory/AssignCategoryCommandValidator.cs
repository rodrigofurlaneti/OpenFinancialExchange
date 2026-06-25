using FluentValidation;

namespace OpenFinancialExchange.Application.Features.OfxTransactions.AssignCategory;

public sealed class AssignCategoryCommandValidator : AbstractValidator<AssignCategoryCommand>
{
    public AssignCategoryCommandValidator()
    {
        RuleFor(x => x.TransactionId).GreaterThan(0).WithMessage("TransactionId must be greater than 0.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).When(x => x.CategoryId is not null)
            .WithMessage("CategoryId must be greater than 0.");
    }
}
