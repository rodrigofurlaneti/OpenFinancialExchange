using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.TransactionCategories.Commands.Update;

public sealed record UpdateTransactionCategoryCommand(
    int Id,
    string Description,
    string OperationType,
    string AccountingNature) : IRequest<Result>;
