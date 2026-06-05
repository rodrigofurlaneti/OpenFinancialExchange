using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Banks.Commands.Delete;

public sealed record DeleteBankCommand(int Id) : IRequest<Result>;
