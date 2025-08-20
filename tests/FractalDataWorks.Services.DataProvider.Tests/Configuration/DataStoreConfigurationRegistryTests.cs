using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Services.DataProvider.Abstractions.Configuration;
using FractalDataWorks.Services.DataProvider.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.DataProvider.Tests.Configuration;

/// <summary>
/// Tests for the DataStoreConfigurationRegistry implementation.
/// </summary>
public sealed class DataStoreConfigurationRegistryTests : IDisposable
{
    private readonly Mock<ILogger<DataStoreConfigurationRegistry>> _loggerMock;
    private readonly Mock<IOptionsMonitor<DataStoreConfiguration[]>> _optionsMonitorMock;
    private readonly List<Action<DataStoreConfiguration[]>> _changeCallbacks = new();

    public DataStoreConfigurationRegistryTests()
    {
        _loggerMock = new Mock<ILogger<DataStoreConfigurationRegistry>>();
        _optionsMonitorMock = new Mock<IOptionsMonitor<DataStoreConfiguration[]>>();

        // Setup options monitor to track change callbacks
        _optionsMonitorMock
            .Setup(m => m.OnChange(It.IsAny<Action<DataStoreConfiguration[]>>()))
            .Returns<Action<DataStoreConfiguration[]>>(callback =>
            {
                _changeCallbacks.Add(callback);
                return Mock.Of<IDisposable>();
            });
    }

    [Fact]
    public void ConstructorShouldThrowWhenLoggerIsNull()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new DataStoreConfigurationRegistry(null!, _optionsMonitorMock.Object));

        exception.ParamName.ShouldBe("logger");

    }

    [Fact]
    public void ConstructorShouldThrowWhenOptionsMonitorIsNull()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new DataStoreConfigurationRegistry(_loggerMock.Object, null!));

        exception.ParamName.ShouldBe("optionsMonitor");

    }

    [Fact]
    public void ConstructorShouldCreateRegistryWithValidParameters()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        // Act
        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Assert
        registry.ShouldNotBeNull();
        registry.Count.ShouldBe(2);

    }

    [Fact]
    public void ConstructorShouldHandleNullCurrentValue()
    {
        // Arrange
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns((DataStoreConfiguration[]?)null);

        // Act
        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Assert
        registry.ShouldNotBeNull();
        registry.Count.ShouldBe(0);

    }

    [Fact]
    public void GetShouldReturnConfigurationById()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.Get(1);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(1);
        result.StoreName.ShouldBe("TestStore1");

    }

    [Fact]
    public void GetShouldReturnNullWhenIdNotFound()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.Get(999);

        // Assert
        result.ShouldBeNull();

    }

    [Fact]
    public void GetAllShouldReturnAllConfigurations()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.GetAll();

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(2);

    }

    [Fact]
    public void TryGetShouldReturnTrueWhenConfigurationExists()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.TryGet(1, out var configuration);

        // Assert
        result.ShouldBeTrue();
        configuration.ShouldNotBeNull();
        configuration!.Id.ShouldBe(1);

    }

    [Fact]
    public void TryGetShouldReturnFalseWhenConfigurationNotExists()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.TryGet(999, out var configuration);

        // Assert
        result.ShouldBeFalse();
        configuration.ShouldBeNull();

    }

    [Fact]
    public void GetByNameShouldReturnConfigurationByStoreName()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.GetByName("TestStore1");

        // Assert
        result.ShouldNotBeNull();
        result!.StoreName.ShouldBe("TestStore1");
        result.Id.ShouldBe(1);

    }

    [Fact]
    public void GetByNameShouldReturnNullWhenStoreNameNotFound()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.GetByName("NonExistentStore");

        // Assert
        result.ShouldBeNull();

    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetByNameShouldThrowWhenStoreNameIsInvalid(string? storeName)
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => registry.GetByName(storeName!));

        exception.ParamName.ShouldBe("name");

    }

    [Fact]
    public void GetByProviderShouldReturnConfigurationsByProviderType()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.GetByProvider("SqlServer");

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
        result.First().StoreName.ShouldBe("TestStore1");

    }

    [Fact]
    public void GetByProviderShouldReturnEmptyWhenProviderTypeNotFound()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.GetByProvider("NonExistentProvider");

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(0);

    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetByProviderShouldThrowWhenProviderTypeIsInvalid(string? providerType)
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => registry.GetByProvider(providerType!));

        exception.ParamName.ShouldBe("providerType");

    }

    [Fact]
    public void GetDefaultShouldReturnFirstEnabledConfiguration()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.GetDefault();

        // Assert
        result.ShouldNotBeNull();
        result!.StoreName.ShouldBe("TestStore1"); // First enabled configuration

    }

    [Fact]
    public void GetDefaultShouldReturnNullWhenNoConfigurationsExist()
    {
        // Arrange
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(Array.Empty<DataStoreConfiguration>());

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.GetDefault();

        // Assert
        result.ShouldBeNull();

    }

    [Fact]
    public void IsEnabledShouldReturnTrueWhenStoreIsEnabled()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.IsEnabled("TestStore1");

        // Assert
        result.ShouldBeTrue();

    }

    [Fact]
    public void IsEnabledShouldReturnFalseWhenStoreIsDisabled()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.IsEnabled("TestStore2");

        // Assert
        result.ShouldBeFalse();

    }

    [Fact]
    public void IsEnabledShouldReturnFalseWhenStoreNotExists()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.IsEnabled("NonExistentStore");

        // Assert
        result.ShouldBeFalse();

    }

    [Fact]
    public void TryGetByNameShouldReturnTrueWhenStoreExists()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.TryGetByName("TestStore1", out var configuration);

        // Assert
        result.ShouldBeTrue();
        configuration.ShouldNotBeNull();
        configuration!.StoreName.ShouldBe("TestStore1");

    }

    [Fact]
    public void TryGetByNameShouldReturnFalseWhenStoreNotExists()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.TryGetByName("NonExistentStore", out var configuration);

        // Assert
        result.ShouldBeFalse();
        configuration.ShouldBeNull();

    }

    [Fact]
    public void TryGetByNameShouldReturnFalseWhenStoreNameIsInvalid()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.TryGetByName(string.Empty, out var configuration);

        // Assert
        result.ShouldBeFalse();
        configuration.ShouldBeNull();

    }

    [Fact]
    public void GetAllEnabledShouldReturnOnlyEnabledConfigurations()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        var result = registry.GetAllEnabled();

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
        result.First().StoreName.ShouldBe("TestStore1");

    }

    [Fact]
    public void CountShouldReturnCorrectTotalCount()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act & Assert
        registry.Count.ShouldBe(2);

    }

    [Fact]
    public void EnabledCountShouldReturnCorrectEnabledCount()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act & Assert
        registry.EnabledCount.ShouldBe(1);

    }

    [Fact]
    public void ConfigurationChangeShouldTriggerRefresh()
    {
        // Arrange
        var initialConfigurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(initialConfigurations);

        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);
        registry.Count.ShouldBe(2);

        // Act - Simulate configuration change
        var newConfigurations = new[]
        {
            new DataStoreConfiguration
            {
                Id = 3,
                StoreName = "TestStore3",
                ProviderType = "PostgreSql",
                IsEnabled = true,
                ExtendedProperties = new Dictionary<string, object>(StringComparer.Ordinal)
            }
        };

        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(newConfigurations);

        // Trigger the change callback
        foreach (var callback in _changeCallbacks)
        {
            callback(newConfigurations);
        }

        // Assert
        registry.Count.ShouldBe(1);
        registry.GetByName("TestStore3").ShouldNotBeNull();
        registry.GetByName("TestStore1").ShouldBeNull(); // Should be removed

    }

    [Fact]
    public void RegistryShouldIgnoreInvalidConfigurations()
    {
        // Arrange
        var configurations = new[]
        {
            new DataStoreConfiguration
            {
                Id = 1,
                StoreName = "ValidStore",
                ProviderType = "SqlServer",
                IsEnabled = true,
                ExtendedProperties = new Dictionary<string, object>(StringComparer.Ordinal)
            },
            new DataStoreConfiguration
            {
                Id = 2,
                StoreName = "", // Invalid - empty name
                ProviderType = "SqlServer",
                IsEnabled = true,
                ExtendedProperties = new Dictionary<string, object>(StringComparer.Ordinal)
            },
            new DataStoreConfiguration
            {
                Id = 3,
                StoreName = "InvalidStore",
                ProviderType = "", // Invalid - empty provider type
                IsEnabled = true,
                ExtendedProperties = new Dictionary<string, object>(StringComparer.Ordinal)
            }
        };

        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        // Act
        using var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Assert
        registry.Count.ShouldBe(1); // Only the valid configuration should be included
        registry.GetByName("ValidStore").ShouldNotBeNull();

    }

    [Fact]
    public void DisposeShouldCleanupResourcesGracefully()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act
        registry.Dispose();

        // Assert - Should not throw when calling disposed methods
        Should.Throw<ObjectDisposedException>(() => registry.Count);
        Should.Throw<ObjectDisposedException>(() => registry.GetAll());

    }

    [Fact]
    public void DisposeMultipleTimesShouldNotThrow()
    {
        // Arrange
        var configurations = CreateTestConfigurations();
        _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(configurations);

        var registry = new DataStoreConfigurationRegistry(_loggerMock.Object, _optionsMonitorMock.Object);

        // Act & Assert - Multiple disposes should not throw
        registry.Dispose();
        Should.NotThrow(() => registry.Dispose());
        Should.NotThrow(() => registry.Dispose());

    }

    private static DataStoreConfiguration[] CreateTestConfigurations()
    {
        return new[]
        {
            new DataStoreConfiguration
            {
                Id = 1,
                StoreName = "TestStore1",
                ProviderType = "SqlServer",
                IsEnabled = true,
                ExtendedProperties = new Dictionary<string, object>(StringComparer.Ordinal)
            },
            new DataStoreConfiguration
            {
                Id = 2,
                StoreName = "TestStore2",
                ProviderType = "PostgreSql",
                IsEnabled = false,
                ExtendedProperties = new Dictionary<string, object>(StringComparer.Ordinal)
            }
        };
    }

    public void Dispose()
    {
        // Cleanup test resources if needed
        _changeCallbacks.Clear();
    }
}