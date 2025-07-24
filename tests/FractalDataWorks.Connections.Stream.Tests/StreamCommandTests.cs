using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Connections.Stream.Tests;

/// <summary>
/// Tests for StreamCommand class.
/// </summary>
public class StreamCommandTests
{
    [Fact]
    public void DefaultValuesAreCorrect()
    {
        // Act
        var command = new StreamCommand();

        // Assert
        command.Operation.ShouldBe(StreamOperation.Read);
        command.Data.ShouldBeNull();
        command.BufferSize.ShouldBeNull();
        command.Position.ShouldBeNull();
        command.SeekOrigin.ShouldBeNull();
    }

    [Fact]
    public async Task ValidationSucceedsForValidReadCommand()
    {
        // Arrange
        var command = new StreamCommand
        {
            Operation = StreamOperation.Read,
            BufferSize = 1024
        };

        // Act
        var result = await command.Validate();

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidationSucceedsForValidWriteCommand()
    {
        // Arrange
        var command = new StreamCommand
        {
            Operation = StreamOperation.Write,
            Data = new byte[] { 1, 2, 3 }
        };

        // Act
        var result = await command.Validate();

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidationFailsForWriteCommandWithoutData()
    {
        // Arrange
        var command = new StreamCommand
        {
            Operation = StreamOperation.Write,
            Data = null
        };

        // Act
        var result = await command.Validate();

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("Data is required"));
    }

    [Fact]
    public async Task ValidationSucceedsForValidSeekCommand()
    {
        // Arrange
        var command = new StreamCommand
        {
            Operation = StreamOperation.Seek,
            Position = 100,
            SeekOrigin = SeekOrigin.Begin
        };

        // Act
        var result = await command.Validate();

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidationFailsForSeekCommandWithoutPosition()
    {
        // Arrange
        var command = new StreamCommand
        {
            Operation = StreamOperation.Seek,
            SeekOrigin = SeekOrigin.Begin
        };

        // Act
        var result = await command.Validate();

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("Position is required"));
    }

    [Fact]
    public async Task ValidationFailsForSeekCommandWithoutSeekOrigin()
    {
        // Arrange
        var command = new StreamCommand
        {
            Operation = StreamOperation.Seek,
            Position = 100
        };

        // Act
        var result = await command.Validate();

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        var error = result.Errors.FirstOrDefault();
        error.ShouldNotBeNull();
        error.PropertyName.ShouldBe("SeekOrigin");
    }

    [Fact]
    public async Task ValidationSucceedsForGetInfoCommand()
    {
        // Arrange
        var command = new StreamCommand
        {
            Operation = StreamOperation.GetInfo
        };

        // Act
        var result = await command.Validate();

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidationFailsForInvalidBufferSize()
    {
        // Arrange
        var command = new StreamCommand
        {
            Operation = StreamOperation.Read,
            BufferSize = 0
        };

        // Act
        var result = await command.Validate();

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("Buffer size must be greater than 0"));
    }

    [Fact]
    public void GetValidatorReturnsCorrectValidator()
    {
        // Arrange
        var command = new StreamCommand();

        // Act
        var validatorTask = command.Validate();

        // Assert
        validatorTask.ShouldNotBeNull();
    }
}