using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Imports.Commands.Delete;

public sealed record DeleteImportCommand(int Id) : IRequest<Result>;
