using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.TransactionCategories.Commands.Create;

public sealed record CreateTransactionCategoryCommand(
    string Code,
    string Description,
    string OperationType,
    string AccountingNature) : IRequest<Result<int>>;
