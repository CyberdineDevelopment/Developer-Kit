using System;
using System.Collections.Generic;
using Xunit;
using Shouldly;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;
using FractalDataWorks.Services.DataProvider.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Tests.Commands;

public class DataCommandBaseGenericTests
{
    private readonly ITestOutputHelper _output;

    public DataCommandBaseGenericTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConstructorShouldInitializeWithCorrectGenericType()
    {
        // Arrange & Act
        var command = new TestGenericDataCommand<string>("TestCommand", "TestConnection");

        // Assert
        command.ExpectedResultType.ShouldBe(typeof(string));
        command.CommandType.ShouldBe("TestCommand");
        command.ConnectionName.ShouldBe("TestConnection");
    }

    [Fact]
    public void ConstructorShouldInitializeWithComplexGenericType()
    {
        // Arrange & Act
        var command = new TestGenericDataCommand<List<int>>("TestCommand", "TestConnection");

        // Assert
        command.ExpectedResultType.ShouldBe(typeof(List<int>));
    }

    [Fact]
    public void WithParametersShouldReturnTypedInterface()
    {
        // Arrange
        var command = new TestGenericDataCommand<string>("TestCommand", "TestConnection");
        var newParameters = new Dictionary<string, object?>(StringComparer.Ordinal) { { "param1", "value1" } };

        // Act
        var result = ((IDataCommand<string>)command).WithParameters(newParameters);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestGenericDataCommand<string>>();
        result.Parameters.ShouldBe(newParameters);
        result.ExpectedResultType.ShouldBe(typeof(string));
    }

    [Fact]
    public void WithParametersShouldThrowWhenParametersNull()
    {
        // Arrange
        var command = new TestGenericDataCommand<string>("TestCommand", "TestConnection");

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => 
            ((IDataCommand<string>)command).WithParameters(null!));
        exception.ParamName.ShouldBe("newParameters");
    }

    [Fact]
    public void WithMetadataShouldReturnTypedInterface()
    {
        // Arrange
        var command = new TestGenericDataCommand<string>("TestCommand", "TestConnection");
        var newMetadata = new Dictionary<string, object>(StringComparer.Ordinal) { { "meta1", "metaValue1" } };

        // Act
        var result = ((IDataCommand<string>)command).WithMetadata(newMetadata);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestGenericDataCommand<string>>();
        result.Metadata.ShouldBe(newMetadata);
        result.ExpectedResultType.ShouldBe(typeof(string));
    }

    [Fact]
    public void WithMetadataShouldThrowWhenMetadataNull()
    {
        // Arrange
        var command = new TestGenericDataCommand<string>("TestCommand", "TestConnection");

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => 
            ((IDataCommand<string>)command).WithMetadata(null!));
        exception.ParamName.ShouldBe("newMetadata");
    }

    [Fact]
    public void TypedWithConnectionShouldReturnCorrectType()
    {
        // Arrange
        var command = new TestGenericDataCommand<int>("TestCommand", "OriginalConnection");
        var newConnectionName = "NewConnection";

        // Act
        var result = command.WithConnection(newConnectionName);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestGenericDataCommand<int>>();
        result.ConnectionName.ShouldBe(newConnectionName);
        result.ExpectedResultType.ShouldBe(typeof(int));
    }

    [Fact]
    public void TypedWithTargetShouldReturnCorrectType()
    {
        // Arrange
        var command = new TestGenericDataCommand<bool>("TestCommand", "TestConnection");
        var newTarget = new DataPath(["new", "target"]);

        // Act
        var result = command.WithTarget(newTarget);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestGenericDataCommand<bool>>();
        result.TargetContainer.ShouldBe(newTarget);
        result.ExpectedResultType.ShouldBe(typeof(bool));
    }

    [Fact]
    public void TypedWithTimeoutShouldReturnCorrectType()
    {
        // Arrange
        var command = new TestGenericDataCommand<DateTime>("TestCommand", "TestConnection");
        var newTimeout = TimeSpan.FromMinutes(10);

        // Act
        var result = command.WithTimeout(newTimeout);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestGenericDataCommand<DateTime>>();
        result.Timeout.ShouldBe(newTimeout);
        result.ExpectedResultType.ShouldBe(typeof(DateTime));
    }

    [Fact]
    public void ShouldWorkWithCustomTypes()
    {
        // Arrange
        var customData = new CustomTestType { Id = 1, Name = "Test" };

        // Act
        var command = new TestGenericDataCommand<CustomTestType>("TestCommand", "TestConnection");

        // Assert
        command.ExpectedResultType.ShouldBe(typeof(CustomTestType));
        command.ShouldNotBeNull();

        _output.WriteLine($"Custom type command: {command}");
    }

    [Fact]
    public void ShouldWorkWithNullableTypes()
    {
        // Arrange & Act
        var command = new TestGenericDataCommand<int?>("TestCommand", "TestConnection");

        // Assert
        command.ExpectedResultType.ShouldBe(typeof(int?));
        command.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldWorkWithCollectionTypes()
    {
        // Arrange & Act
        var command = new TestGenericDataCommand<IEnumerable<string>>("TestCommand", "TestConnection");

        // Assert
        command.ExpectedResultType.ShouldBe(typeof(IEnumerable<string>));
        command.ShouldNotBeNull();
    }

    [Fact]
    public void AllWithMethodsShouldPreserveGenericType()
    {
        // Arrange
        var originalCommand = new TestGenericDataCommand<CustomTestType>("TestCommand", "TestConnection");
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal) { { "test", "value" } };
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal) { { "cache", true } };
        var target = new DataPath(["test", "path"]);
        var timeout = TimeSpan.FromSeconds(45);

        // Act
        var withParams = originalCommand.WithParameters(parameters);
        var withMeta = originalCommand.WithMetadata(metadata);
        var withConnection = originalCommand.WithConnection("NewConnection");
        var withTarget = originalCommand.WithTarget(target);
        var withTimeout = originalCommand.WithTimeout(timeout);

        // Assert
        withParams.ExpectedResultType.ShouldBe(typeof(CustomTestType));
        withMeta.ExpectedResultType.ShouldBe(typeof(CustomTestType));
        withConnection.ExpectedResultType.ShouldBe(typeof(CustomTestType));
        withTarget.ExpectedResultType.ShouldBe(typeof(CustomTestType));
        withTimeout.ExpectedResultType.ShouldBe(typeof(CustomTestType));

        // Verify they're all the correct concrete type
        withParams.ShouldBeOfType<TestGenericDataCommand<CustomTestType>>();
        withMeta.ShouldBeOfType<TestGenericDataCommand<CustomTestType>>();
        withConnection.ShouldBeOfType<TestGenericDataCommand<CustomTestType>>();
        withTarget.ShouldBeOfType<TestGenericDataCommand<CustomTestType>>();
        withTimeout.ShouldBeOfType<TestGenericDataCommand<CustomTestType>>();
    }

    [Fact]
    public void CanCastToNonGenericInterface()
    {
        // Arrange
        var command = new TestGenericDataCommand<string>("TestCommand", "TestConnection");

        // Act
        IDataCommand nonGeneric = command;

        // Assert
        nonGeneric.ShouldNotBeNull();
        nonGeneric.ExpectedResultType.ShouldBe(typeof(string));
        nonGeneric.CommandType.ShouldBe("TestCommand");
        // Note: ConnectionName is not part of IDataCommand interface, it's on the concrete type
        (nonGeneric as TestGenericDataCommand<string>)?.ConnectionName.ShouldBe("TestConnection");
    }

    // Test custom type
    private sealed class CustomTestType
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    // Test implementation of generic DataCommandBase
    private sealed class TestGenericDataCommand<TResult> : DataCommandBase<TResult>
    {
        public TestGenericDataCommand(
            string commandType,
            string connectionName,
            DataPath? targetContainer = null,
            IReadOnlyDictionary<string, object?>? parameters = null,
            IReadOnlyDictionary<string, object>? metadata = null,
            TimeSpan? timeout = null)
            : base(commandType, connectionName, targetContainer, parameters, metadata, timeout)
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
            return new TestGenericDataCommand<TResult>(CommandType, connectionName, targetContainer, parameters, metadata, timeout);
        }
    }
}