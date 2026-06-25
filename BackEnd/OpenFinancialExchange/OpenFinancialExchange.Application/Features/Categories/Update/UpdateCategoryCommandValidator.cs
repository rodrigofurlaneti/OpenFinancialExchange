using FluentValidation;

namespace OpenFinancialExchange.Application.Features.Categories.Update;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

        RuleFor(x => x.Kind)
            .NotEmpty().WithMessage("Kind is required.")
            .Must(k => k is not null &&
                       new[] { "CREDIT", "DEBIT", "BOTH" }.Contains(k.ToUpperInvariant()))
            .WithMessage("Kind must be CREDIT, DEBIT or BOTH.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Color is required.")
            .MaximumLength(7).WithMessage("Color must not exceed 7 characters.");
    }
}
