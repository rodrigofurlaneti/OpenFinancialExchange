using OpenFinancialExchange.Application.Abstractions;
using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.Auth.Login;

internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(
            request.Email.ToLowerInvariant(), cancellationToken);

        if (user is null || !user.IsActive)
            return Result.Failure<AuthResponse>(
                new Error("Auth.InvalidCredentials", "E-mail ou senha inválidos."));

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthResponse>(
                new Error("Auth.InvalidCredentials", "E-mail ou senha inválidos."));

        var (token, expiresAt) = _jwtTokenService.Generate(user);

        return Result.Success(new AuthResponse(token, expiresAt));
    }
}
