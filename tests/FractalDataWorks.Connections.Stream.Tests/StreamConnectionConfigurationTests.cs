using System.IO;
using System.Threading.Tasks;
using FluentValidation;
using FractalDataWorks.Services;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Connections.Stream.Tests;

/// <summary>
/// Tests for StreamConnectionConfiguration class.
/// </summary>
public class StreamConnectionConfigurationTests
{
    [Fact]
    public void DefaultValuesAreCorrect()
    {
        // Act
        var config = new StreamConnectionConfiguration();

        // Assert
        config.StreamType.ShouldBe(StreamType.File);
        config.Path.ShouldBeNull();
        config.FileMode.ShouldBe(FileMode.OpenOrCreate);
        config.FileAccess.ShouldBe(FileAccess.ReadWrite);
        config.FileShare.ShouldBe(FileShare.Read);
        config.BufferSize.ShouldBe(4096);
        config.UseAsync.ShouldBeTrue();
        config.AutoFlush.ShouldBeFalse();
        config.InitialCapacity.ShouldBe(0);
        config.SectionName.ShouldBe("StreamConnection");
    }

    [Fact]
    public void ValidationSucceedsForValidFileConfiguration()
    {
        // Arrange
        var config = new StreamConnectionConfiguration
        {
            IsEnabled = true,
            StreamType = StreamType.File,
            Path = "/path/to/file.txt",
            BufferSize = 8192
        };

        // Act
        var validator = new StreamConnectionConfigurationValidator();
        var result = validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ValidationSucceedsForValidMemoryConfiguration()
    {
        // Arrange
        var config = new StreamConnectionConfiguration
        {
            IsEnabled = true,
            StreamType = StreamType.Memory,
            InitialCapacity = 1024
        };

        // Act
        var validator = new StreamConnectionConfigurationValidator();
        var result = validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ValidationFailsForFileConfigurationWithoutPath()
    {
        // Arrange
        var config = new StreamConnectionConfiguration
        {
            IsEnabled = true,
            StreamType = StreamType.File,
            Path = null
        };

        // Act
        var validator = new StreamConnectionConfigurationValidator();
        var result = validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("Path is required"));
    }

    [Fact]
    public void ValidationSucceedsForDisabledFileConfigurationWithoutPath()
    {
        // Arrange
        var config = new StreamConnectionConfiguration
        {
            IsEnabled = false,
            StreamType = StreamType.File,
            Path = null
        };

        // Act
        var validator = new StreamConnectionConfigurationValidator();
        var result = validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ValidationFailsForInvalidBufferSize()
    {
        // Arrange
        var config = new StreamConnectionConfiguration
        {
            IsEnabled = true,
            StreamType = StreamType.Memory,
            BufferSize = 0
        };

        // Act
        var validator = new StreamConnectionConfigurationValidator();
        var result = validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("Buffer size must be greater than 0"));
    }

    [Fact]
    public void ValidationFailsForNegativeInitialCapacity()
    {
        // Arrange
        var config = new StreamConnectionConfiguration
        {
            IsEnabled = true,
            StreamType = StreamType.Memory,
            InitialCapacity = -1
        };

        // Act
        var validator = new StreamConnectionConfigurationValidator();
        var result = validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("Initial capacity must be non-negative"));
    }

    [Fact]
    public async Task GetValidatorReturnsCorrectValidator()
    {
        // Arrange
        var config = new StreamConnectionConfiguration();

        // Act
        var result = await config.Validate();

        // Assert
        result.ShouldNotBeNull();
    }
}