using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;
using FractalDataWorks.Results;

namespace FractalDataWorks.Results.Tests;

/// <summary>
/// Tests for NonResult struct.
/// Note: NonResult is marked with [ExcludeFromCodeCoverage] but we test its public API for completeness.
/// </summary>
public class NonResultTests
{
    [Fact]
    public void NonResultShouldImplementIEquatable()
    {
        // Arrange
        var nonResultType = typeof(NonResult);

        // Act
        var interfaces = nonResultType.GetInterfaces();

        // Assert
        interfaces.ShouldContain(typeof(IEquatable<NonResult>));
    }

    [Fact]
    public void NonResultShouldBeValueType()
    {
        // Arrange & Act
        var nonResultType = typeof(NonResult);

        // Assert
        nonResultType.IsValueType.ShouldBeTrue();
        nonResultType.IsClass.ShouldBeFalse();
    }

    [Fact]
    public void NonResultValueShouldExist()
    {
        // Arrange & Act
        var value = NonResult.Value;

        // Assert - Just check that we can access it without exceptions
        value.ShouldBeOfType<NonResult>();
    }

    [Fact]
    public void NonResultDefaultShouldEqualValue()
    {
        // Arrange
        var defaultNonResult = default(NonResult);
        var explicitValue = NonResult.Value;

        // Act & Assert
        defaultNonResult.Equals(explicitValue).ShouldBeTrue();
        explicitValue.Equals(defaultNonResult).ShouldBeTrue();
    }

    [Fact]
    public void NonResultEqualsShouldReturnTrueForAllInstances()
    {
        // Arrange
        var first = new NonResult();
        var second = new NonResult();
        var third = default(NonResult);
        var fourth = NonResult.Value;

        // Act & Assert
        first.Equals(second).ShouldBeTrue();
        second.Equals(third).ShouldBeTrue();
        third.Equals(fourth).ShouldBeTrue();
        first.Equals(fourth).ShouldBeTrue();
    }

    [Fact]
    public void NonResultEqualsWithObjectShouldWork()
    {
        // Arrange
        var nonResult = new NonResult();
        object nonResultAsObject = new NonResult();
        object notANonResult = "not a NonResult";

        // Act & Assert
        nonResult.Equals(nonResultAsObject).ShouldBeTrue();
        nonResult.Equals(notANonResult).ShouldBeFalse();
        nonResult.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void NonResultGetHashCodeShouldBeConsistent()
    {
        // Arrange
        var first = new NonResult();
        var second = new NonResult();
        var third = default(NonResult);

        // Act
        var hash1 = first.GetHashCode();
        var hash2 = second.GetHashCode();
        var hash3 = third.GetHashCode();

        // Assert
        hash1.ShouldBe(hash2);
        hash2.ShouldBe(hash3);
        hash1.ShouldBe(0); // According to implementation, should always return 0
    }

    [Fact]
    public void NonResultToStringShouldReturnExpectedValue()
    {
        // Arrange
        var nonResult = new NonResult();

        // Act
        var stringRepresentation = nonResult.ToString();

        // Assert
        stringRepresentation.ShouldBe("NonResult");
    }

    [Fact]
    public void NonResultEqualityOperatorShouldWork()
    {
        // Arrange
        var first = new NonResult();
        var second = new NonResult();

        // Act & Assert
        (first == second).ShouldBeTrue();
        (first != second).ShouldBeFalse();
    }

    [Fact]
    public void NonResultInequalityOperatorShouldWork()
    {
        // Arrange
        var first = new NonResult();
        var second = new NonResult();

        // Act & Assert
        (first != second).ShouldBeFalse();
        (first == second).ShouldBeTrue();
    }

    [Fact]
    public void NonResultOperatorsShouldAlwaysReturnSameResults()
    {
        // Arrange
        var instances = new[]
        {
            new NonResult(),
            default(NonResult),
            NonResult.Value
        };

        // Act & Assert
        for (int i = 0; i < instances.Length; i++)
        {
            for (int j = 0; j < instances.Length; j++)
            {
                (instances[i] == instances[j]).ShouldBeTrue();
                (instances[i] != instances[j]).ShouldBeFalse();
            }
        }
    }

    [Fact]
    public void NonResultShouldWorkAsGenericTypeParameter()
    {
        // Arrange & Act
        var result = FdwResult<NonResult>.Success(NonResult.Value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<NonResult>();
        result.Value.ToString().ShouldBe("NonResult");
    }

    [Fact]
    public void NonResultShouldSupportCollections()
    {
        // Arrange
        var nonResults = new[]
        {
            new NonResult(),
            default(NonResult),
            NonResult.Value
        };

        // Act & Assert
        nonResults.Length.ShouldBe(3);
        nonResults.All(nr => nr.Equals(NonResult.Value)).ShouldBeTrue();
        nonResults.All(nr => nr.GetHashCode() == 0).ShouldBeTrue();
        nonResults.All(nr => nr.ToString() == "NonResult").ShouldBeTrue();
    }

    [Theory]
    [InlineData("NonResult")]
    public void NonResultToStringShouldMatchExpected(string expected)
    {
        // Arrange
        var nonResult = NonResult.Value;

        // Act
        var result = nonResult.ToString();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void NonResultShouldBeEquatableInHashSet()
    {
        // Arrange
        var hashSet = new HashSet<NonResult>();

        // Act
        hashSet.Add(new NonResult());
        hashSet.Add(default(NonResult));
        hashSet.Add(NonResult.Value);

        // Assert
        hashSet.Count.ShouldBe(1); // All should be considered equal
    }

    [Fact]
    public void NonResultShouldWorkInDictionary()
    {
        // Arrange
        var dictionary = new Dictionary<NonResult, string>();

        // Act
        dictionary[new NonResult()] = "first";
        dictionary[default(NonResult)] = "second"; // Should overwrite
        dictionary[NonResult.Value] = "third"; // Should overwrite again

        // Assert
        dictionary.Count.ShouldBe(1);
        dictionary[NonResult.Value].ShouldBe("third");
    }
}