using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FractalDataWorks.Configuration.Sources;
using FractalDataWorks.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Configuration.Tests;

/// <summary>
/// Tests for JsonConfigurationSource class.
/// </summary>
public class JsonConfigurationSourceTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ILogger<JsonConfigurationSource>> _mockLogger;
    private readonly string _tempPath;
    private readonly JsonConfigurationSource _source;

    public JsonConfigurationSourceTests(ITestOutputHelper output)
    {
        _output = output;
        _mockLogger = new Mock<ILogger<JsonConfigurationSource>>();
        _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _source = new JsonConfigurationSource(_mockLogger.Object, _tempPath);
    }

    [Fact]
    public void ConstructorShouldSetProperties()
    {
        // Assert
        _source.Name.ShouldBe("JSON");
        _source.IsWritable.ShouldBeTrue();
        _source.SupportsReload.ShouldBeFalse();

        _output.WriteLine($"Source properties: Name='{_source.Name}', IsWritable={_source.IsWritable}, SupportsReload={_source.SupportsReload}");
    }

    [Fact]
    public void ConstructorShouldCreateBasePathDirectory()
    {
        // Assert
        Directory.Exists(_tempPath).ShouldBeTrue();

        _output.WriteLine($"Base path created: {_tempPath}");
    }

    [Fact]
    public void ConstructorShouldThrowForNullBasePath()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            new JsonConfigurationSource(_mockLogger.Object, null!));

        _output.WriteLine("Constructor correctly throws for null base path");
    }

    [Fact]
    public async Task LoadShouldReturnEmptyCollectionWhenNoFiles()
    {
        // Act
        var result = await _source.Load<TestConfiguration>();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(0);

        _output.WriteLine("Load returned empty collection when no files exist");
    }

    [Fact]
    public async Task LoadShouldReturnConfigurationsFromJsonFiles()
    {
        // Arrange
        var config1 = new TestConfiguration { Id = 1, Name = "Config 1" };
        var config2 = new TestConfiguration { Id = 2, Name = "Config 2" };
        
        await CreateJsonFile(config1);
        await CreateJsonFile(config2);

        // Act
        var result = await _source.Load<TestConfiguration>();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(2);

        var configs = result.Value.ToList();
        configs.ShouldContain(c => c.Id == 1 && c.Name == "Config 1");
        configs.ShouldContain(c => c.Id == 2 && c.Name == "Config 2");

        _output.WriteLine($"Loaded {configs.Count} configurations from JSON files");
        foreach (var config in configs)
        {
            _output.WriteLine($"  - {config.Name} (ID: {config.Id})");
        }
    }

    [Fact]
    public async Task LoadShouldIgnoreInvalidJsonFiles()
    {
        // Arrange
        var validConfig = new TestConfiguration { Id = 1, Name = "Valid Config" };
        await CreateJsonFile(validConfig);
        
        // Create invalid JSON file
        var invalidFile = Path.Combine(_tempPath, "TestConfiguration_999.json");
        await File.WriteAllTextAsync(invalidFile, "{ invalid json }");

        // Act
        var result = await _source.Load<TestConfiguration>();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(1);
        result.Value.First().Name.ShouldBe("Valid Config");

        _output.WriteLine("Load correctly ignored invalid JSON file and loaded valid configuration");
    }

    [Fact]
    public async Task LoadByIdShouldReturnConfigurationWhenFileExists()
    {
        // Arrange
        var config = new TestConfiguration { Id = 42, Name = "Test Config" };
        await CreateJsonFile(config);

        // Act
        var result = await _source.Load<TestConfiguration>(42);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(42);
        result.Value.Name.ShouldBe("Test Config");

        _output.WriteLine($"Loaded configuration by ID: {result.Value.Name} (ID: {result.Value.Id})");
    }

    [Fact]
    public async Task LoadByIdShouldReturnFailureWhenFileNotExists()
    {
        // Act
        var result = await _source.Load<TestConfiguration>(999);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldContain("Configuration file not found");

        _output.WriteLine($"Expected file not found error: {result.Message}");
    }

    [Fact]
    public async Task SaveShouldCreateJsonFile()
    {
        // Arrange
        var config = new TestConfiguration { Id = 123, Name = "Save Test Config" };

        // Act
        var result = await _source.Save(config);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(config);

        // Verify file was created
        var expectedFile = Path.Combine(_tempPath, "TestConfiguration_123.json");
        File.Exists(expectedFile).ShouldBeTrue();

        // Verify file content
        var json = await File.ReadAllTextAsync(expectedFile);
        var deserializedConfig = JsonSerializer.Deserialize<TestConfiguration>(json, GetJsonOptions());
        deserializedConfig.ShouldNotBeNull();
        deserializedConfig.Id.ShouldBe(123);
        deserializedConfig.Name.ShouldBe("Save Test Config");

        _output.WriteLine($"Successfully saved configuration to: {expectedFile}");
    }

    [Fact]
    public async Task SaveShouldOverwriteExistingFile()
    {
        // Arrange
        var originalConfig = new TestConfiguration { Id = 456, Name = "Original" };
        var updatedConfig = new TestConfiguration { Id = 456, Name = "Updated" };
        
        await CreateJsonFile(originalConfig);

        // Act
        var result = await _source.Save(updatedConfig);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        // Verify file was updated
        var configFile = Path.Combine(_tempPath, "TestConfiguration_456.json");
        var json = await File.ReadAllTextAsync(configFile);
        var deserializedConfig = JsonSerializer.Deserialize<TestConfiguration>(json, GetJsonOptions());
        deserializedConfig.ShouldNotBeNull();
        deserializedConfig.Name.ShouldBe("Updated");

        _output.WriteLine($"Successfully updated existing configuration file");
    }

    [Fact]
    public async Task DeleteShouldRemoveJsonFile()
    {
        // Arrange
        var config = new TestConfiguration { Id = 789, Name = "To Delete" };
        await CreateJsonFile(config);
        
        var configFile = Path.Combine(_tempPath, "TestConfiguration_789.json");
        File.Exists(configFile).ShouldBeTrue();

        // Act
        var result = await _source.Delete<TestConfiguration>(789);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        File.Exists(configFile).ShouldBeFalse();

        _output.WriteLine("Successfully deleted configuration file");
    }

    [Fact]
    public async Task DeleteShouldReturnFailureWhenFileNotExists()
    {
        // Act
        var result = await _source.Delete<TestConfiguration>(999);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldContain("Configuration file not found");

        _output.WriteLine($"Expected file not found error on delete: {result.Message}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999)]
    public void GetFileNameShouldReturnCorrectFormat(int id)
    {
        // Arrange
        var expectedFileName = $"TestConfiguration_{id}.json";

        // Act - Use reflection to test private method
        var method = typeof(JsonConfigurationSource).GetMethod("GetFileName", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            new[] { typeof(int) });
        method.ShouldNotBeNull();
        
        var genericMethod = method.MakeGenericMethod(typeof(TestConfiguration));
        var fileName = (string)genericMethod.Invoke(null, new object[] { id })!;

        // Assert
        fileName.ShouldBe(expectedFileName);

        _output.WriteLine($"Generated file name for ID {id}: {fileName}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, recursive: true);
        }
    }

    private async Task CreateJsonFile(TestConfiguration config)
    {
        var fileName = $"TestConfiguration_{config.Id}.json";
        var filePath = Path.Combine(_tempPath, fileName);
        var json = JsonSerializer.Serialize(config, GetJsonOptions());
        await File.WriteAllTextAsync(filePath, json);
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // Test configuration class
    public class TestConfiguration : ConfigurationBase<TestConfiguration>
    {
        public override string SectionName => "TestSection";
    }
}