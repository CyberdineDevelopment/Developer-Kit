using System;
using FluentValidation;
using FluentValidation.Results;
using Shouldly;
using Xunit;
using Xunit.v3;
using FractalDataWorks.Configuration;

namespace FractalDataWorks.Configuration.Tests;

/// <summary>
/// Tests for ConfigurationBase class.
/// </summary>
public class ConfigurationBaseTests
{
    private readonly ITestOutputHelper _output;

    public ConfigurationBaseTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConstructorShouldSetDefaultValues()
    {
        // Arrange & Act
        var config = new TestConfiguration();

        // Assert
        config.Id.ShouldBe(0);
        config.Name.ShouldBe(string.Empty);
        config.IsEnabled.ShouldBeTrue();
        config.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
        config.ModifiedAt.ShouldBeNull();
        config.SectionName.ShouldBe("TestSection");

        _output.WriteLine($"Configuration created: Id={config.Id}, Name='{config.Name}', IsEnabled={config.IsEnabled}, CreatedAt={config.CreatedAt}");
    }

    [Fact]
    public void ValidateShouldReturnSuccessWhenNoValidator()
    {
        // Arrange
        var config = new TestConfiguration();

        // Act
        var result = config.Validate();

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);

        _output.WriteLine($"Validation result: IsValid={result.IsValid}, ErrorCount={result.Errors.Count}");
    }

    [Fact]
    public void ValidateShouldUseCustomValidator()
    {
        // Arrange
        var config = new TestConfigurationWithValidator();

        // Act
        var result = config.Validate();

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].ErrorMessage.ShouldBe("Name is required for validation test");

        _output.WriteLine($"Validation result: IsValid={result.IsValid}, ErrorCount={result.Errors.Count}");
        foreach (var error in result.Errors)
        {
            _output.WriteLine($"Error: {error.ErrorMessage}");
        }
    }

    [Fact]
    public void MarkAsModifiedShouldSetModifiedAt()
    {
        // Arrange
        var config = new TestConfiguration();
        var beforeModification = DateTime.UtcNow;

        // Act
        config.MarkModified();

        // Assert
        config.ModifiedAt.ShouldNotBeNull();
        config.ModifiedAt.Value.ShouldBeGreaterThanOrEqualTo(beforeModification);
        config.ModifiedAt.Value.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);

        _output.WriteLine($"Configuration modified at: {config.ModifiedAt}");
    }

    [Fact]
    public void CloneShouldCreateDeepCopy()
    {
        // Arrange
        var original = new TestConfiguration
        {
            Id = 123,
            Name = "Test Config",
            IsEnabled = false,
            ModifiedAt = DateTime.UtcNow
        };
        // Set CreatedAt using reflection since it has protected setter
        typeof(ConfigurationBase<TestConfiguration>)
            .GetProperty("CreatedAt")!
            .SetValue(original, DateTime.UtcNow.AddDays(-1));

        // Act
        var clone = original.Clone();

        // Assert
        clone.ShouldNotBeSameAs(original);
        clone.Id.ShouldBe(original.Id);
        clone.Name.ShouldBe(original.Name);
        clone.IsEnabled.ShouldBe(original.IsEnabled);
        clone.CreatedAt.ShouldBe(original.CreatedAt);
        clone.ModifiedAt.ShouldBe(original.ModifiedAt);

        _output.WriteLine($"Original: Id={original.Id}, Name='{original.Name}', IsEnabled={original.IsEnabled}");
        _output.WriteLine($"Clone: Id={clone.Id}, Name='{clone.Name}', IsEnabled={clone.IsEnabled}");
    }

    [Fact]
    public void CloneShouldCreateIndependentInstance()
    {
        // Arrange
        var original = new TestConfiguration { Name = "Original" };
        var clone = original.Clone();

        // Act
        clone.Name = "Modified Clone";

        // Assert
        original.Name.ShouldBe("Original");
        clone.Name.ShouldBe("Modified Clone");

        _output.WriteLine($"Original name after clone modification: '{original.Name}'");
        _output.WriteLine($"Clone name after modification: '{clone.Name}'");
    }

    [Theory]
    [InlineData(1, "Test1", true)]
    [InlineData(999, "Test999", false)]
    [InlineData(0, "", true)]
    public void CopyToShouldTransferAllProperties(int id, string name, bool isEnabled)
    {
        // Arrange
        var source = new TestConfiguration
        {
            Id = id,
            Name = name,
            IsEnabled = isEnabled,
            ModifiedAt = DateTime.UtcNow.AddDays(-1)
        };
        // Set CreatedAt using reflection since it has protected setter
        typeof(ConfigurationBase<TestConfiguration>)
            .GetProperty("CreatedAt")!
            .SetValue(source, DateTime.UtcNow.AddDays(-5));
        var target = new TestConfiguration();

        // Act
        source.CopyToTest(target);

        // Assert
        target.Id.ShouldBe(source.Id);
        target.Name.ShouldBe(source.Name);
        target.IsEnabled.ShouldBe(source.IsEnabled);
        target.CreatedAt.ShouldBe(source.CreatedAt);
        target.ModifiedAt.ShouldBe(source.ModifiedAt);

        _output.WriteLine($"Copied properties: Id={target.Id}, Name='{target.Name}', IsEnabled={target.IsEnabled}");
    }

    // Test configuration class for testing purposes
    public class TestConfiguration : ConfigurationBase<TestConfiguration>
    {
        public override string SectionName => "TestSection";

        // Expose protected method for testing
        public void MarkModified() => MarkAsModified();
        public void CopyToTest(TestConfiguration target) => CopyTo(target);
    }

    // Test configuration with validator for testing validation
    public class TestConfigurationWithValidator : ConfigurationBase<TestConfigurationWithValidator>
    {
        public override string SectionName => "TestSectionWithValidator";

        protected override IValidator<TestConfigurationWithValidator>? GetValidator()
        {
            return new TestValidator();
        }

        private class TestValidator : AbstractValidator<TestConfigurationWithValidator>
        {
            public TestValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithMessage("Name is required for validation test");
            }
        }
    }
}