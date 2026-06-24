using OpenFinancialExchange.Application.Abstractions;
using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.Auth.Register;

internal sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await _userRepository.ExistsByEmailAsync(
            request.Email.ToLowerInvariant(), cancellationToken);

        if (exists)
            return Result.Failure<AuthResponse>(
                new Error("Auth.EmailInUse", "Este e-mail já está em uso."));

        var hash = _passwordHasher.Hash(request.Password);
        var userResult = User.Create(request.Email, hash);

        if (userResult.IsFailure)
            return Result.Failure<AuthResponse>(userResult.Error);

        await _userRepository.AddAsync(userResult.Value, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var (token, expiresAt) = _jwtTokenService.Generate(userResult.Value);

        return Result.Success(new AuthResponse(token, expiresAt));
    }
}
