using System;
using Shouldly;
using Xunit;
using Xunit.v3;
using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Configuration.Abstractions.Tests;

/// <summary>
/// Tests for ConfigurationSourceChangedEventArgs class.
/// Note: This class is marked with [ExcludeFromCodeCoverage] but we test it to ensure basic functionality.
/// </summary>
public class ConfigurationSourceChangedEventArgsTests
{
    private readonly ITestOutputHelper _output;

    public ConfigurationSourceChangedEventArgsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConstructorShouldSetPropertiesCorrectlyWithoutConfigurationId()
    {
        // Arrange
        var changeType = ConfigurationChangeType.Added;
        var configurationType = typeof(string);
        var beforeConstruction = DateTime.UtcNow;

        // Act
        var eventArgs = new ConfigurationSourceChangedEventArgs(changeType, configurationType);

        // Assert
        eventArgs.ChangeType.ShouldBe(changeType);
        eventArgs.ConfigurationType.ShouldBe(configurationType);
        eventArgs.ConfigurationId.ShouldBeNull();
        eventArgs.ChangedAt.ShouldBeGreaterThanOrEqualTo(beforeConstruction);
        eventArgs.ChangedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);

        _output.WriteLine($"EventArgs created: ChangeType={eventArgs.ChangeType}, ConfigurationType={eventArgs.ConfigurationType.Name}, ChangedAt={eventArgs.ChangedAt}");
    }

    [Fact]
    public void ConstructorShouldSetPropertiesCorrectlyWithConfigurationId()
    {
        // Arrange
        var changeType = ConfigurationChangeType.Updated;
        var configurationType = typeof(int);
        var configurationId = 42;
        var beforeConstruction = DateTime.UtcNow;

        // Act
        var eventArgs = new ConfigurationSourceChangedEventArgs(changeType, configurationType, configurationId);

        // Assert
        eventArgs.ChangeType.ShouldBe(changeType);
        eventArgs.ConfigurationType.ShouldBe(configurationType);
        eventArgs.ConfigurationId.ShouldBe(configurationId);
        eventArgs.ChangedAt.ShouldBeGreaterThanOrEqualTo(beforeConstruction);
        eventArgs.ChangedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);

        _output.WriteLine($"EventArgs created: ChangeType={eventArgs.ChangeType}, ConfigurationType={eventArgs.ConfigurationType.Name}, ConfigurationId={eventArgs.ConfigurationId}, ChangedAt={eventArgs.ChangedAt}");
    }

    [Theory]
    [InlineData(ConfigurationChangeType.Added)]
    [InlineData(ConfigurationChangeType.Updated)]
    [InlineData(ConfigurationChangeType.Deleted)]
    [InlineData(ConfigurationChangeType.Reloaded)]
    public void ConstructorShouldAcceptAllChangeTypes(ConfigurationChangeType changeType)
    {
        // Arrange
        var configurationType = typeof(object);

        // Act
        var eventArgs = new ConfigurationSourceChangedEventArgs(changeType, configurationType);

        // Assert
        eventArgs.ChangeType.ShouldBe(changeType);
        _output.WriteLine($"Successfully created EventArgs with ChangeType: {changeType}");
    }

    [Fact]
    public void ConstructorShouldAcceptNullConfigurationId()
    {
        // Arrange
        var changeType = ConfigurationChangeType.Deleted;
        var configurationType = typeof(bool);

        // Act
        var eventArgs = new ConfigurationSourceChangedEventArgs(changeType, configurationType, null);

        // Assert
        eventArgs.ConfigurationId.ShouldBeNull();
        _output.WriteLine("Successfully created EventArgs with null ConfigurationId");
    }

    [Fact]
    public void ShouldInheritFromEventArgs()
    {
        // Arrange
        var eventArgs = new ConfigurationSourceChangedEventArgs(ConfigurationChangeType.Added, typeof(string));

        // Act & Assert
        eventArgs.ShouldBeAssignableTo<EventArgs>();
        _output.WriteLine("ConfigurationSourceChangedEventArgs correctly inherits from EventArgs");
    }
}