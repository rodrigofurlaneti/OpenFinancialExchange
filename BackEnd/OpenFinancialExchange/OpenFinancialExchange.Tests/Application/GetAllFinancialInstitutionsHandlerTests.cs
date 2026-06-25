using FluentAssertions;
using NSubstitute;
using OpenFinancialExchange.Application.Features.FinancialInstitutions.GetAll;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Tests.Application;

public class GetAllFinancialInstitutionsHandlerTests
{
    private readonly IFinancialInstitutionRepository _repository = Substitute.For<IFinancialInstitutionRepository>();

    [Fact]
    public async Task Handle_ShouldReturnMappedResponses()
    {
        var institutionResult = FinancialInstitution.Create(1, "237", "Bradesco", "237");
        var institutions = new List<FinancialInstitution> { institutionResult.Value };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(institutions);

        var handler = new GetAllFinancialInstitutionsQueryHandler(_repository);
        var result = await handler.Handle(new GetAllFinancialInstitutionsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().BankId.Should().Be("237");
    }

    [Fact]
    public async Task Handle_WhenNoInstitutions_ShouldReturnEmptyCollection()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<FinancialInstitution>());

        var handler = new GetAllFinancialInstitutionsQueryHandler(_repository);
        var result = await handler.Handle(new GetAllFinancialInstitutionsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
