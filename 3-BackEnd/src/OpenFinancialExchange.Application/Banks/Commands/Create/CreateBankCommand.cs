using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Banks.Commands.Create;

public sealed record CreateBankCommand(
    string COMPECode,
    string BankName,
    string? ISPB) : IRequest<Result<int>>;
