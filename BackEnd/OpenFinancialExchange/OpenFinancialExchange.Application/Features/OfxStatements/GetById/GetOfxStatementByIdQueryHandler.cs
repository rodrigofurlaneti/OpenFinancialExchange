using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.OfxStatements.GetById;

internal sealed class GetOfxStatementByIdQueryHandler(IOfxStatementRepository repository)
    : IQueryHandler<GetOfxStatementByIdQuery, OfxStatementResponse>
{
    public async Task<Result<OfxStatementResponse>> Handle(
        GetOfxStatementByIdQuery request, CancellationToken cancellationToken)
    {
        var statement = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (statement is null)
            return Result.Failure<OfxStatementResponse>(new Error("OfxStatement.NotFound",
                $"OFX statement with Id '{request.Id}' was not found."));

        return Result.Success(new OfxStatementResponse(statement.Id, statement.ImportId, statement.BankAccountId,
            statement.TrnUid, statement.CurDef, statement.DtServer, statement.Language,
            statement.StatusCode, statement.StatusSeverity, statement.DtStart, statement.DtEnd, statement.CreatedAt));
    }
}
