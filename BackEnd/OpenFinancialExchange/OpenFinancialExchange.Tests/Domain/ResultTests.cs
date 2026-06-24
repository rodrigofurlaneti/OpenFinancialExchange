using FluentAssertions;
using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.Tests.Domain;

public class ResultTests
{
    [Fact]
    public void Success_ShouldHaveIsSuccessTrue()
    {
        var result = Result.Success();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void SuccessOfT_ShouldReturnValue()
    {
        var result = Result.Success(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_ShouldHaveIsFailureTrue()
    {
        var error = new Error("Test.Error", "Something went wrong");
        var result = Result.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void FailureOfT_AccessingValue_ShouldThrow()
    {
        var result = Result.Failure<int>(new Error("Test.Error", "Error"));
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SuccessWithNoneError_ShouldBeValid()
    {
        var result = Result.Success();
        result.Error.Should().Be(Error.None);
        result.Error.Code.Should().BeEmpty();
    }
}
