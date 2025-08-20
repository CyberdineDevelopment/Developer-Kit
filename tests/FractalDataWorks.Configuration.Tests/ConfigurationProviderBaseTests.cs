using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using Xunit.v3;
using FractalDataWorks.Configuration;

namespace FractalDataWorks.Configuration.Tests;

/// <summary>
/// Tests for ConfigurationProviderBase class.
/// </summary>
public class ConfigurationProviderBaseTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IFdwConfigurationSource> _mockSource;
    private readonly TestConfigurationProvider _provider;

    public ConfigurationProviderBaseTests(ITestOutputHelper output)
    {
        _output = output;
        _mockLogger = new Mock<ILogger>();
        _mockSource = new Mock<IFdwConfigurationSource>();
        _provider = new TestConfigurationProvider(_mockLogger.Object, _mockSource.Object);
    }

    [Fact]
    public async Task GetByIdShouldReturnCachedConfiguration()
    {
        // Arrange
        var config = CreateTestConfiguration(1, "Test Config 1");
        var configs = new List<TestConfiguration> { config };
        
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(configs));

        // Act
        var result = await _provider.Get(1);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(1);
        result.Value.Name.ShouldBe("Test Config 1");

        _output.WriteLine($"Retrieved configuration: Id={result.Value.Id}, Name='{result.Value.Name}'");
    }

    [Fact]
    public async Task GetByIdShouldReturnFailureWhenNotFound()
    {
        // Arrange
        var configs = new List<TestConfiguration>();
        
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(configs));

        // Act
        var result = await _provider.Get(999);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldContain("Configuration with ID 999 not found");

        _output.WriteLine($"Expected not found result: {result.Message}");
    }

    [Fact]
    public async Task GetByIdShouldReturnFailureWhenSourceFails()
    {
        // Arrange
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Failure<IEnumerable<TestConfiguration>>("Source error"));

        // Act
        var result = await _provider.Get(1);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Source error");

        _output.WriteLine($"Expected source error result: {result.Message}");
    }

    [Fact]
    public async Task GetByNameShouldReturnConfiguration()
    {
        // Arrange
        var configs = new List<TestConfiguration>
        {
            CreateTestConfiguration(1, "Config One"),
            CreateTestConfiguration(2, "Config Two")
        };
        
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(configs));

        // Act
        var result = await _provider.Get("Config Two");

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(2);
        result.Value.Name.ShouldBe("Config Two");

        _output.WriteLine($"Retrieved configuration by name: Id={result.Value.Id}, Name='{result.Value.Name}'");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByNameShouldReturnFailureForInvalidName(string? name)
    {
        // Act
        var result = await _provider.Get(name!);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Invalid configuration name");

        _output.WriteLine($"Expected invalid name result for '{name}': {result.Message}");
    }

    [Fact]
    public async Task GetByNameShouldBeCaseInsensitive()
    {
        // Arrange
        var config = CreateTestConfiguration(1, "Test Config");
        var configs = new List<TestConfiguration> { config };
        
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(configs));

        // Act
        var result = await _provider.Get("TEST CONFIG");

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Test Config");

        _output.WriteLine($"Case insensitive match found: '{result.Value.Name}'");
    }

    [Fact]
    public async Task GetAllShouldReturnAllConfigurations()
    {
        // Arrange
        var configs = new List<TestConfiguration>
        {
            CreateTestConfiguration(1, "Config 1"),
            CreateTestConfiguration(2, "Config 2"),
            CreateTestConfiguration(3, "Config 3")
        };
        
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(configs));

        // Act
        var result = await _provider.GetAll();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(3);

        _output.WriteLine($"Retrieved {result.Value.Count()} configurations");
        foreach (var config in result.Value)
        {
            _output.WriteLine($"  - {config.Name} (ID: {config.Id})");
        }
    }

    [Fact]
    public async Task GetEnabledShouldReturnOnlyEnabledConfigurations()
    {
        // Arrange
        var configs = new List<TestConfiguration>
        {
            CreateTestConfiguration(1, "Enabled 1", true),
            CreateTestConfiguration(2, "Disabled", false),
            CreateTestConfiguration(3, "Enabled 2", true)
        };
        
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(configs));

        // Act
        var result = await _provider.GetEnabled();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(2);
        result.Value.ShouldAllBe(c => c.IsEnabled);

        _output.WriteLine($"Retrieved {result.Value.Count()} enabled configurations");
        foreach (var config in result.Value)
        {
            _output.WriteLine($"  - {config.Name} (Enabled: {config.IsEnabled})");
        }
    }

    [Fact]
    public async Task SaveShouldReturnFailureForNullConfiguration()
    {
        // Act
        var result = await _provider.Save(null!);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Configuration cannot be null");

        _output.WriteLine($"Expected null configuration error: {result.Message}");
    }

    [Fact]
    public async Task SaveShouldReturnFailureForInvalidConfiguration()
    {
        // Arrange
        var provider = new TestConfigurationWithValidationProvider(_mockLogger.Object, _mockSource.Object);
        var invalidConfig = new TestConfigurationWithValidation
        {
            Id = 1,
            Name = "", // Invalid - empty name
            IsEnabled = true
        };

        // Act
        var result = await provider.Save(invalidConfig);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldContain("Configuration validation failed");

        _output.WriteLine($"Expected validation error: {result.Message}");
    }

    [Fact]
    public async Task SaveShouldSucceedForValidConfiguration()
    {
        // Arrange
        var config = CreateTestConfiguration(1, "Valid Config");
        
        _mockSource.Setup(s => s.Save(config))
            .ReturnsAsync(FdwResult<TestConfiguration>.Success(config));

        // Act
        var result = await _provider.Save(config);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(config);

        _output.WriteLine($"Successfully saved configuration: {config.Name}");
    }

    [Fact]
    public async Task SaveShouldSetModifiedAtForExistingConfiguration()
    {
        // Arrange
        var config = CreateTestConfiguration(1, "Existing Config");
        var beforeSave = DateTime.UtcNow;
        
        _mockSource.Setup(s => s.Save(It.IsAny<TestConfiguration>()))
            .ReturnsAsync((TestConfiguration c) => FdwResult<TestConfiguration>.Success(c));

        // Act
        var result = await _provider.Save(config);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ModifiedAt.ShouldNotBeNull();
        result.Value.ModifiedAt.Value.ShouldBeGreaterThanOrEqualTo(beforeSave);

        _output.WriteLine($"Configuration modified at: {result.Value.ModifiedAt}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task DeleteShouldReturnFailureForInvalidId(int invalidId)
    {
        // Act
        var result = await _provider.Delete(invalidId);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Invalid configuration ID");

        _output.WriteLine($"Expected invalid ID error for {invalidId}: {result.Message}");
    }

    [Fact]
    public async Task DeleteShouldSucceedForValidId()
    {
        // Arrange
        _mockSource.Setup(s => s.Delete<TestConfiguration>(1))
            .ReturnsAsync(FdwResult<NonResult>.Success(NonResult.Value));

        // Act
        var result = await _provider.Delete(1);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        _output.WriteLine("Successfully deleted configuration");
    }

    [Fact]
    public async Task ReloadShouldClearCacheAndSucceed()
    {
        // Arrange
        var config = CreateTestConfiguration(1, "Cached Config");
        var configs = new List<TestConfiguration> { config };
        
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(configs));

        // First load to populate cache
        await _provider.Get(1);

        // Act
        var reloadResult = await _provider.Reload();

        // Assert
        reloadResult.ShouldNotBeNull();
        reloadResult.IsSuccess.ShouldBeTrue();

        _output.WriteLine("Successfully reloaded configuration cache");
    }

    [Fact]
    public void DisposeShouldUnsubscribeFromSourceChanges()
    {
        // Arrange
        _mockSource.SetupAdd(s => s.Changed += It.IsAny<EventHandler<ConfigurationSourceChangedEventArgs>>());
        _mockSource.SetupRemove(s => s.Changed -= It.IsAny<EventHandler<ConfigurationSourceChangedEventArgs>>());

        // Act
        _provider.Dispose();

        // Assert
        _mockSource.VerifyRemove(s => s.Changed -= It.IsAny<EventHandler<ConfigurationSourceChangedEventArgs>>(), Times.Once);

        _output.WriteLine("Verified event unsubscription on dispose");
    }

    public void Dispose()
    {
        _provider?.Dispose();
    }

    private static TestConfiguration CreateTestConfiguration(int id, string name, bool isEnabled = true)
    {
        return new TestConfiguration
        {
            Id = id,
            Name = name,
            IsEnabled = isEnabled
        };
    }

    // Test implementation of ConfigurationProviderBase
    public class TestConfigurationProvider : ConfigurationProviderBase<TestConfiguration>
    {
        public TestConfigurationProvider(ILogger logger, IFdwConfigurationSource source)
            : base(logger, source)
        {
        }
    }

    // Test implementation for validation testing
    public class TestConfigurationWithValidationProvider : ConfigurationProviderBase<TestConfigurationWithValidation>
    {
        public TestConfigurationWithValidationProvider(ILogger logger, IFdwConfigurationSource source)
            : base(logger, source)
        {
        }
    }

    // Test configuration class
    public class TestConfiguration : ConfigurationBase<TestConfiguration>
    {
        public override string SectionName => "TestSection";
    }

    // Test configuration with validation
    public class TestConfigurationWithValidation : ConfigurationBase<TestConfigurationWithValidation>
    {
        public override string SectionName => "TestSectionWithValidation";

        protected override IValidator<TestConfigurationWithValidation>? GetValidator()
        {
            return new TestValidator();
        }

        private class TestValidator : AbstractValidator<TestConfigurationWithValidation>
        {
            public TestValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithMessage("Name is required");
            }
        }
    }
}