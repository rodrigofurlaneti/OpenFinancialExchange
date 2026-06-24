using MediatR;
using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.Application.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse> { }
