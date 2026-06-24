using FluentAssertions;
using NSubstitute;
using OpenFinancialExchange.Application.Features.FinancialInstitutions.Create;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Tests.Application;

public class CreateFinancialInstitutionHandlerTests
{
    private readonly IFinancialInstitutionRepository _repository = Substitute.For<IFinancialInstitutionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateFinancialInstitutionCommandHandler _handler;

    public CreateFinancialInstitutionHandlerTests()
        => _handler = new CreateFinancialInstitutionCommandHandler(_repository, _unitOfWork);

    [Fact]
    public async Task Handle_WhenNewInstitution_ShouldAddAndCommit()
    {
        _repository.ExistsAsync("237", "237", Arg.Any<CancellationToken>()).Returns(false);
        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new CreateFinancialInstitutionCommand("237", "Bradesco", "237");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).AddAsync(Arg.Any<FinancialInstitution>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDuplicateBankId_ShouldReturnFailure()
    {
        _repository.ExistsAsync("237", "237", Arg.Any<CancellationToken>()).Returns(true);

        var command = new CreateFinancialInstitutionCommand("237", "Bradesco", "237");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FinancialInstitution.AlreadyExists");
        await _repository.DidNotReceive().AddAsync(Arg.Any<FinancialInstitution>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidBankId_ShouldReturnDomainFailure()
    {
        _repository.ExistsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(false);

        var command = new CreateFinancialInstitutionCommand("", null, null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
