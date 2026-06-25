using FluentAssertions;
using Moq;
using OpenFinancialExchange.Application.Abstractions;
using OpenFinancialExchange.Application.Features.FinancialInstitutions.Create;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;
using Reqnroll;

namespace OpenFinancialExchange.Specs.StepDefinitions;

[Binding]
public sealed class FinancialInstitutionSteps
{
    private readonly Mock<IFinancialInstitutionRepository> _repository = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private CreateFinancialInstitutionCommand _command = null!;
    private Result<long> _result = null!;

    [Given(@"I have a financial institution command with BankId ""(.*)"" and OrgName ""(.*)""")]
    public void GivenIHaveACreateCommand(string bankId, string orgName)
    {
        _command = new CreateFinancialInstitutionCommand(bankId, orgName, null);
        _repository.Setup(r => r.ExistsAsync(bankId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Given(@"a financial institution with BankId ""(.*)"" already exists")]
    public void GivenInstitutionAlreadyExists(string bankId)
    {
        _command = new CreateFinancialInstitutionCommand(bankId, "Bank", null);
        _repository.Setup(r => r.ExistsAsync(bankId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    [When(@"I send the create command")]
    [When(@"I try to create another institution with BankId ""(.*)""")]
    public async Task WhenISendTheCommand(string? _ = null)
    {
        _currentUser.SetupGet(c => c.UserId).Returns(1L);
        var handler = new CreateFinancialInstitutionCommandHandler(
            _repository.Object, _currentUser.Object, _unitOfWork.Object);
        _result = await handler.Handle(_command, CancellationToken.None);
    }

    [Then(@"the result should be successful")]
    public void ThenResultShouldBeSuccessful() => _result.IsSuccess.Should().BeTrue();

    [Then(@"the institution should be saved")]
    public void ThenInstitutionShouldBeSaved()
        => _repository.Verify(r => r.AddAsync(It.IsAny<FinancialInstitution>(), It.IsAny<CancellationToken>()), Times.Once);

    [Then(@"the result should fail")]
    public void ThenResultShouldFail() => _result.IsFailure.Should().BeTrue();

    [Then(@"the error code should be ""(.*)""")]
    public void ThenErrorCodeShouldBe(string code) => _result.Error.Code.Should().Be(code);
}
