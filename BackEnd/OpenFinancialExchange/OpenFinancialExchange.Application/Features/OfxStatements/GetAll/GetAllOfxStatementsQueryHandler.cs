using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.OfxStatements.GetAll;

internal sealed class GetAllOfxStatementsQueryHandler(IOfxStatementRepository repository)
    : IQueryHandler<GetAllOfxStatementsQuery, IReadOnlyCollection<OfxStatementResponse>>
{
    public async Task<Result<IReadOnlyCollection<OfxStatementResponse>>> Handle(
        GetAllOfxStatementsQuery request, CancellationToken cancellationToken)
    {
        var statements = await repository.GetAllAsync(cancellationToken);
        var response = statements
            .Select(s => new OfxStatementResponse(s.Id, s.ImportId, s.BankAccountId, s.TrnUid,
                s.CurDef, s.DtServer, s.Language, s.StatusCode, s.StatusSeverity, s.DtStart, s.DtEnd, s.CreatedAt))
            .ToList().AsReadOnly();
        return Result.Success<IReadOnlyCollection<OfxStatementResponse>>(response);
    }
}
