using System;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.SecretManagement.Abstractions.Tests;

/// <summary>
/// Essential tests for HealthStatus enum to achieve code coverage.
/// </summary>
public sealed class SimpleHealthStatusTests
{
    [Fact]
    public void HealthStatusShouldHaveCorrectValues()
    {
        // Assert all enum values have expected integer values
        ((int)HealthStatus.Unknown).ShouldBe(0);
        ((int)HealthStatus.Healthy).ShouldBe(1);
        ((int)HealthStatus.Warning).ShouldBe(2);
        ((int)HealthStatus.Degraded).ShouldBe(3);
        ((int)HealthStatus.Unhealthy).ShouldBe(4);
        ((int)HealthStatus.Critical).ShouldBe(5);
    }

    [Theory]
    [InlineData(HealthStatus.Unknown, 0)]
    [InlineData(HealthStatus.Healthy, 1)]
    [InlineData(HealthStatus.Warning, 2)]
    [InlineData(HealthStatus.Degraded, 3)]
    [InlineData(HealthStatus.Unhealthy, 4)]
    [InlineData(HealthStatus.Critical, 5)]
    public void HealthStatusValuesShouldMatchExpectedIntegers(HealthStatus status, int expectedValue)
    {
        ((int)status).ShouldBe(expectedValue);
    }

    [Fact]
    public void HealthStatusShouldBeDefinedForAllValidValues()
    {
        var validValues = new[] { 0, 1, 2, 3, 4, 5 };
        
        foreach (var value in validValues)
        {
            var status = (HealthStatus)value;
            Enum.IsDefined(typeof(HealthStatus), status).ShouldBeTrue();
        }
    }

    [Fact]
    public void HealthStatusShouldParseFromValidStrings()
    {
        Enum.Parse<HealthStatus>("Unknown").ShouldBe(HealthStatus.Unknown);
        Enum.Parse<HealthStatus>("Healthy").ShouldBe(HealthStatus.Healthy);
        Enum.Parse<HealthStatus>("Warning").ShouldBe(HealthStatus.Warning);
        Enum.Parse<HealthStatus>("Degraded").ShouldBe(HealthStatus.Degraded);
        Enum.Parse<HealthStatus>("Unhealthy").ShouldBe(HealthStatus.Unhealthy);
        Enum.Parse<HealthStatus>("Critical").ShouldBe(HealthStatus.Critical);
    }

    [Fact]
    public void HealthStatusShouldThrowWhenParsingInvalidStrings()
    {
        Should.Throw<ArgumentException>(() => Enum.Parse<HealthStatus>("Invalid"));
    }
}