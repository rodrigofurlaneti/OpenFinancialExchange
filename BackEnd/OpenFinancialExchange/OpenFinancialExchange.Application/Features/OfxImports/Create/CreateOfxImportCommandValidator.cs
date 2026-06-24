using FluentValidation;

namespace OpenFinancialExchange.Application.Features.OfxImports.Create;

public sealed class CreateOfxImportCommandValidator : AbstractValidator<CreateOfxImportCommand>
{
    public CreateOfxImportCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.FileHash).NotEmpty().Length(64)
            .WithMessage("FileHash must be a 64-character SHA-256 string.");
        RuleFor(x => x.Encoding).MaximumLength(20).When(x => x.Encoding is not null);
        RuleFor(x => x.Charset).MaximumLength(20).When(x => x.Charset is not null);
    }
}
