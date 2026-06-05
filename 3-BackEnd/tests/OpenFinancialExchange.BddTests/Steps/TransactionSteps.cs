using FluentAssertions;
using NSubstitute;
using OpenFinancialExchange.Application.Transactions.Commands.Create;
using OpenFinancialExchange.Application.Transactions.Commands.Reconcile;
using OpenFinancialExchange.Application.Transactions.DTOs;
using OpenFinancialExchange.Application.Transactions.Queries.GetByDateRange;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;
using Reqnroll;

namespace OpenFinancialExchange.BddTests.Steps;

[Binding]
public sealed class TransactionSteps
{
    private readonly ITransactionRepository _repository = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork            _unitOfWork  = Substitute.For<IUnitOfWork>();

    private Result<int>                          _createResult      = default!;
    private Result                               _reconcileResult   = default!;
    private Result<IReadOnlyList<TransactionDto>> _dateRangeResult  = default!;

    // ── Given ────────────────────────────────────────────────────────────────

    [Given("no transaction exists with FITID {string}")]
    public void GivenNoTransactionWithFITID(string fitid)
        => _repository.FITIDExistsAsync(fitid).Returns(false);

    [Given("a transaction already exists with FITID {string}")]
    public void GivenTransactionWithFITID(string fitid)
        => _repository.FITIDExistsAsync(fitid).Returns(true);

    [Given("an existing unreconciled transaction with id {int}")]
    public void GivenUnreconciledTransaction(int id)
    {
        var txResult = Transaction.Create(
            statementId: 1, categoryId: null,
            transactionType: "CREDIT",
            postedDateRaw: "20260213000000[-03:EST]",
            postedDate: new DateOnly(2026, 2, 13),
            timeZone: "-03:EST",
            amount: 100m,
            fitid: $"FITID-{id}",
            checkNumber: null, memo: "Test",
            payeeName: null, transactionDateMemo: null, operationSubtype: null);

        txResult.IsSuccess.Should().BeTrue();
        _repository.GetByIdAsync(id).Returns(txResult.Value);
    }

    [Given("an existing already reconciled transaction with id {int}")]
    public void GivenReconciledTransaction(int id)
    {
        var txResult = Transaction.Create(
            statementId: 1, categoryId: null,
            transactionType: "CREDIT",
            postedDateRaw: "20260213000000[-03:EST]",
            postedDate: new DateOnly(2026, 2, 13),
            timeZone: "-03:EST",
            amount: 200m,
            fitid: $"FITID-REC-{id}",
            checkNumber: null, memo: "Reconciled",
            payeeName: null, transactionDateMemo: null, operationSubtype: null);

        txResult.IsSuccess.Should().BeTrue();
        var entity = txResult.Value;
        entity.Reconcile(); // mark as reconciled so second call fails
        _repository.GetByIdAsync(id).Returns(entity);
    }

    [Given("transactions exist between {string} and {string}")]
    public void GivenTransactionsBetweenDates(string from, string to)
    {
        var fromDate = DateOnly.Parse(from);
        var toDate   = DateOnly.Parse(to);

        var txResult = Transaction.Create(
            statementId: 1, categoryId: null,
            transactionType: "CREDIT",
            postedDateRaw: "20260213000000[-03:EST]",
            postedDate: fromDate,
            timeZone: "-03:EST",
            amount: 500m,
            fitid: "FITID-RANGE-001",
            checkNumber: null, memo: "Range test",
            payeeName: null, transactionDateMemo: null, operationSubtype: null);

        txResult.IsSuccess.Should().BeTrue();
        IReadOnlyList<Transaction> list = [txResult.Value];
        _repository.GetByDateRangeAsync(fromDate, toDate).Returns(list);
    }

    // ── When ─────────────────────────────────────────────────────────────────

    [When("I create a transaction with FITID {string}, amount {decimal}, type {string}, and statementId {int}")]
    public async Task WhenICreateTransaction(string fitid, decimal amount, string type, int statementId)
    {
        var handler = new CreateTransactionCommandHandler(_repository, _unitOfWork);
        _createResult = await handler.Handle(
            new CreateTransactionCommand(
                StatementId: statementId,
                CategoryId: null,
                TransactionType: type,
                PostedDateRaw: "20260213000000[-03:EST]",
                PostedDate: new DateOnly(2026, 2, 13),
                TimeZone: "-03:EST",
                Amount: amount,
                FITID: fitid,
                CheckNumber: null,
                Memo: "Test transaction",
                PayeeName: null,
                TransactionDateMemo: null,
                OperationSubtype: null),
            CancellationToken.None);
    }

    [When("I reconcile the transaction with id {int}")]
    public async Task WhenIReconcileTransaction(int id)
    {
        var handler = new ReconcileTransactionCommandHandler(_repository, _unitOfWork);
        _reconcileResult = await handler.Handle(
            new ReconcileTransactionCommand(id),
            CancellationToken.None);
    }

    [When("I query transactions from {string} to {string}")]
    public async Task WhenIQueryTransactionsByDateRange(string from, string to)
    {
        var handler = new GetTransactionsByDateRangeQueryHandler(_repository);
        _dateRangeResult = await handler.Handle(
            new GetTransactionsByDateRangeQuery(DateOnly.Parse(from), DateOnly.Parse(to)),
            CancellationToken.None);
    }

    // ── Then ─────────────────────────────────────────────────────────────────

    [Then("the transaction creation should succeed")]
    public void ThenTransactionCreationSucceeds()
        => _createResult.IsSuccess.Should().BeTrue();

    [Then("the transaction creation should fail with error {string}")]
    public void ThenTransactionCreationFails(string errorCode)
    {
        _createResult.IsFailure.Should().BeTrue();
        _createResult.Error.Code.Should().Be(errorCode);
    }

    [Then("the reconciliation should succeed")]
    public void ThenReconciliationSucceeds()
        => _reconcileResult.IsSuccess.Should().BeTrue();

    [Then("the reconciliation should fail with error {string}")]
    public void ThenReconciliationFails(string errorCode)
    {
        _reconcileResult.IsFailure.Should().BeTrue();
        _reconcileResult.Error.Code.Should().Be(errorCode);
    }

    [Then("the date range query should return results")]
    public void ThenDateRangeQueryReturnsResults()
    {
        _dateRangeResult.IsSuccess.Should().BeTrue();
        _dateRangeResult.Value.Should().NotBeEmpty();
    }
}
