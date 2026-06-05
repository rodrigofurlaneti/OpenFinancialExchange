using FluentAssertions;
using NSubstitute;
using OpenFinancialExchange.Application.TransactionCategories.Commands.Create;
using OpenFinancialExchange.Application.TransactionCategories.Commands.Delete;
using OpenFinancialExchange.Application.TransactionCategories.Commands.Update;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;
using Reqnroll;

namespace OpenFinancialExchange.BddTests.Steps;

[Binding]
public sealed class TransactionCategorySteps
{
    private readonly ITransactionCategoryRepository _repository =
        Substitute.For<ITransactionCategoryRepository>();

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private Result<int>  _createResult = default!;
    private Result       _updateResult = default!;
    private Result       _deleteResult = default!;

    // ── Given ────────────────────────────────────────────────────────────────

    [Given("no transaction category exists with code {string}")]
    public void GivenNoTransactionCategoryWithCode(string code)
        => _repository.CodeExistsAsync(code).Returns(false);

    [Given("a transaction category already exists with code {string}")]
    public void GivenTransactionCategoryWithCode(string code)
        => _repository.CodeExistsAsync(code).Returns(true);

    [Given("an existing transaction category with id {int} and description {string}")]
    public void GivenExistingTransactionCategory(int id, string description)
    {
        var categoryResult = TransactionCategory.Create("EXISTING", description, "OP_TYPE", "REVENUE");
        categoryResult.IsSuccess.Should().BeTrue();
        _repository.GetByIdAsync(id).Returns(categoryResult.Value);
    }

    [Given("no transaction category exists with id {int}")]
    public void GivenNoTransactionCategoryWithId(int id)
        => _repository.GetByIdAsync(id).Returns((TransactionCategory?)null);

    // ── When ─────────────────────────────────────────────────────────────────

    [When("I create a transaction category with code {string}, description {string}, operationType {string}, and accountingNature {string}")]
    public async Task WhenICreateTransactionCategory(
        string code, string description, string operationType, string accountingNature)
    {
        var handler = new CreateTransactionCategoryCommandHandler(_repository, _unitOfWork);
        _createResult = await handler.Handle(
            new CreateTransactionCategoryCommand(code, description, operationType, accountingNature),
            CancellationToken.None);
    }

    [When("I update the transaction category with id {int} to description {string}, operationType {string}, and accountingNature {string}")]
    public async Task WhenIUpdateTransactionCategory(
        int id, string description, string operationType, string accountingNature)
    {
        var handler = new UpdateTransactionCategoryCommandHandler(_repository, _unitOfWork);
        _updateResult = await handler.Handle(
            new UpdateTransactionCategoryCommand(id, description, operationType, accountingNature),
            CancellationToken.None);
    }

    [When("I delete the transaction category with id {int}")]
    public async Task WhenIDeleteTransactionCategory(int id)
    {
        var handler = new DeleteTransactionCategoryCommandHandler(_repository, _unitOfWork);
        _deleteResult = await handler.Handle(
            new DeleteTransactionCategoryCommand(id),
            CancellationToken.None);
    }

    // ── Then ─────────────────────────────────────────────────────────────────

    [Then("the transaction category creation should succeed")]
    public void ThenTransactionCategoryCreationSucceeds()
        => _createResult.IsSuccess.Should().BeTrue();

    [Then("the transaction category creation should fail with error {string}")]
    public void ThenTransactionCategoryCreationFails(string errorCode)
    {
        _createResult.IsFailure.Should().BeTrue();
        _createResult.Error.Code.Should().Be(errorCode);
    }

    [Then("the transaction category update should succeed")]
    public void ThenTransactionCategoryUpdateSucceeds()
        => _updateResult.IsSuccess.Should().BeTrue();

    [Then("the transaction category deletion should fail with error {string}")]
    public void ThenTransactionCategoryDeletionFails(string errorCode)
    {
        _deleteResult.IsFailure.Should().BeTrue();
        _deleteResult.Error.Code.Should().Be(errorCode);
    }
}
