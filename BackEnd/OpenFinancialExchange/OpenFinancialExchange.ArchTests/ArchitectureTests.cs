using FluentAssertions;
using NetArchTest.Rules;
using OpenFinancialExchange.Application.Abstractions;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Infrastructure.Persistence;

namespace OpenFinancialExchange.ArchTests;

public class ArchitectureTests
{
    private static readonly string DomainNamespace = "OpenFinancialExchange.Domain";
    private static readonly string ApplicationNamespace = "OpenFinancialExchange.Application";
    private static readonly string InfrastructureNamespace = "OpenFinancialExchange.Infrastructure";
    private static readonly string ApiNamespace = "OpenFinancialExchange.API";

    private static Types DomainTypes => Types.InAssembly(typeof(Entity).Assembly);
    private static Types ApplicationTypes => Types.InAssembly(typeof(DependencyInjection).Assembly);
    private static Types InfrastructureTypes => Types.InAssembly(typeof(AppDbContext).Assembly);

    // ─── Domain isolation (no external dependencies) ───────────────────

    [Fact]
    public void Domain_ShouldNotReferenceApplication()
    {
        DomainTypes.That().ResideInNamespace(DomainNamespace)
            .ShouldNot().HaveDependencyOn(ApplicationNamespace)
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_ShouldNotReferenceInfrastructure()
    {
        DomainTypes.That().ResideInNamespace(DomainNamespace)
            .ShouldNot().HaveDependencyOn(InfrastructureNamespace)
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_ShouldNotReferenceApi()
    {
        DomainTypes.That().ResideInNamespace(DomainNamespace)
            .ShouldNot().HaveDependencyOn(ApiNamespace)
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_ShouldNotReferenceMediatR()
    {
        DomainTypes.That().ResideInNamespace(DomainNamespace)
            .ShouldNot().HaveDependencyOn("MediatR")
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_ShouldNotReferenceEntityFramework()
    {
        DomainTypes.That().ResideInNamespace(DomainNamespace)
            .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_ShouldNotReferenceFluentValidation()
    {
        DomainTypes.That().ResideInNamespace(DomainNamespace)
            .ShouldNot().HaveDependencyOn("FluentValidation")
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── Application isolation ──────────────────────────────────────────

    [Fact]
    public void Application_ShouldNotReferenceInfrastructure()
    {
        ApplicationTypes.That().ResideInNamespace(ApplicationNamespace)
            .ShouldNot().HaveDependencyOn(InfrastructureNamespace)
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_ShouldNotReferenceApi()
    {
        ApplicationTypes.That().ResideInNamespace(ApplicationNamespace)
            .ShouldNot().HaveDependencyOn(ApiNamespace)
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_ShouldNotReferenceEntityFramework()
    {
        ApplicationTypes.That().ResideInNamespace(ApplicationNamespace)
            .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── Infrastructure isolation ───────────────────────────────────────

    [Fact]
    public void Infrastructure_ShouldNotReferenceApi()
    {
        InfrastructureTypes.That().ResideInNamespace(InfrastructureNamespace)
            .ShouldNot().HaveDependencyOn(ApiNamespace)
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── Handlers must be internal sealed ───────────────────────────────

    [Fact]
    public void CommandHandlers_ShouldBeInternalSealed()
    {
        ApplicationTypes.That()
            .HaveNameEndingWith("CommandHandler")
            .Should().BeSealed()
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void QueryHandlers_ShouldBeInternalSealed()
    {
        ApplicationTypes.That()
            .HaveNameEndingWith("QueryHandler")
            .Should().BeSealed()
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── Repositories must be internal sealed ───────────────────────────

    [Fact]
    public void Repositories_ShouldBeInternalSealed()
    {
        InfrastructureTypes.That()
            .HaveNameEndingWith("Repository")
            .Should().BeSealed()
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── Commands naming convention ──────────────────────────────────────

    [Fact]
    public void Commands_ShouldResideInFeaturesNamespace()
    {
        ApplicationTypes.That()
            .HaveNameEndingWith("Command")
            .Should().ResideInNamespace($"{ApplicationNamespace}.Features")
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Queries_ShouldResideInFeaturesNamespace()
    {
        ApplicationTypes.That()
            .HaveNameEndingWith("Query")
            .Should().ResideInNamespace($"{ApplicationNamespace}.Features")
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Validators_ShouldResideInFeaturesNamespace()
    {
        ApplicationTypes.That()
            .HaveNameEndingWith("Validator")
            .Should().ResideInNamespace($"{ApplicationNamespace}.Features")
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── Aggregates extend AggregateRoot ────────────────────────────────

    [Fact]
    public void FinancialInstitution_ShouldExtendAggregateRoot()
    {
        DomainTypes.That()
            .HaveNameEndingWith("FinancialInstitution")
            .Should().Inherit(typeof(global::OpenFinancialExchange.Domain.Primitives.AggregateRoot))
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void BankAccount_ShouldExtendAggregateRoot()
    {
        DomainTypes.That()
            .HaveName("BankAccount")
            .Should().Inherit(typeof(global::OpenFinancialExchange.Domain.Primitives.AggregateRoot))
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void OfxImport_ShouldExtendAggregateRoot()
    {
        DomainTypes.That()
            .HaveName("OfxImport")
            .Should().Inherit(typeof(global::OpenFinancialExchange.Domain.Primitives.AggregateRoot))
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void OfxStatement_ShouldExtendAggregateRoot()
    {
        DomainTypes.That()
            .HaveName("OfxStatement")
            .Should().Inherit(typeof(global::OpenFinancialExchange.Domain.Primitives.AggregateRoot))
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── EF Configurations are internal sealed ───────────────────────────

    [Fact]
    public void EfConfigurations_ShouldBeInternalSealed()
    {
        InfrastructureTypes.That()
            .HaveNameEndingWith("Configuration")
            .And().ImplementInterface(typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>))
            .Should().BeSealed()
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── Response DTOs are sealed records ───────────────────────────────

    [Fact]
    public void ResponseDtos_ShouldBeSealed()
    {
        ApplicationTypes.That()
            .HaveNameEndingWith("Response")
            .Should().BeSealed()
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── Entities reside in Domain.Entities namespace ───────────────────

    [Fact]
    public void DomainEntities_ShouldResideInEntitiesNamespace()
    {
        DomainTypes.That()
            .Inherit(typeof(Entity))
            .And().DoNotHaveName("Entity")
            .And().DoNotHaveName("AggregateRoot")
            .Should().ResideInNamespace($"{DomainNamespace}.Entities")
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── Primitives reside in Domain.Primitives namespace ───────────────

    [Fact]
    public void DomainPrimitives_ShouldResideInPrimitivesNamespace()
    {
        DomainTypes.That()
            .ResideInNamespace($"{DomainNamespace}.Primitives")
            .Should().ResideInNamespace($"{DomainNamespace}.Primitives")
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── Repositories interfaces reside in Domain.Repositories ──────────

    [Fact]
    public void RepositoryInterfaces_ShouldResideInDomainRepositories()
    {
        DomainTypes.That()
            .AreInterfaces()
            .And().HaveNameEndingWith("Repository")
            .Should().ResideInNamespace($"{DomainNamespace}.Repositories")
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── Infrastructure repositories implement domain interfaces ─────────

    [Fact]
    public void InfrastructureRepositories_ShouldResideInPersistenceNamespace()
    {
        InfrastructureTypes.That()
            .HaveNameEndingWith("Repository")
            .Should().ResideInNamespace($"{InfrastructureNamespace}.Persistence.Repositories")
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── EF Configurations reside in proper namespace ────────────────────

    [Fact]
    public void EfConfigurations_ShouldResideInConfigurationsNamespace()
    {
        InfrastructureTypes.That()
            .HaveNameEndingWith("Configuration")
            .And().ImplementInterface(typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>))
            .Should().ResideInNamespace($"{InfrastructureNamespace}.Persistence.Configurations")
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── Validators extend AbstractValidator ─────────────────────────────

    [Fact]
    public void Validators_ShouldInheritAbstractValidator()
    {
        ApplicationTypes.That()
            .HaveNameEndingWith("Validator")
            .Should().Inherit(typeof(FluentValidation.AbstractValidator<>))
            .GetResult().IsSuccessful.Should().BeTrue();
    }

    // ─── IUnitOfWork interface is in Domain.Repositories ─────────────────

    [Fact]
    public void IUnitOfWork_ShouldResideInDomainRepositories()
    {
        DomainTypes.That()
            .HaveName("IUnitOfWork")
            .Should().ResideInNamespace($"{DomainNamespace}.Repositories")
            .GetResult().IsSuccessful.Should().BeTrue();
    }
}
