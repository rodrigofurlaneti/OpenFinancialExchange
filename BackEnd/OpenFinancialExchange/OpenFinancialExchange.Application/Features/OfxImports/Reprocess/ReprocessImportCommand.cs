using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.OfxImports.Reprocess;

/// <summary>
/// Re-parses the stored OfxData of one import: wipes its statements/transactions
/// and recreates institution, account, statement and transactions. Returns the
/// number of transactions created.
/// </summary>
public sealed record ReprocessImportCommand(long ImportId) : ICommand<int>;
