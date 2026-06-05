using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.TransactionCategories.Commands.Delete;

public sealed record DeleteTransactionCategoryCommand(int Id) : IRequest<Result>;
