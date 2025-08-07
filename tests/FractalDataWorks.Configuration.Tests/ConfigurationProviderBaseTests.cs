using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FractalDataWorks;
using FractalDataWorks.Configuration;


using FractalDataWorks.Results;
using FractalDataWorks.Validation;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Configuration.Tests;

/// <summary>
/// Tests for ConfigurationProviderBase class.
/// </summary>
public class ConfigurationProviderBaseTests : IDisposable
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IFdwConfigurationSource> _mockSource;
    private readonly TestConfigurationProvider _provider;
    private readonly List<TestConfiguration> _testConfigs;

    public ConfigurationProviderBaseTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockSource = new Mock<IFdwConfigurationSource>();
        _provider = new TestConfigurationProvider(_mockLogger.Object, _mockSource.Object);
        
        _testConfigs = new List<TestConfiguration>
        {
            new TestConfiguration { Id = 1, Name = "Config1", IsEnabled = true },
            new TestConfiguration { Id = 2, Name = "Config2", IsEnabled = false },
            new TestConfiguration { Id = 3, Name = "Config3", IsEnabled = true }
        };
    }

    [Fact]
    public async Task GetByIdReturnsConfigurationFromCache()
    {
        // Arrange
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs));

        // Act - First call loads from source
        var result1 = await _provider.Get(1);
        
        // Act - Second call should come from cache
        var result2 = await _provider.Get(1);

        // Assert
        result1.IsSuccess.ShouldBeTrue();
        result1.Value.Id.ShouldBe(1);
        result2.IsSuccess.ShouldBeTrue();
        result2.Value.Id.ShouldBe(1);
        
        // Source should only be called once
        _mockSource.Verify(s => s.Load<TestConfiguration>(), Times.Once);
    }

    [Fact]
    public async Task GetByIdReturnsFailureForInvalidId()
    {
        // Act
        var result = await _provider.Get(-1);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByIdReturnsFailureWhenNotFound()
    {
        // Arrange
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs));

        // Act
        var result = await _provider.Get(999);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByNameReturnsConfiguration()
    {
        // Arrange
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs));

        // Act
        var result = await _provider.Get("Config2");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(2);
        result.Value.Name.ShouldBe("Config2");
    }

    [Fact]
    public async Task GetByNameReturnsFailureForNullOrEmpty()
    {
        // Act
        var result1 = await _provider.Get(null!);
        var result2 = await _provider.Get("");
        var result3 = await _provider.Get("   ");

        // Assert
        result1.IsSuccess.ShouldBeFalse();
        result2.IsSuccess.ShouldBeFalse();
        result3.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByNameIsCaseInsensitive()
    {
        // Arrange
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs));

        // Act
        var result = await _provider.Get("CONFIG1");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Config1");
    }

    [Fact]
    public async Task GetAllReturnsAllConfigurations()
    {
        // Arrange
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs));

        // Act
        var result = await _provider.GetAll();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count().ShouldBe(3);
    }

    [Fact]
    public async Task GetAllCachesResults()
    {
        // Arrange
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs));

        // Act
        var result1 = await _provider.GetAll();
        var result2 = await _provider.GetAll();

        // Assert
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();
        
        // Source should only be called once
        _mockSource.Verify(s => s.Load<TestConfiguration>(), Times.Once);
    }

    [Fact]
    public async Task GetEnabledReturnsOnlyEnabledConfigurations()
    {
        // Arrange
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs));

        // Act
        var result = await _provider.GetEnabled();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count().ShouldBe(2);
        result.Value.All(c => c.IsEnabled).ShouldBeTrue();
    }

    [Fact]
    public async Task SaveUpdatesExistingConfiguration()
    {
        // Arrange
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs));
        _mockSource.Setup(s => s.Save(It.IsAny<TestConfiguration>()))
            .ReturnsAsync((TestConfiguration config) => FdwResult<TestConfiguration>.Success(config));

        var updatedConfig = new TestConfiguration { Id = 1, Name = "Updated", IsEnabled = false };

        // Act
        var result = await _provider.Save(updatedConfig);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Updated");
        result.Value.IsEnabled.ShouldBeFalse();
        
        // Verify cache was cleared
        var getResult = await _provider.Get(1);
        getResult.Value.Name.ShouldBe("Updated");
    }

    [Fact]
    public async Task SaveAddsNewConfiguration()
    {
        // Arrange
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs));
        _mockSource.Setup(s => s.Save(It.IsAny<TestConfiguration>()))
            .ReturnsAsync((TestConfiguration config) => FdwResult<TestConfiguration>.Success(config));

        var newConfig = new TestConfiguration { Id = 4, Name = "New", IsEnabled = true };

        // Act
        var result = await _provider.Save(newConfig);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _mockSource.Verify(s => s.Save(newConfig), Times.Once);
    }

    [Fact]
    public async Task SaveReturnsFailureForNullConfiguration()
    {
        // Act
        var result = await _provider.Save(null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
    }

    [Fact]
    public async Task SaveValidatesConfigurationBeforeSaving()
    {
        // Arrange
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs));

        var invalidConfig = new TestConfiguration { Id = 1, Name = "Invalid", IsEnabled = true, RequiredProperty = null };

        // Act
        var result = await _provider.Save(invalidConfig);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        _mockSource.Verify(s => s.Save(It.IsAny<TestConfiguration>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRemovesConfiguration()
    {
        // Arrange
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs));
        _mockSource.Setup(s => s.Delete<TestConfiguration>(It.IsAny<int>()))
            .ReturnsAsync(() => FdwResult<NonResult>.Success(NonResult.Value));

        // Act
        var result = await _provider.Delete(1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _mockSource.Verify(s => s.Delete<TestConfiguration>(1), Times.Once);
    }

    [Fact]
    public async Task DeleteReturnsFailureForInvalidId()
    {
        // Act
        var result = await _provider.Delete(-1);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteReturnsFailureWhenNotFound()
    {
        // Arrange
        _mockSource.Setup(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs));

        // Act
        var result = await _provider.Delete(999);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task ReloadClearsCacheAndReloadsFromSource()
    {
        // Arrange
        _mockSource.SetupSequence(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs))
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(new List<TestConfiguration>
            {
                new TestConfiguration { Id = 1, Name = "Reloaded", IsEnabled = true }
            }));

        // Act
        var result1 = await _provider.Get(1);
        var reloadResult = await _provider.Reload();
        var result2 = await _provider.Get(1);

        // Assert
        result1.Value.Name.ShouldBe("Config1");
        reloadResult.IsSuccess.ShouldBeTrue();
        result2.Value.Name.ShouldBe("Reloaded");
        
        _mockSource.Verify(s => s.Load<TestConfiguration>(), Times.Exactly(2));
    }

    [Fact]
    public async Task ValidateCallsConfigurationValidateAsync()
    {
        // Arrange
        var config = new TestConfiguration { Id = 1, IsEnabled = true, RequiredProperty = "Valid" };

        // Act
        var result = await _provider.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task SourceLoadFailureReturnsCachedData()
    {
        // Arrange
        var testConfigs2 = new List<TestConfiguration> { _testConfigs[0] }; // different data
        _mockSource.SetupSequence(s => s.Load<TestConfiguration>())
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(_testConfigs))
            .ReturnsAsync(FdwResult<IEnumerable<TestConfiguration>>.Success(testConfigs2));

        // Act
        var result1 = await _provider.GetAll();
        var reloadResult = await _provider.Reload();
        var result2 = await _provider.GetAll();

        // Assert
        result1.IsSuccess.ShouldBeTrue();
        result1.Value.Count().ShouldBe(3);
        reloadResult.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();
        result2.Value.Count().ShouldBe(1); // Should have new data
    }

    [Fact]
    public void DisposeCleansUpResources()
    {
        // Arrange
        var provider = new TestConfigurationProvider(_mockLogger.Object, _mockSource.Object);

        // Act
        provider.Dispose();
        provider.Dispose(); // Second call should not throw

        // Assert
        provider.IsDisposed.ShouldBeTrue();
    }

    public void Dispose()
    {
        _provider?.Dispose();
    }

    // Test classes
    private class TestConfigurationProvider : ConfigurationProviderBase<TestConfiguration>
    {
        public bool IsDisposed { get; private set; }

        public TestConfigurationProvider(ILogger logger, IFdwConfigurationSource source) 
            : base(logger, source)
        {
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    private class TestConfiguration : ConfigurationBase<TestConfiguration>
    {
        public string? RequiredProperty { get; set; }

        public override string SectionName => "Test";

        protected override IValidator<TestConfiguration> GetValidator()
        {
            return new TestConfigurationValidator();
        }
    }

    private class TestConfigurationValidator : AbstractValidator<TestConfiguration>
    {
        public TestConfigurationValidator()
        {
            RuleFor(x => x.RequiredProperty)
                .NotEmpty()
                .When(x => x.IsEnabled)
                .WithMessage("RequiredProperty is required when configuration is enabled");
        }
    }

}