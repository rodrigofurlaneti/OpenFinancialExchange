using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.OfxImports.Reprocess;

/// <summary>Reprocesses every stored import for the current user.</summary>
public sealed record ReprocessAllImportsCommand : ICommand<ReprocessAllResult>;

public sealed record ReprocessAllResult(int ImportsProcessed, int TransactionsCreated);
