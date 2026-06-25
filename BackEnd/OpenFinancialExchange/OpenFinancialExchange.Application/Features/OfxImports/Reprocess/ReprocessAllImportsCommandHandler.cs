using MediatR;
using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.OfxImports.Reprocess;

internal sealed class ReprocessAllImportsCommandHandler(
    IOfxImportRepository importRepository,
    ISender sender)
    : ICommandHandler<ReprocessAllImportsCommand, ReprocessAllResult>
{
    public async Task<Result<ReprocessAllResult>> Handle(
        ReprocessAllImportsCommand request, CancellationToken cancellationToken)
    {
        var imports = await importRepository.GetAllAsync(cancellationToken);

        var processed = 0;
        var created = 0;

        // Chronological order so per-account FitId dedup accumulates correctly.
        foreach (var import in imports.OrderBy(i => i.Id))
        {
            var result = await sender.Send(new ReprocessImportCommand(import.Id), cancellationToken);
            if (result.IsSuccess)
            {
                processed++;
                created += result.Value;
            }
        }

        return Result.Success(new ReprocessAllResult(processed, created));
    }
}
