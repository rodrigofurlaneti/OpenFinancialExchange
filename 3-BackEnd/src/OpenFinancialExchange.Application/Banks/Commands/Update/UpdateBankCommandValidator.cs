using FluentValidation;

namespace OpenFinancialExchange.Application.Banks.Commands.Update;

public sealed class UpdateBankCommandValidator : AbstractValidator<UpdateBankCommand>
{
    public UpdateBankCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.");

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required.")
            .MaximumLength(200).WithMessage("Bank name must not exceed 200 characters.");

        RuleFor(x => x.ISPB)
            .MaximumLength(20).WithMessage("ISPB must not exceed 20 characters.")
            .When(x => x.ISPB is not null);
    }
}
