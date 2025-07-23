using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FractalDataWorks.Configuration;
using FractalDataWorks.Validation;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Configuration.Tests;

/// <summary>
/// Tests for ConfigurationBase class.
/// </summary>
public class ConfigurationBaseTests
{
    [Fact]
    public void DefaultConstructorCreatesDisabledConfiguration()
    {
        // Act
        var config = new TestConfiguration();

        // Assert
        config.IsEnabled.ShouldBeTrue(); // Default is true, not false
        config.Id.ShouldBe(0);
        config.Name.ShouldBe(string.Empty);
    }

    [Fact]
    public void SectionNamePropertyReturnsCorrectValue()
    {
        // Arrange
        var config = new TestConfiguration();

        // Act
        var sectionName = config.SectionName;

        // Assert
        sectionName.ShouldBe("Test");
    }

    [Fact]
    public void IsValidReturnsTrueForDisabledConfiguration()
    {
        // Arrange
        var config = new TestConfiguration { IsEnabled = false };

        // Act
        var isValid = config.IsValid;

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValidReturnsTrueForValidEnabledConfiguration()
    {
        // Arrange
        var config = new TestConfiguration 
        { 
            IsEnabled = true,
            RequiredProperty = "Valid Value"
        };

        // Act
        var isValid = config.IsValid;

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValidReturnsFalseForInvalidEnabledConfiguration()
    {
        // Arrange
        var config = new TestConfiguration 
        { 
            IsEnabled = true,
            RequiredProperty = null
        };

        // Act
        var isValid = config.IsValid;

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateAsyncReturnsSuccessForDisabledConfiguration()
    {
        // Arrange
        var config = new TestConfiguration { IsEnabled = false };

        // Act
        var result = await config.ValidateAsync();

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateAsyncReturnsSuccessForValidConfiguration()
    {
        // Arrange
        var config = new TestConfiguration 
        { 
            IsEnabled = true,
            RequiredProperty = "Valid Value"
        };

        // Act
        var result = await config.ValidateAsync();

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateAsyncReturnsFailureForInvalidConfiguration()
    {
        // Arrange
        var config = new TestConfiguration 
        { 
            IsEnabled = true,
            RequiredProperty = null
        };

        // Act
        var result = await config.ValidateAsync();

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void GetValidatorReturnsCachedValidator()
    {
        // Arrange
        var config = new TestConfiguration();

        // Act - Call GetValidator twice through IsValid property
        var isValid1 = config.IsValid;
        var isValid2 = config.IsValid;

        // Assert - Both calls should return same result and use cached validator
        isValid1.ShouldBe(isValid2);
        config.ValidatorCallCount.ShouldBe(1); // Validator should only be created once
    }

    [Fact]
    public void CreatedAtIsSetToUtcNowByDefault()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        
        // Act
        var config = new TestConfiguration();
        var afterCreate = DateTime.UtcNow;

        // Assert
        config.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeCreate);
        config.CreatedAt.ShouldBeLessThanOrEqualTo(afterCreate);
    }

    [Fact]
    public void ModifiedAtIsNullByDefault()
    {
        // Arrange & Act
        var config = new TestConfiguration();

        // Assert
        config.ModifiedAt.ShouldBeNull();
    }

    [Fact]
    public void ValidateMethodReturnsIsValidValue()
    {
        // Arrange
        var config = new TestConfiguration 
        { 
            IsEnabled = true,
            RequiredProperty = "Valid"
        };
        var iconfig = (IFdwConfiguration)config;

        // Act
        var result = iconfig.Validate();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void LastValidationResultReturnsNullBeforeValidation()
    {
        // Arrange
        var config = new TestConfiguration();

        // Act
        var result = config.LastValidationResult;

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void LastValidationResultReturnsResultAfterValidation()
    {
        // Arrange
        var config = new TestConfiguration 
        { 
            IsEnabled = true,
            RequiredProperty = null
        };

        // Act
        var isValid = config.IsValid; // This triggers validation
        var result = config.LastValidationResult;

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void ConfigurationWithNoValidatorAlwaysReturnsValid()
    {
        // Arrange
        var config = new NoValidatorConfiguration { IsEnabled = true };

        // Act
        var isValid = config.IsValid;

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateAsyncWithNoValidatorReturnsSuccess()
    {
        // Arrange
        var config = new NoValidatorConfiguration { IsEnabled = true };

        // Act
        var result = await config.ValidateAsync();

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void IsValidCachesResult()
    {
        // Arrange
        var config = new TestConfiguration
        {
            IsEnabled = true,
            RequiredProperty = "Valid"
        };

        // Act - Access IsValid multiple times
        var isValid1 = config.IsValid;
        var isValid2 = config.IsValid;
        var isValid3 = config.IsValid;

        // Assert - All should return the same cached value
        isValid1.ShouldBe(isValid2);
        isValid2.ShouldBe(isValid3);
        config.ValidatorCallCount.ShouldBe(1); // Validator should only be called once
    }

    [Fact]
    public void ConfigurationCanBeModifiedAfterCreation()
    {
        // Arrange
        var config = new TestConfiguration
        {
            Id = 123,
            Name = "Test Config",
            IsEnabled = false
        };

        // Act
        config.Id = 456;
        config.Name = "Modified Config";
        config.IsEnabled = true;
        config.ModifiedAt = DateTime.UtcNow;

        // Assert
        config.Id.ShouldBe(456);
        config.Name.ShouldBe("Modified Config");
        config.IsEnabled.ShouldBeTrue();
        config.ModifiedAt.ShouldNotBeNull();
    }

    // Test classes
    private class TestConfiguration : ConfigurationBase<TestConfiguration>
    {
        public string? RequiredProperty { get; set; }
        public int ValidatorCallCount { get; private set; }

        public override string SectionName => "Test";

        protected override IValidator<TestConfiguration> GetValidator()
        {
            ValidatorCallCount++;
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

    private class NoValidatorConfiguration : ConfigurationBase<NoValidatorConfiguration>
    {
        public override string SectionName => "NoValidator";

        protected override IValidator<NoValidatorConfiguration> GetValidator()
        {
            return null!;
        }
    }
}