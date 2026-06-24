using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.OfxStatements.GetAll;

public sealed record GetAllOfxStatementsQuery : IQuery<IReadOnlyCollection<OfxStatementResponse>>;
