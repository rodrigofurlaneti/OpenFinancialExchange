using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Banks.Commands.Update;

public sealed record UpdateBankCommand(
    int Id,
    string BankName,
    string? ISPB) : IRequest<Result>;
