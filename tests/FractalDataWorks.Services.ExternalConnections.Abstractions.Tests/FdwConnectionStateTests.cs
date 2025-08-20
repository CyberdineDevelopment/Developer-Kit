using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Tests;

/// <summary>
/// Tests for the FdwConnectionState enumeration.
/// </summary>
public sealed class FdwConnectionStateTests
{
    [Fact]
    public void ShouldHaveUnknownAsDefaultValue()
    {
        // Arrange & Act
        var defaultState = default(FdwConnectionState);

        // Assert
        defaultState.ShouldBe(FdwConnectionState.Unknown);
    }

    [Fact]
    public void ShouldHaveCorrectUnderlyingValues()
    {
        // Arrange & Act & Assert
        ((int)FdwConnectionState.Unknown).ShouldBe(0);
        ((int)FdwConnectionState.Created).ShouldBe(1);
        ((int)FdwConnectionState.Opening).ShouldBe(2);
        ((int)FdwConnectionState.Open).ShouldBe(3);
        ((int)FdwConnectionState.Executing).ShouldBe(4);
        ((int)FdwConnectionState.Closing).ShouldBe(5);
        ((int)FdwConnectionState.Closed).ShouldBe(6);
        ((int)FdwConnectionState.Broken).ShouldBe(7);
        ((int)FdwConnectionState.Disposed).ShouldBe(8);
    }

    [Fact]
    public void ShouldHaveAllExpectedStates()
    {
        // Arrange
        var expectedStates = new[]
        {
            FdwConnectionState.Unknown,
            FdwConnectionState.Created,
            FdwConnectionState.Opening,
            FdwConnectionState.Open,
            FdwConnectionState.Executing,
            FdwConnectionState.Closing,
            FdwConnectionState.Closed,
            FdwConnectionState.Broken,
            FdwConnectionState.Disposed
        };

        // Act
        var actualStates = Enum.GetValues<FdwConnectionState>();

        // Assert
        actualStates.Length.ShouldBe(expectedStates.Length);
        foreach (var expectedState in expectedStates)
        {
            actualStates.ShouldContain(expectedState);
        }
    }

    [Fact]
    public void ShouldHaveUniqueValues()
    {
        // Arrange & Act
        var allValues = Enum.GetValues<FdwConnectionState>().Cast<int>().ToArray();
        var distinctValues = allValues.Distinct().ToArray();

        // Assert
        allValues.Length.ShouldBe(distinctValues.Length, "All enum values should be unique");
    }

    [Theory]
    [InlineData(FdwConnectionState.Unknown)]
    [InlineData(FdwConnectionState.Created)]
    [InlineData(FdwConnectionState.Opening)]
    [InlineData(FdwConnectionState.Open)]
    [InlineData(FdwConnectionState.Executing)]
    [InlineData(FdwConnectionState.Closing)]
    [InlineData(FdwConnectionState.Closed)]
    [InlineData(FdwConnectionState.Broken)]
    [InlineData(FdwConnectionState.Disposed)]
    public void ShouldConvertToStringCorrectly(FdwConnectionState state)
    {
        // Arrange & Act
        var stringValue = state.ToString();

        // Assert
        stringValue.ShouldNotBeNullOrWhiteSpace();
        stringValue.ShouldBe(Enum.GetName(state));
    }

    [Fact]
    public void ShouldParseFromStringCorrectly()
    {
        // Arrange & Act & Assert
        Enum.Parse<FdwConnectionState>("Unknown").ShouldBe(FdwConnectionState.Unknown);
        Enum.Parse<FdwConnectionState>("Created").ShouldBe(FdwConnectionState.Created);
        Enum.Parse<FdwConnectionState>("Opening").ShouldBe(FdwConnectionState.Opening);
        Enum.Parse<FdwConnectionState>("Open").ShouldBe(FdwConnectionState.Open);
        Enum.Parse<FdwConnectionState>("Executing").ShouldBe(FdwConnectionState.Executing);
        Enum.Parse<FdwConnectionState>("Closing").ShouldBe(FdwConnectionState.Closing);
        Enum.Parse<FdwConnectionState>("Closed").ShouldBe(FdwConnectionState.Closed);
        Enum.Parse<FdwConnectionState>("Broken").ShouldBe(FdwConnectionState.Broken);
        Enum.Parse<FdwConnectionState>("Disposed").ShouldBe(FdwConnectionState.Disposed);
    }

    [Fact]
    public void ShouldThrowWhenParsingInvalidString()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() => Enum.Parse<FdwConnectionState>("InvalidState"));
    }

    [Fact]
    public void ShouldSupportIsDefined()
    {
        // Arrange & Act & Assert
        Enum.IsDefined(FdwConnectionState.Unknown).ShouldBeTrue();
        Enum.IsDefined(FdwConnectionState.Open).ShouldBeTrue();
        Enum.IsDefined((FdwConnectionState)999).ShouldBeFalse();
    }

    [Theory]
    [InlineData(FdwConnectionState.Created, FdwConnectionState.Opening)]
    [InlineData(FdwConnectionState.Opening, FdwConnectionState.Open)]
    [InlineData(FdwConnectionState.Open, FdwConnectionState.Executing)]
    [InlineData(FdwConnectionState.Executing, FdwConnectionState.Open)]
    [InlineData(FdwConnectionState.Open, FdwConnectionState.Closing)]
    [InlineData(FdwConnectionState.Closing, FdwConnectionState.Closed)]
    public void ShouldSupportValidStateTransitions(FdwConnectionState fromState, FdwConnectionState toState)
    {
        // Arrange & Act
        var isValidTransition = IsValidStateTransition(fromState, toState);

        // Assert
        isValidTransition.ShouldBeTrue($"Transition from {fromState} to {toState} should be valid");
    }

    [Theory]
    [InlineData(FdwConnectionState.Closed, FdwConnectionState.Opening)]
    [InlineData(FdwConnectionState.Disposed, FdwConnectionState.Open)]
    [InlineData(FdwConnectionState.Broken, FdwConnectionState.Executing)]
    public void ShouldIdentifyInvalidStateTransitions(FdwConnectionState fromState, FdwConnectionState toState)
    {
        // Arrange & Act
        var isValidTransition = IsValidStateTransition(fromState, toState);

        // Assert
        isValidTransition.ShouldBeFalse($"Transition from {fromState} to {toState} should be invalid");
    }

    [Fact]
    public void ShouldSupportComparisonOperations()
    {
        // Arrange & Act & Assert
        (FdwConnectionState.Created < FdwConnectionState.Open).ShouldBeTrue();
        (FdwConnectionState.Open > FdwConnectionState.Created).ShouldBeTrue();
        FdwConnectionState.Closed.ShouldBe(FdwConnectionState.Closed);
        FdwConnectionState.Unknown.ShouldNotBe(FdwConnectionState.Disposed);
    }

    private static bool IsValidStateTransition(FdwConnectionState fromState, FdwConnectionState toState)
    {
        return (fromState, toState) switch
        {
            // Valid transitions
            (FdwConnectionState.Unknown, FdwConnectionState.Created) => true,
            (FdwConnectionState.Created, FdwConnectionState.Opening) => true,
            (FdwConnectionState.Opening, FdwConnectionState.Open) => true,
            (FdwConnectionState.Opening, FdwConnectionState.Broken) => true,
            (FdwConnectionState.Open, FdwConnectionState.Executing) => true,
            (FdwConnectionState.Open, FdwConnectionState.Closing) => true,
            (FdwConnectionState.Executing, FdwConnectionState.Open) => true,
            (FdwConnectionState.Executing, FdwConnectionState.Broken) => true,
            (FdwConnectionState.Closing, FdwConnectionState.Closed) => true,
            (FdwConnectionState.Closing, FdwConnectionState.Broken) => true,
            (FdwConnectionState.Closed, FdwConnectionState.Disposed) => true,
            (FdwConnectionState.Broken, FdwConnectionState.Disposed) => true,
            
            // Any state can transition to Broken
            (_, FdwConnectionState.Broken) => true,
            
            // Any state (except Disposed) can transition to Disposed
            (not FdwConnectionState.Disposed, FdwConnectionState.Disposed) => true,
            
            // Same state transitions are valid
            var (from, to) when from == to => true,
            
            // All other transitions are invalid
            _ => false
        };
    }
}