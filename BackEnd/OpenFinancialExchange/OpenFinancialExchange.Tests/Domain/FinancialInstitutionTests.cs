using FluentAssertions;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Tests.Domain;

public class FinancialInstitutionTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var result = FinancialInstitution.Create(1, "237", "Bradesco", "237");

        result.IsSuccess.Should().BeTrue();
        result.Value.BankId.Should().Be("237");
        result.Value.OrgName.Should().Be("Bradesco");
        result.Value.Fid.Should().Be("237");
        result.Value.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyBankId_ShouldFail(string bankId)
    {
        var result = FinancialInstitution.Create(1, bankId, "Bradesco", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FinancialInstitution.EmptyBankId");
    }

    [Fact]
    public void Create_WithBankIdTooLong_ShouldFail()
    {
        var result = FinancialInstitution.Create(1, new string('X', 21), null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FinancialInstitution.BankIdTooLong");
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateOrgNameAndFid()
    {
        var fi = FinancialInstitution.Create(1, "237", null, null).Value;

        var result = fi.UpdateDetails("Bradesco", "237");

        result.IsSuccess.Should().BeTrue();
        fi.OrgName.Should().Be("Bradesco");
        fi.Fid.Should().Be("237");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var fi = FinancialInstitution.Create(1, "001", null, null).Value;

        fi.Deactivate();

        fi.IsActive.Should().BeFalse();
    }
}
