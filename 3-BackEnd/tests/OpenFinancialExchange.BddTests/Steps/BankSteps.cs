using FluentAssertions;
using NSubstitute;
using OpenFinancialExchange.Application.Banks.Commands.Create;
using OpenFinancialExchange.Application.Banks.Commands.Delete;
using OpenFinancialExchange.Application.Banks.Commands.Update;
using OpenFinancialExchange.Application.Banks.Queries.GetById;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;
using Reqnroll;

namespace OpenFinancialExchange.BddTests.Steps;

[Binding]
public sealed class BankSteps
{
    private readonly IBankRepository _repository = Substitute.For<IBankRepository>();
    private readonly IUnitOfWork     _unitOfWork  = Substitute.For<IUnitOfWork>();

    private Result<int>    _createResult = default!;
    private Result         _updateResult = default!;
    private Result         _deleteResult = default!;
    private Result<Application.Banks.DTOs.BankDto> _queryResult = default!;

    // ── Given ────────────────────────────────────────────────────────────────

    [Given("no bank exists with COMPE code {string}")]
    public void GivenNoBankWithCompeCode(string code)
        => _repository.COMPECodeExistsAsync(code).Returns(false);

    [Given("a bank already exists with COMPE code {string}")]
    public void GivenBankWithCompeCode(string code)
        => _repository.COMPECodeExistsAsync(code).Returns(true);

    [Given("an existing bank with id {int} and name {string}")]
    public void GivenExistingBank(int id, string name)
    {
        var bankResult = Bank.Create("1111", name, null);
        bankResult.IsSuccess.Should().BeTrue();
        _repository.GetByIdAsync(id).Returns(bankResult.Value);
    }

    [Given("no bank exists with id {int}")]
    public void GivenNoBankWithId(int id)
        => _repository.GetByIdAsync(id).Returns((Bank?)null);

    // ── When ─────────────────────────────────────────────────────────────────

    [When("I create a bank with COMPE code {string}, name {string}, and ISPB {string}")]
    public async Task WhenICreateBank(string compeCode, string name, string ispb)
    {
        var handler = new CreateBankCommandHandler(_repository, _unitOfWork);
        _createResult = await handler.Handle(
            new CreateBankCommand(compeCode, name, ispb),
            CancellationToken.None);
    }

    [When("I update the bank with id {int} to name {string} and ISPB {string}")]
    public async Task WhenIUpdateBank(int id, string name, string ispb)
    {
        var handler = new UpdateBankCommandHandler(_repository, _unitOfWork);
        _updateResult = await handler.Handle(
            new UpdateBankCommand(id, name, ispb),
            CancellationToken.None);
    }

    [When("I get the bank with id {int}")]
    public async Task WhenIGetBank(int id)
    {
        var handler = new GetByIdBankQueryHandler(_repository);
        _queryResult = await handler.Handle(new GetByIdBankQuery(id), CancellationToken.None);
    }

    [When("I delete the bank with id {int}")]
    public async Task WhenIDeleteBank(int id)
    {
        var handler = new DeleteBankCommandHandler(_repository, _unitOfWork);
        _deleteResult = await handler.Handle(new DeleteBankCommand(id), CancellationToken.None);
    }

    // ── Then ─────────────────────────────────────────────────────────────────

    [Then("the bank creation should succeed")]
    public void ThenBankCreationSucceeds()
        => _createResult.IsSuccess.Should().BeTrue();

    [Then("the bank creation should fail with error {string}")]
    public void ThenBankCreationFails(string errorCode)
    {
        _createResult.IsFailure.Should().BeTrue();
        _createResult.Error.Code.Should().Be(errorCode);
    }

    [Then("the bank update should succeed")]
    public void ThenBankUpdateSucceeds()
        => _updateResult.IsSuccess.Should().BeTrue();

    [Then("the bank query should fail with error {string}")]
    public void ThenBankQueryFails(string errorCode)
    {
        _queryResult.IsFailure.Should().BeTrue();
        _queryResult.Error.Code.Should().Be(errorCode);
    }

    [Then("the bank deletion should fail with error {string}")]
    public void ThenBankDeletionFails(string errorCode)
    {
        _deleteResult.IsFailure.Should().BeTrue();
        _deleteResult.Error.Code.Should().Be(errorCode);
    }
}
