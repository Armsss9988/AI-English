using EnglishCoach.SharedKernel.Result;
using FluentAssertions;
using Xunit;

namespace EnglishCoach.UnitTests.Domain;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnValue_WhenCreated()
    {
        // Arrange
        var expected = "test value";

        // Act
        var result = Result<string>.Success(expected);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void Failure_ShouldReturnError_WhenCreated()
    {
        // Arrange
        var error = Error.Validation("Something went wrong");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_ShouldThrow_WhenAccessedOnFailure()
    {
        // Arrange
        var result = Result<string>.Failure(Error.NotFound("User", "123"));

        // Act & Assert
        var act = () => _ = result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_ShouldWork_ForSuccess()
    {
        // Arrange & Act
        Result<string> result = "value";

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value");
    }

    [Fact]
    public void ImplicitConversion_ShouldWork_ForFailure()
    {
        // Arrange & Act
        Result<string> result = Error.Internal("fail");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INTERNAL_ERROR");
    }

    [Fact]
    public void Map_ShouldTransformValue_WhenSuccess()
    {
        // Arrange
        var result = Result<string>.Success("hello");

        // Act
        var mapped = result.Map(s => s.ToUpper());

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("HELLO");
    }

    [Fact]
    public void Map_ShouldPropagateError_WhenFailure()
    {
        // Arrange
        var error = Error.Validation("invalid");
        var result = Result<string>.Failure(error);

        // Act
        var mapped = result.Map(s => s.ToUpper());

        // Assert
        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(error);
    }

    [Fact]
    public void UnwrapOr_ShouldReturnValue_WhenSuccess()
    {
        // Arrange
        var result = Result<string>.Success("value");

        // Act
        var value = result.UnwrapOr("default");

        // Assert
        value.Should().Be("value");
    }

    [Fact]
    public void UnwrapOr_ShouldReturnDefault_WhenFailure()
    {
        // Arrange
        var result = Result<string>.Failure(Error.Internal());

        // Act
        var value = result.UnwrapOr("default");

        // Assert
        value.Should().Be("default");
    }
}