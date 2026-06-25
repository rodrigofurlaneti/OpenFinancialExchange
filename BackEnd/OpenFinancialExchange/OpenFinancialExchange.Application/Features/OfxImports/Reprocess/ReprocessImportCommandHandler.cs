using OpenFinancialExchange.Application.Abstractions;
using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Application.Common;
using OpenFinancialExchange.Application.Common.Parsers;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.OfxImports.Reprocess;

internal sealed class ReprocessImportCommandHandler(
    IOfxImportRepository importRepository,
    IOfxStatementRepository statementRepository,
    IOfxTransactionRepository transactionRepository,
    IFinancialInstitutionRepository financialInstitutionRepository,
    IBankAccountRepository bankAccountRepository,
    ICategoryRepository categoryRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReprocessImportCommand, int>
{
    public async Task<Result<int>> Handle(ReprocessImportCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            return Result.Failure<int>(new Error("Auth.Unauthorized", "No authenticated user."));

        var import = await importRepository.GetByIdAsync(request.ImportId, cancellationToken);
        if (import is null)
            return Result.Failure<int>(new Error("OfxImport.NotFound",
                $"Import with Id '{request.ImportId}' was not found."));

        // 1. Wipe any existing statements for this import (cascade transactions/balances).
        await statementRepository.RemoveByImportAsync(request.ImportId, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(import.OfxData))
            return Result.Success(0);

        // 2. Re-parse the stored OFX body.
        var parsed = OfxSgmlParser.Parse(import.OfxData);
        if (parsed is null)
            return Result.Success(0);

        // 3. Find-or-create the financial institution.
        var institution = await financialInstitutionRepository.FindByBankIdAsync(parsed.BankId, cancellationToken);
        if (institution is null)
        {
            var fiResult = FinancialInstitution.Create(userId, parsed.BankId, null, null);
            if (fiResult.IsFailure) return Result.Failure<int>(fiResult.Error);
            await financialInstitutionRepository.AddAsync(fiResult.Value, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            institution = fiResult.Value;
        }

        // 4. Find-or-create the bank account.
        var account = await bankAccountRepository.FindByAcctIdAsync(parsed.AcctId, parsed.BankId, cancellationToken);
        if (account is null)
        {
            var accResult = BankAccount.Create(userId, institution.Id, parsed.BankId, null, parsed.AcctId, parsed.AcctType);
            if (accResult.IsFailure) return Result.Failure<int>(accResult.Error);
            await bankAccountRepository.AddAsync(accResult.Value, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            account = accResult.Value;
        }

        // 5. Create the statement.
        var stmtResult = OfxStatement.Create(
            userId, import.Id, account.Id, parsed.TrnUid, parsed.CurDef,
            parsed.DtServer, parsed.Language, parsed.StatusCode, parsed.StatusSeverity,
            parsed.DtStart, parsed.DtEnd);
        if (stmtResult.IsFailure) return Result.Failure<int>(stmtResult.Error);

        await statementRepository.AddAsync(stmtResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        var statementId = stmtResult.Value.Id;

        // 6. Create transactions (dedupe by FitId across the account).
        if (parsed.Transactions.Count == 0)
            return Result.Success(0);

        var incomingFitIds = parsed.Transactions.Where(t => t.FitId is not null).Select(t => t.FitId!);
        var existingFitIds = await transactionRepository.GetExistingFitIdsAsync(account.Id, incomingFitIds, cancellationToken);
        // Dedupe against the DB AND within this batch (some OFX files repeat the same FitId).
        var seenFitIds = new HashSet<string>(existingFitIds, StringComparer.Ordinal);

        // Auto-categorize by keyword rules (food, transport, internal movements, ...).
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        var categoryKeywords = categories.Select(c => (c.Id, (string?)c.Keywords)).ToList();

        var transactions = new List<OfxTransaction>(parsed.Transactions.Count);
        foreach (var t in parsed.Transactions)
        {
            if (t.FitId is not null && !seenFitIds.Add(t.FitId))
                continue;

            var trnResult = OfxTransaction.Create(
                userId, statementId, t.TrnType, t.DtPosted, t.TrnAmt,
                t.FitId, t.Name, t.Memo, t.CheckNum);
            if (trnResult.IsFailure)
                continue;

            var matchedCategoryId = CategoryKeywordMatcher.Match(categoryKeywords, t.Name, t.Memo);
            if (matchedCategoryId is not null)
                trnResult.Value.AssignCategory(matchedCategoryId);

            transactions.Add(trnResult.Value);
        }

        if (transactions.Count > 0)
        {
            await transactionRepository.AddRangeAsync(transactions, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return Result.Success(transactions.Count);
    }
}
