using MediatR;
using OpenFinancialExchange.Application.Imports.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Imports.Queries.GetAll;

public sealed record GetAllImportsQuery : IRequest<Result<IReadOnlyList<ImportDto>>>;
