using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.FinancialInstitutions.Update;

public sealed record UpdateFinancialInstitutionCommand(
    long Id,
    string? OrgName,
    string? Fid) : ICommand;
