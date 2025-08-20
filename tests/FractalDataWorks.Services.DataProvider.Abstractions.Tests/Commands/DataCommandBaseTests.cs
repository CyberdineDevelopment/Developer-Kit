using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using Shouldly;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;
using FractalDataWorks;
using System.Diagnostics.CodeAnalysis;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Tests.Commands;

public class DataCommandBaseTests
{
    private readonly ITestOutputHelper _output;

    public DataCommandBaseTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConstructorShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var commandType = "TestCommand";
        var connectionName = "TestConnection";
        var targetContainer = new DataPath(["test", "container"]);
        var expectedResultType = typeof(string);
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal) { { "param1", "value1" } };
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal) { { "meta1", "metaValue1" } };
        var timeout = TimeSpan.FromSeconds(30);

        // Act
        var command = new TestDataCommand(commandType, connectionName, targetContainer, expectedResultType, parameters, metadata, timeout);

        // Assert
        command.CommandType.ShouldBe(commandType);
        command.ConnectionName.ShouldBe(connectionName);
        command.TargetContainer.ShouldBe(targetContainer);
        command.Target.ShouldBe(targetContainer.ToString());
        command.ExpectedResultType.ShouldBe(expectedResultType);
        command.Parameters.ShouldBe(parameters);
        command.Metadata.ShouldBe(metadata);
        command.Timeout.ShouldBe(timeout);
        command.CommandId.ShouldNotBe(Guid.Empty);
        command.CorrelationId.ShouldNotBe(Guid.Empty);
        command.Timestamp.ShouldBeInRange(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow);
        command.Configuration.ShouldBeNull();

        _output.WriteLine($"Command: {command}");
    }

    [Fact]
    public void ConstructorWithNullParametersShouldInitializeEmptyCollections()
    {
        // Arrange
        var commandType = "TestCommand";
        var connectionName = "TestConnection";
        var expectedResultType = typeof(string);

        // Act
        var command = new TestDataCommand(commandType, connectionName, null, expectedResultType);

        // Assert
        command.Parameters.ShouldNotBeNull();
        command.Parameters.Count.ShouldBe(0);
        command.Metadata.ShouldNotBeNull();
        command.Metadata.Count.ShouldBe(0);
        command.TargetContainer.ShouldBeNull();
        command.Target.ShouldBeNull();
        command.Timeout.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ConstructorShouldThrowWhenCommandTypeIsNullOrWhitespace(string? commandType)
    {
        // Arrange
        var connectionName = "TestConnection";
        var expectedResultType = typeof(string);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => 
            new TestDataCommand(commandType!, connectionName, null, expectedResultType));
        exception.ParamName.ShouldBe("commandType");
        exception.Message.ShouldContain("Command type cannot be null or empty");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ConstructorShouldThrowWhenConnectionNameIsNullOrWhitespace(string? connectionName)
    {
        // Arrange
        var commandType = "TestCommand";
        var expectedResultType = typeof(string);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => 
            new TestDataCommand(commandType, connectionName!, null, expectedResultType));
        exception.ParamName.ShouldBe("connectionName");
        exception.Message.ShouldContain("Connection name cannot be null or empty");
    }

    [Fact]
    public void ConstructorShouldThrowWhenExpectedResultTypeIsNull()
    {
        // Arrange
        var commandType = "TestCommand";
        var connectionName = "TestConnection";

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => 
            new TestDataCommand(commandType, connectionName, null, null!));
        exception.ParamName.ShouldBe("expectedResultType");
    }

    [Fact]
    public void WithConnectionShouldCreateNewInstanceWithUpdatedConnectionName()
    {
        // Arrange
        var originalCommand = new TestDataCommand("TestCommand", "OriginalConnection", null, typeof(string));
        var newConnectionName = "NewConnection";

        // Act
        var newCommand = originalCommand.WithConnection(newConnectionName);

        // Assert
        newCommand.ShouldNotBeSameAs(originalCommand);
        newCommand.ConnectionName.ShouldBe(newConnectionName);
        newCommand.CommandType.ShouldBe(originalCommand.CommandType);
        newCommand.ExpectedResultType.ShouldBe(originalCommand.ExpectedResultType);
        newCommand.TargetContainer.ShouldBe(originalCommand.TargetContainer);
        newCommand.Parameters.ShouldBe(originalCommand.Parameters);
        newCommand.Metadata.ShouldBe(originalCommand.Metadata);
        newCommand.Timeout.ShouldBe(originalCommand.Timeout);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithConnectionShouldThrowWhenConnectionNameIsNullOrWhitespace(string? connectionName)
    {
        // Arrange
        var command = new TestDataCommand("TestCommand", "OriginalConnection", null, typeof(string));

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => command.WithConnection(connectionName!));
        exception.ParamName.ShouldBe("connectionName");
        exception.Message.ShouldContain("Connection name cannot be null or empty");
    }

    [Fact]
    public void WithTargetShouldCreateNewInstanceWithUpdatedTargetContainer()
    {
        // Arrange
        var originalCommand = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string));
        var newTarget = new DataPath(["new", "target"]);

        // Act
        var newCommand = originalCommand.WithTarget(newTarget);

        // Assert
        newCommand.ShouldNotBeSameAs(originalCommand);
        newCommand.TargetContainer.ShouldBe(newTarget);
        newCommand.Target.ShouldBe(newTarget.ToString());
        newCommand.ConnectionName.ShouldBe(originalCommand.ConnectionName);
        newCommand.CommandType.ShouldBe(originalCommand.CommandType);
    }

    [Fact]
    public void WithTargetShouldThrowWhenTargetContainerIsNull()
    {
        // Arrange
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string));

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => command.WithTarget(null!));
        exception.ParamName.ShouldBe("targetContainer");
    }

    [Fact]
    public void WithTimeoutShouldCreateNewInstanceWithUpdatedTimeout()
    {
        // Arrange
        var originalCommand = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string));
        var newTimeout = TimeSpan.FromMinutes(5);

        // Act
        var newCommand = originalCommand.WithTimeout(newTimeout);

        // Assert
        newCommand.ShouldNotBeSameAs(originalCommand);
        newCommand.Timeout.ShouldBe(newTimeout);
        newCommand.ConnectionName.ShouldBe(originalCommand.ConnectionName);
        newCommand.CommandType.ShouldBe(originalCommand.CommandType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-30)]
    public void WithTimeoutShouldThrowWhenTimeoutIsNotPositive(int seconds)
    {
        // Arrange
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string));
        var timeout = TimeSpan.FromSeconds(seconds);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => command.WithTimeout(timeout));
        exception.ParamName.ShouldBe("timeout");
        exception.Message.ShouldContain("Timeout must be positive");
    }

    [Fact]
    public void ValidateShouldReturnValidResultForValidCommand()
    {
        // Arrange
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string));

        // Act
        var result = command.Validate();

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
    }

    [Fact]
    public void ValidateShouldReturnErrorsForInvalidCommand()
    {
        // Arrange
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string), 
            timeout: TimeSpan.FromSeconds(-1));

        // Act
        var result = command.Validate();

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].PropertyName.ShouldBe("Timeout");
        result.Errors[0].ErrorMessage.ShouldBe("Timeout must be positive if specified.");

        _output.WriteLine($"Validation errors: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");
    }

    [Fact]
    public void ValidateShouldReturnErrorForNullParameterKeys()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal) { { "", "value" } };
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string), parameters);

        // Act
        var result = command.Validate();

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Parameters" && e.ErrorMessage.Contains("Parameter keys cannot be null or empty"));
    }

    [Fact]
    public void WithParametersShouldCreateNewInstanceWithUpdatedParameters()
    {
        // Arrange
        var originalCommand = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string));
        var newParameters = new Dictionary<string, object?>(StringComparer.Ordinal) { { "newParam", "newValue" } };

        // Act
        var newCommand = originalCommand.WithParameters(newParameters);

        // Assert
        newCommand.ShouldNotBeSameAs(originalCommand);
        newCommand.Parameters.ShouldBe(newParameters);
        (newCommand as TestDataCommand)?.ConnectionName.ShouldBe(originalCommand.ConnectionName);
    }

    [Fact]
    public void WithParametersShouldThrowWhenParametersIsNull()
    {
        // Arrange
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string));

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => command.WithParameters(null!));
        exception.ParamName.ShouldBe("newParameters");
    }

    [Fact]
    public void WithMetadataShouldCreateNewInstanceWithUpdatedMetadata()
    {
        // Arrange
        var originalCommand = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string));
        var newMetadata = new Dictionary<string, object>(StringComparer.Ordinal) { { "newMeta", "newMetaValue" } };

        // Act
        var newCommand = originalCommand.WithMetadata(newMetadata);

        // Assert
        newCommand.ShouldNotBeSameAs(originalCommand);
        newCommand.Metadata.ShouldBe(newMetadata);
        (newCommand as TestDataCommand)?.ConnectionName.ShouldBe(originalCommand.ConnectionName);
    }

    [Fact]
    public void WithMetadataShouldThrowWhenMetadataIsNull()
    {
        // Arrange
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string));

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => command.WithMetadata(null!));
        exception.ParamName.ShouldBe("newMetadata");
    }

    [Fact]
    public void GetParameterShouldReturnCorrectValueWhenParameterExists()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal) 
        { 
            { "stringParam", "testValue" },
            { "intParam", 42 },
            { "nullParam", null }
        };
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string), parameters);

        // Act & Assert
        command.GetTestParameter<string>("stringParam").ShouldBe("testValue");
        command.GetTestParameter<int>("intParam").ShouldBe(42);
        command.GetTestParameter<string>("nullParam").ShouldBeNull();
    }

    [Fact]
    public void GetParameterShouldThrowWhenParameterNotFound()
    {
        // Arrange
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string));

        // Act & Assert
        var exception = Should.Throw<KeyNotFoundException>(() => command.GetTestParameter<string>("nonExistent"));
        exception.Message.ShouldContain("Parameter 'nonExistent' not found");
    }

    [Fact]
    public void GetParameterShouldConvertValueWhenPossible()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal) { { "numberAsString", "42" } };
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string), parameters);

        // Act
        var result = command.GetTestParameter<int>("numberAsString");

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public void GetParameterShouldThrowWhenConversionFails()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal) { { "invalidNumber", "notANumber" } };
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string), parameters);

        // Act & Assert
        var exception = Should.Throw<InvalidCastException>(() => command.GetTestParameter<int>("invalidNumber"));
        exception.Message.ShouldContain("Cannot convert parameter 'invalidNumber' value from String to Int32");
    }

    [Fact]
    public void TryGetParameterShouldReturnTrueWhenParameterExistsAndConvertible()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal) { { "validParam", "42" } };
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string), parameters);

        // Act
        var success = command.TryGetTestParameter<int>("validParam", out var value);

        // Assert
        success.ShouldBeTrue();
        value.ShouldBe(42);
    }

    [Fact]
    public void TryGetParameterShouldReturnFalseWhenParameterNotFound()
    {
        // Arrange
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string));

        // Act
        var success = command.TryGetTestParameter<string>("nonExistent", out var value);

        // Assert
        success.ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void GetMetadataShouldReturnCorrectValueWhenMetadataExists()
    {
        // Arrange
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal) 
        { 
            { "stringMeta", "testMetaValue" },
            { "intMeta", 100 }
        };
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string), null, metadata);

        // Act & Assert
        command.GetTestMetadata<string>("stringMeta").ShouldBe("testMetaValue");
        command.GetTestMetadata<int>("intMeta").ShouldBe(100);
    }

    [Fact]
    public void GetMetadataShouldThrowWhenMetadataNotFound()
    {
        // Arrange
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string));

        // Act & Assert
        var exception = Should.Throw<KeyNotFoundException>(() => command.GetTestMetadata<string>("nonExistent"));
        exception.Message.ShouldContain("Metadata 'nonExistent' not found");
    }

    [Fact]
    public void TryGetMetadataShouldReturnTrueWhenMetadataExistsAndConvertible()
    {
        // Arrange
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal) { { "validMeta", "123" } };
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string), null, metadata);

        // Act
        var success = command.TryGetTestMetadata<int>("validMeta", out var value);

        // Assert
        success.ShouldBeTrue();
        value.ShouldBe(123);
    }

    [Fact]
    public void TryGetMetadataShouldReturnFalseWhenMetadataNotFound()
    {
        // Arrange
        var command = new TestDataCommand("TestCommand", "TestConnection", null, typeof(string));

        // Act
        var success = command.TryGetTestMetadata<string>("nonExistent", out var value);

        // Assert
        success.ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void ToStringShouldReturnCorrectFormatWithTarget()
    {
        // Arrange
        var targetContainer = new DataPath(["test", "container"]);
        var command = new TestDataCommand("Query", "TestDB", targetContainer, typeof(string));

        // Act
        var result = command.ToString();

        // Assert
        result.ShouldBe($"Query(TestDB) -> {targetContainer}");
    }

    [Fact]
    public void ToStringShouldReturnCorrectFormatWithoutTarget()
    {
        // Arrange
        var command = new TestDataCommand("Insert", "TestDB", null, typeof(string));

        // Act
        var result = command.ToString();

        // Assert
        result.ShouldBe("Insert(TestDB)");
    }

    // Test class that exposes protected members for testing
    private sealed class TestDataCommand : DataCommandBase
    {
        public TestDataCommand(
            string commandType,
            string connectionName,
            DataPath? targetContainer,
            Type expectedResultType,
            IReadOnlyDictionary<string, object?>? parameters = null,
            IReadOnlyDictionary<string, object>? metadata = null,
            TimeSpan? timeout = null)
            : base(commandType, connectionName, targetContainer, expectedResultType, parameters, metadata, timeout)
        {
        }

        public override bool IsDataModifying => false;

        protected override DataCommandBase CreateCopy(
            string connectionName,
            DataPath? targetContainer,
            IReadOnlyDictionary<string, object?> parameters,
            IReadOnlyDictionary<string, object> metadata,
            TimeSpan? timeout)
        {
            return new TestDataCommand(CommandType, connectionName, targetContainer, ExpectedResultType, parameters, metadata, timeout);
        }

        // Expose protected methods for testing
        public T? GetTestParameter<T>(string name) => GetParameter<T>(name);
        public bool TryGetTestParameter<T>(string name, out T? value) => TryGetParameter<T>(name, out value);
        public T GetTestMetadata<T>(string name) => GetMetadata<T>(name);
        public bool TryGetTestMetadata<T>(string name, out T? value) => TryGetMetadata<T>(name, out value);
    }
}