using MediatR;
using OpenFinancialExchange.Application.Imports.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Imports.Queries.GetById;

public sealed record GetByIdImportQuery(int Id) : IRequest<Result<ImportDto>>;
