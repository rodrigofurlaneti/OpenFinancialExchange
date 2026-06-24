using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.OfxImports.GetById;

public sealed record GetOfxImportByIdQuery(long Id) : IQuery<OfxImportResponse>;
