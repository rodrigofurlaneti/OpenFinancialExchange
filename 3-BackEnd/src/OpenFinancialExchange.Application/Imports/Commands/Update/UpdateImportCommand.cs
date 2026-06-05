using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Imports.Commands.Update;

public sealed record UpdateImportCommand(
    int Id,
    string? Notes) : IRequest<Result>;
