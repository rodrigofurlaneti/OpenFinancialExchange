using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.FinancialInstitutions.Create;

public sealed record CreateFinancialInstitutionCommand(
    string BankId,
    string? OrgName,
    string? Fid) : ICommand<long>;
