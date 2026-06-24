using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.OfxStatements.GetById;

public sealed record GetOfxStatementByIdQuery(long Id) : IQuery<OfxStatementResponse>;
