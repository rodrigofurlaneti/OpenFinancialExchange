using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.OfxImports.GetAll;

public sealed record GetAllOfxImportsQuery : IQuery<IReadOnlyCollection<OfxImportResponse>>;
