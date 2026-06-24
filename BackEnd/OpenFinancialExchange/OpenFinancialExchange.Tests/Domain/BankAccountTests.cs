using FluentAssertions;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Tests.Domain;

public class BankAccountTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var result = BankAccount.Create(1, "237", "1234", "000123456-7", "CHECKING");

        result.IsSuccess.Should().BeTrue();
        result.Value.AcctType.Should().Be("CHECKING");
        result.Value.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("")]
    [InlineData("CURRENT")]
    public void Create_WithInvalidAcctType_ShouldFail(string acctType)
    {
        var result = BankAccount.Create(1, "237", null, "123456", acctType);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("BankAccount.InvalidAcctType");
    }

    [Fact]
    public void Create_WithInvalidInstitutionId_ShouldFail()
    {
        var result = BankAccount.Create(0, "237", null, "123456", "CHECKING");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("BankAccount.InvalidInstitution");
    }

    [Fact]
    public void Create_AcctTypeIsCaseInsensitive()
    {
        var result = BankAccount.Create(1, "237", null, "123456", "checking");

        result.IsSuccess.Should().BeTrue();
        result.Value.AcctType.Should().Be("CHECKING");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var account = BankAccount.Create(1, "237", null, "123456", "SAVINGS").Value;

        account.Deactivate();

        account.IsActive.Should().BeFalse();
    }
}
