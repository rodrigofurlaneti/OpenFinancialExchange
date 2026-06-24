using MediatR;
using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result> { }

public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }
