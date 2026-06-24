using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Application.Features.Auth;

namespace OpenFinancialExchange.Application.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthResponse>;
