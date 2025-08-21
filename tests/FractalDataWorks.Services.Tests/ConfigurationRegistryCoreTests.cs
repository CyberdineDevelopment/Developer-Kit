using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Services.EnhancedEnums;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

public class ConfigurationRegistryCoreTests
{
    private readonly ITestOutputHelper _output;
    private readonly List<TestConfiguration> _testConfigurations;

    public ConfigurationRegistryCoreTests(ITestOutputHelper output)
    {
        _output = output;
        
        _testConfigurations = new List<TestConfiguration>
        {
            new TestConfiguration { Id = 1, Name = "Config1", Value = "Value1" },
            new TestConfiguration { Id = 2, Name = "Config2", Value = "Value2" },
            new TestConfiguration { Id = 3, Name = "Config3", Value = "Value3" }
        };
    }

    // Test Configuration Class
    public class TestConfiguration : IFdwConfiguration
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string SectionName => "TestConfiguration";

        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            if (string.IsNullOrEmpty(Name))
                result.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(Name), "Name cannot be empty"));
            return result;
        }
    }

    [Fact]
    public void ConstructorWithValidConfigurationsShouldInitializeCorrectly()
    {
        // Arrange & Act
        var registry = new ConfigurationRegistryCore<TestConfiguration>(_testConfigurations);

        // Assert
        registry.ShouldNotBeNull();
        registry.GetAll().Count().ShouldBe(3);
        
        _output.WriteLine($"Registry initialized with {registry.GetAll().Count()} configurations");
    }

    [Fact]
    public void ConstructorWithNullConfigurationsShouldCreateEmptyRegistry()
    {
        // Arrange & Act
        var registry = new ConfigurationRegistryCore<TestConfiguration>(null);

        // Assert
        registry.ShouldNotBeNull();
        registry.GetAll().Count().ShouldBe(0);
        
        _output.WriteLine($"Registry initialized with null configurations - count: {registry.GetAll().Count()}");
    }

    [Fact]
    public void ConstructorWithEmptyConfigurationsShouldCreateEmptyRegistry()
    {
        // Arrange
        var emptyConfigurations = new List<TestConfiguration>();

        // Act
        var registry = new ConfigurationRegistryCore<TestConfiguration>(emptyConfigurations);

        // Assert
        registry.ShouldNotBeNull();
        registry.GetAll().Count().ShouldBe(0);
        
        _output.WriteLine($"Registry initialized with empty configurations - count: {registry.GetAll().Count()}");
    }

    [Theory]
    [InlineData(1, true, "Config1")]
    [InlineData(2, true, "Config2")]
    [InlineData(3, true, "Config3")]
    [InlineData(999, false, null)]
    [InlineData(-1, false, null)]
    public void GetByIdShouldReturnExpectedResult(int id, bool shouldExist, string? expectedName)
    {
        // Arrange
        var registry = new ConfigurationRegistryCore<TestConfiguration>(_testConfigurations);

        // Act
        var result = registry.Get(id);

        // Assert
        if (shouldExist)
        {
            result.ShouldNotBeNull();
            result.Id.ShouldBe(id);
            result.Name.ShouldBe(expectedName);
            _output.WriteLine($"Found configuration by ID {id}: {result.Name}");
        }
        else
        {
            result.ShouldBeNull();
            _output.WriteLine($"Configuration not found for ID: {id}");
        }
    }

    [Fact]
    public void GetAllShouldReturnAllConfigurations()
    {
        // Arrange
        var registry = new ConfigurationRegistryCore<TestConfiguration>(_testConfigurations);

        // Act
        var allConfigurations = registry.GetAll().ToList();

        // Assert
        allConfigurations.ShouldNotBeNull();
        allConfigurations.Count.ShouldBe(3);
        allConfigurations.ShouldContain(c => c.Id == 1 && c.Name == "Config1");
        allConfigurations.ShouldContain(c => c.Id == 2 && c.Name == "Config2");
        allConfigurations.ShouldContain(c => c.Id == 3 && c.Name == "Config3");
        
        _output.WriteLine($"GetAll returned {allConfigurations.Count} configurations:");
        foreach (var config in allConfigurations)
        {
            _output.WriteLine($"  - ID: {config.Id}, Name: {config.Name}, Value: {config.Value}");
        }
    }

    [Fact]
    public void GetAllFromEmptyRegistryShouldReturnEmptyCollection()
    {
        // Arrange
        var emptyRegistry = new ConfigurationRegistryCore<TestConfiguration>(new List<TestConfiguration>());

        // Act
        var allConfigurations = emptyRegistry.GetAll().ToList();

        // Assert
        allConfigurations.ShouldNotBeNull();
        allConfigurations.Count.ShouldBe(0);
        
        _output.WriteLine($"Empty registry GetAll returned {allConfigurations.Count} configurations");
    }

    [Theory]
    [InlineData(1, true, "Config1")]
    [InlineData(2, true, "Config2")]
    [InlineData(3, true, "Config3")]
    [InlineData(999, false, null)]
    [InlineData(-1, false, null)]
    public void TryGetByIdShouldReturnExpectedResult(int id, bool shouldExist, string? expectedName)
    {
        // Arrange
        var registry = new ConfigurationRegistryCore<TestConfiguration>(_testConfigurations);

        // Act
        var found = registry.TryGet(id, out var configuration);

        // Assert
        found.ShouldBe(shouldExist);
        
        if (shouldExist)
        {
            configuration.ShouldNotBeNull();
            configuration!.Id.ShouldBe(id);
            configuration.Name.ShouldBe(expectedName);
            _output.WriteLine($"TryGet found configuration by ID {id}: {configuration.Name}");
        }
        else
        {
            configuration.ShouldBeNull();
            _output.WriteLine($"TryGet did not find configuration for ID: {id}");
        }
    }

    [Fact]
    public void TryGetFromEmptyRegistryShouldReturnFalse()
    {
        // Arrange
        var emptyRegistry = new ConfigurationRegistryCore<TestConfiguration>(new List<TestConfiguration>());

        // Act
        var found = emptyRegistry.TryGet(1, out var configuration);

        // Assert
        found.ShouldBeFalse();
        configuration.ShouldBeNull();
        
        _output.WriteLine($"TryGet from empty registry returned: {found}");
    }

    [Fact]
    public void GetAllShouldReturnImmutableView()
    {
        // Arrange
        var originalConfigurations = new List<TestConfiguration>(_testConfigurations);
        var registry = new ConfigurationRegistryCore<TestConfiguration>(originalConfigurations);

        // Act - Get all configurations and modify the original list
        var retrievedConfigurations = registry.GetAll().ToList();
        originalConfigurations.Clear(); // This should not affect the registry

        // Assert - Registry should still have the original configurations
        var afterClearConfigurations = registry.GetAll().ToList();
        afterClearConfigurations.Count.ShouldBe(3);
        afterClearConfigurations.ShouldNotBeSameAs(originalConfigurations);
        
        _output.WriteLine($"After clearing original list, registry still has {afterClearConfigurations.Count} configurations");
    }

    [Fact]
    public void GetAllMultipleCallsShouldReturnConsistentResults()
    {
        // Arrange
        var registry = new ConfigurationRegistryCore<TestConfiguration>(_testConfigurations);

        // Act
        var firstCall = registry.GetAll().ToList();
        var secondCall = registry.GetAll().ToList();
        var thirdCall = registry.GetAll().ToList();

        // Assert
        firstCall.Count.ShouldBe(secondCall.Count);
        secondCall.Count.ShouldBe(thirdCall.Count);
        
        // Verify content is the same across calls
        for (int i = 0; i < firstCall.Count; i++)
        {
            firstCall[i].Id.ShouldBe(secondCall[i].Id);
            firstCall[i].Id.ShouldBe(thirdCall[i].Id);
            firstCall[i].Name.ShouldBe(secondCall[i].Name);
            firstCall[i].Name.ShouldBe(thirdCall[i].Name);
        }
        
        _output.WriteLine($"Multiple GetAll calls returned consistent results with {firstCall.Count} configurations each");
    }

    // Test with configuration type that doesn't have an Id property
    public class ConfigurationWithoutId : IFdwConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public string SectionName => "ConfigurationWithoutId";

        public ValidationResult Validate()
        {
            return new ValidationResult();
        }
    }

    [Fact]
    public void GetWithConfigurationWithoutIdPropertyShouldReturnNull()
    {
        // Arrange
        var configurationsWithoutId = new List<ConfigurationWithoutId>
        {
            new ConfigurationWithoutId { Name = "Config1" },
            new ConfigurationWithoutId { Name = "Config2" }
        };
        var registry = new ConfigurationRegistryCore<ConfigurationWithoutId>(configurationsWithoutId);

        // Act
        var result = registry.Get(1);

        // Assert
        result.ShouldBeNull();
        
        _output.WriteLine($"Get by ID with configuration without Id property returned: {result?.Name ?? "null"}");
    }

    [Fact]
    public void TryGetWithConfigurationWithoutIdPropertyShouldReturnFalse()
    {
        // Arrange
        var configurationsWithoutId = new List<ConfigurationWithoutId>
        {
            new ConfigurationWithoutId { Name = "Config1" },
            new ConfigurationWithoutId { Name = "Config2" }
        };
        var registry = new ConfigurationRegistryCore<ConfigurationWithoutId>(configurationsWithoutId);

        // Act
        var found = registry.TryGet(1, out var configuration);

        // Assert
        found.ShouldBeFalse();
        configuration.ShouldBeNull();
        
        _output.WriteLine($"TryGet with configuration without Id property returned: {found}");
    }

    // Test with configuration that has different Id property type
    public class ConfigurationWithStringId : IFdwConfiguration
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SectionName => "ConfigurationWithStringId";

        public ValidationResult Validate()
        {
            return new ValidationResult();
        }
    }

    [Fact]
    public void GetWithStringIdPropertyShouldReturnNull()
    {
        // Arrange
        var configurationsWithStringId = new List<ConfigurationWithStringId>
        {
            new ConfigurationWithStringId { Id = "1", Name = "Config1" },
            new ConfigurationWithStringId { Id = "2", Name = "Config2" }
        };
        var registry = new ConfigurationRegistryCore<ConfigurationWithStringId>(configurationsWithStringId);

        // Act
        var result = registry.Get(1);

        // Assert
        result.ShouldBeNull();
        
        _output.WriteLine($"Get by ID with string Id property returned: {result?.Name ?? "null"}");
    }

    // Test with configuration that has dynamic properties
    public class DynamicConfiguration : IFdwConfiguration
    {
        private readonly Dictionary<string, object> _properties = new(StringComparer.Ordinal);

        public object this[string key]
        {
            get => _properties.ContainsKey(key) ? _properties[key] : null!;
            set => _properties[key] = value;
        }

        public string SectionName => "DynamicConfiguration";

        // Use dynamic behavior via IDictionary for the registry's dynamic access
        public ValidationResult Validate()
        {
            return new ValidationResult();
        }

        // Make this work with dynamic access pattern used in ConfigurationRegistryCore
        public static implicit operator DynamicConfiguration(Dictionary<string, object> dict)
        {
            var config = new DynamicConfiguration();
            foreach (var kvp in dict)
            {
                config[kvp.Key] = kvp.Value;
            }
            return config;
        }
    }

    // Concrete test configuration that implements dynamic behavior similar to what the registry expects
    public class TestConfigurationWithDynamicId : IFdwConfiguration
    {
        public dynamic Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string SectionName => "TestConfigurationWithDynamicId";

        public ValidationResult Validate()
        {
            return new ValidationResult();
        }
    }

    [Fact]
    public void GetWithDynamicIdShouldWorkCorrectly()
    {
        // Arrange
        var dynamicConfigurations = new List<TestConfigurationWithDynamicId>
        {
            new TestConfigurationWithDynamicId { Id = 1, Name = "Config1" },
            new TestConfigurationWithDynamicId { Id = 2, Name = "Config2" }
        };
        var registry = new ConfigurationRegistryCore<TestConfigurationWithDynamicId>(dynamicConfigurations);

        // Act
        var result = registry.Get(1);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Config1");
        ((int)result.Id).ShouldBe(1);
        
        _output.WriteLine($"Get with dynamic Id property returned: {result.Name} (ID: {result.Id})");
    }

    [Fact]
    public void RegistryWithDuplicateIdsShouldReturnFirstMatch()
    {
        // Arrange
        var configurationsWithDuplicates = new List<TestConfiguration>
        {
            new TestConfiguration { Id = 1, Name = "Config1_First", Value = "Value1" },
            new TestConfiguration { Id = 2, Name = "Config2", Value = "Value2" },
            new TestConfiguration { Id = 1, Name = "Config1_Second", Value = "Value1_Duplicate" }
        };
        var registry = new ConfigurationRegistryCore<TestConfiguration>(configurationsWithDuplicates);

        // Act
        var result = registry.Get(1);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Config1_First"); // Should return the first match
        result.Value.ShouldBe("Value1");
        
        _output.WriteLine($"Registry with duplicates returned first match: {result.Name}");
    }
}