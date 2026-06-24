using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.FinancialInstitutions.GetById;

public sealed record GetFinancialInstitutionByIdQuery(long Id) : IQuery<FinancialInstitutionResponse>;
