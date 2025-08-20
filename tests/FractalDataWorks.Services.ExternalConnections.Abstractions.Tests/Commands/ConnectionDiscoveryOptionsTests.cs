using System;
using Shouldly;
using Xunit;
using FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Tests.Commands;

/// <summary>
/// Tests for the ConnectionDiscoveryOptions class.
/// </summary>
public sealed class ConnectionDiscoveryOptionsTests
{
    [Fact]
    public void ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var options = new ConnectionDiscoveryOptions();

        // Assert
        options.IncludeMetadata.ShouldBeTrue();
        options.IncludeColumns.ShouldBeTrue();
        options.IncludeRelationships.ShouldBeFalse();
        options.IncludeIndexes.ShouldBeFalse();
        options.MaxDepth.ShouldBe(3);
    }

    [Fact]
    public void ShouldAllowSettingIncludeMetadata()
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions();

        // Act
        options.IncludeMetadata = false;

        // Assert
        options.IncludeMetadata.ShouldBeFalse();
    }

    [Fact]
    public void ShouldAllowSettingIncludeColumns()
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions();

        // Act
        options.IncludeColumns = false;

        // Assert
        options.IncludeColumns.ShouldBeFalse();
    }

    [Fact]
    public void ShouldAllowSettingIncludeRelationships()
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions();

        // Act
        options.IncludeRelationships = true;

        // Assert
        options.IncludeRelationships.ShouldBeTrue();
    }

    [Fact]
    public void ShouldAllowSettingIncludeIndexes()
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions();

        // Act
        options.IncludeIndexes = true;

        // Assert
        options.IncludeIndexes.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void ShouldAllowSettingValidMaxDepth(int maxDepth)
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions();

        // Act
        options.MaxDepth = maxDepth;

        // Assert
        options.MaxDepth.ShouldBe(maxDepth);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void ShouldAllowNegativeMaxDepth(int maxDepth)
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions();

        // Act
        options.MaxDepth = maxDepth;

        // Assert
        options.MaxDepth.ShouldBe(maxDepth);
    }

    [Fact]
    public void ShouldSupportPropertyChaining()
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions();

        // Act
        options.IncludeMetadata = true;
        options.IncludeColumns = false;
        options.IncludeRelationships = true;
        options.IncludeIndexes = true;
        options.MaxDepth = 5;

        // Assert
        options.IncludeMetadata.ShouldBeTrue();
        options.IncludeColumns.ShouldBeFalse();
        options.IncludeRelationships.ShouldBeTrue();
        options.IncludeIndexes.ShouldBeTrue();
        options.MaxDepth.ShouldBe(5);
    }

    [Fact]
    public void ShouldCreateMinimalDiscoveryOptions()
    {
        // Arrange & Act
        var options = new ConnectionDiscoveryOptions
        {
            IncludeMetadata = false,
            IncludeColumns = false,
            IncludeRelationships = false,
            IncludeIndexes = false,
            MaxDepth = 0
        };

        // Assert
        options.IncludeMetadata.ShouldBeFalse();
        options.IncludeColumns.ShouldBeFalse();
        options.IncludeRelationships.ShouldBeFalse();
        options.IncludeIndexes.ShouldBeFalse();
        options.MaxDepth.ShouldBe(0);
    }

    [Fact]
    public void ShouldCreateFullDiscoveryOptions()
    {
        // Arrange & Act
        var options = new ConnectionDiscoveryOptions
        {
            IncludeMetadata = true,
            IncludeColumns = true,
            IncludeRelationships = true,
            IncludeIndexes = true,
            MaxDepth = 10
        };

        // Assert
        options.IncludeMetadata.ShouldBeTrue();
        options.IncludeColumns.ShouldBeTrue();
        options.IncludeRelationships.ShouldBeTrue();
        options.IncludeIndexes.ShouldBeTrue();
        options.MaxDepth.ShouldBe(10);
    }

    [Fact]
    public void ShouldBeMutableAfterConstruction()
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions
        {
            IncludeMetadata = false,
            MaxDepth = 1
        };

        // Act
        options.IncludeMetadata = true;
        options.IncludeColumns = false;
        options.MaxDepth = 5;

        // Assert
        options.IncludeMetadata.ShouldBeTrue();
        options.IncludeColumns.ShouldBeFalse();
        options.MaxDepth.ShouldBe(5);
    }

    [Fact]
    public void ShouldHandleExtremeMaxDepthValues()
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions();

        // Act & Assert
        options.MaxDepth = int.MaxValue;
        options.MaxDepth.ShouldBe(int.MaxValue);

        options.MaxDepth = int.MinValue;
        options.MaxDepth.ShouldBe(int.MinValue);
    }
}