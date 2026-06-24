using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Application.Features.Auth;

namespace OpenFinancialExchange.Application.Features.Auth.Register;

public sealed record RegisterCommand(string Email, string Password) : ICommand<AuthResponse>;
