using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.FinancialInstitutions.GetAll;

public sealed record GetAllFinancialInstitutionsQuery : IQuery<IReadOnlyCollection<FinancialInstitutionResponse>>;
