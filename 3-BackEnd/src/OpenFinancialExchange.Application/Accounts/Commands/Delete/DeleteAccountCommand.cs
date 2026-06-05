using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Accounts.Commands.Delete;

public sealed record DeleteAccountCommand(int Id) : IRequest<Result>;
