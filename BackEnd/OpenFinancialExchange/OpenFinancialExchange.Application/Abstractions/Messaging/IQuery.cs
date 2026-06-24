using MediatR;
using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
