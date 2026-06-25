using OpenFinancialExchange.Application.Abstractions;
using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Application.Common;
using OpenFinancialExchange.Application.Common.Parsers;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.OfxImports.Create;

internal sealed class CreateOfxImportCommandHandler(
    IOfxImportRepository repository,
    IOfxStatementRepository statementRepository,
    IOfxTransactionRepository transactionRepository,
    ICategoryRepository categoryRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOfxImportCommand, long>
{
    public async Task<Result<long>> Handle(CreateOfxImportCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            return Result.Failure<long>(new Error("Auth.Unauthorized", "No authenticated user."));

        // 1. Duplicate check
        var alreadyImported = await repository.ExistsByHashAsync(request.FileHash, cancellationToken);
        if (alreadyImported)
            return Result.Failure<long>(new Error("OfxImport.Duplicate",
                "This file has already been imported (duplicate SHA-256 hash)."));

        // 2. Create and persist the raw import record
        var importResult = OfxImport.Create(userId, request.FileName, request.FileHash, request.OfxHeaderVersion,
            request.OfxVersion, request.OfxData, request.Encoding, request.Charset,
            request.Security, request.Compression, request.OldFileUid, request.NewFileUid);

        if (importResult.IsFailure)
            return Result.Failure<long>(importResult.Error);

        await repository.AddAsync(importResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);          // DB-generated Id
        var importId = importResult.Value.Id;

        // 3. Only proceed if the caller told us which account this OFX belongs to
        if (request.BankAccountId is null or <= 0 || string.IsNullOrWhiteSpace(request.OfxData))
            return Result.Success(importId);

        // 4. Parse the OFX SGML body
        var parsed = OfxSgmlParser.Parse(request.OfxData);
        if (parsed is null)
            return Result.Success(importId);

        // 5. Create and persist the statement
        var stmtResult = OfxStatement.Create(
            userId, importId, request.BankAccountId.Value, parsed.TrnUid, parsed.CurDef,
            parsed.DtServer, parsed.Language, parsed.StatusCode, parsed.StatusSeverity,
            parsed.DtStart, parsed.DtEnd);

        if (stmtResult.IsFailure)
            return Result.Success(importId);

        await statementRepository.AddAsync(stmtResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);          // DB-generated Id
        var statementId = stmtResult.Value.Id;

        // 6. Create and persist transactions (skip duplicates by FitId)
        if (parsed.Transactions.Count == 0)
            return Result.Success(importId);

        // One query to find which FitIds already exist for this bank account
        var incomingFitIds = parsed.Transactions
            .Where(t => t.FitId is not null)
            .Select(t => t.FitId!);

        var existingFitIds = await transactionRepository
            .GetExistingFitIdsAsync(request.BankAccountId.Value, incomingFitIds, cancellationToken);
        // Dedupe against the DB AND within this batch (some OFX files repeat the same FitId).
        var seenFitIds = new HashSet<string>(existingFitIds, StringComparer.Ordinal);

        // Auto-categorize by keyword rules (food, transport, internal movements, ...).
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        var categoryKeywords = categories.Select(c => (c.Id, (string?)c.Keywords)).ToList();

        var transactions = new List<OfxTransaction>(parsed.Transactions.Count);
        foreach (var t in parsed.Transactions)
        {
            // Skip if FitId already exists in the DB or earlier in this same batch
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

        return Result.Success(importId);
    }
}
