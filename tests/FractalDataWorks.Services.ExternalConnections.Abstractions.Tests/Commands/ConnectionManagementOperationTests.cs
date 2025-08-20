using System;
using System.Linq;
using Shouldly;
using Xunit;
using FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Tests.Commands;

/// <summary>
/// Tests for the ConnectionManagementOperation enumeration.
/// </summary>
public sealed class ConnectionManagementOperationTests
{
    [Fact]
    public void ShouldHaveListConnectionsAsDefaultValue()
    {
        // Arrange & Act
        var defaultOperation = default(ConnectionManagementOperation);

        // Assert
        defaultOperation.ShouldBe(ConnectionManagementOperation.ListConnections);
    }

    [Fact]
    public void ShouldHaveCorrectUnderlyingValues()
    {
        // Arrange & Act & Assert
        ((int)ConnectionManagementOperation.ListConnections).ShouldBe(0);
        ((int)ConnectionManagementOperation.RemoveConnection).ShouldBe(1);
        ((int)ConnectionManagementOperation.GetConnectionMetadata).ShouldBe(2);
        ((int)ConnectionManagementOperation.RefreshConnectionStatus).ShouldBe(3);
        ((int)ConnectionManagementOperation.TestConnection).ShouldBe(4);
    }

    [Fact]
    public void ShouldHaveAllExpectedOperations()
    {
        // Arrange
        var expectedOperations = new[]
        {
            ConnectionManagementOperation.ListConnections,
            ConnectionManagementOperation.RemoveConnection,
            ConnectionManagementOperation.GetConnectionMetadata,
            ConnectionManagementOperation.RefreshConnectionStatus,
            ConnectionManagementOperation.TestConnection
        };

        // Act
        var actualOperations = Enum.GetValues<ConnectionManagementOperation>();

        // Assert
        actualOperations.Length.ShouldBe(expectedOperations.Length);
        foreach (var expectedOperation in expectedOperations)
        {
            actualOperations.ShouldContain(expectedOperation);
        }
    }

    [Fact]
    public void ShouldHaveUniqueValues()
    {
        // Arrange & Act
        var allValues = Enum.GetValues<ConnectionManagementOperation>().Cast<int>().ToArray();
        var distinctValues = allValues.Distinct().ToArray();

        // Assert
        allValues.Length.ShouldBe(distinctValues.Length, "All enum values should be unique");
    }

    [Theory]
    [InlineData(ConnectionManagementOperation.ListConnections)]
    [InlineData(ConnectionManagementOperation.RemoveConnection)]
    [InlineData(ConnectionManagementOperation.GetConnectionMetadata)]
    [InlineData(ConnectionManagementOperation.RefreshConnectionStatus)]
    [InlineData(ConnectionManagementOperation.TestConnection)]
    public void ShouldConvertToStringCorrectly(ConnectionManagementOperation operation)
    {
        // Arrange & Act
        var stringValue = operation.ToString();

        // Assert
        stringValue.ShouldNotBeNullOrWhiteSpace();
        stringValue.ShouldBe(Enum.GetName(operation));
    }

    [Fact]
    public void ShouldParseFromStringCorrectly()
    {
        // Arrange & Act & Assert
        Enum.Parse<ConnectionManagementOperation>("ListConnections").ShouldBe(ConnectionManagementOperation.ListConnections);
        Enum.Parse<ConnectionManagementOperation>("RemoveConnection").ShouldBe(ConnectionManagementOperation.RemoveConnection);
        Enum.Parse<ConnectionManagementOperation>("GetConnectionMetadata").ShouldBe(ConnectionManagementOperation.GetConnectionMetadata);
        Enum.Parse<ConnectionManagementOperation>("RefreshConnectionStatus").ShouldBe(ConnectionManagementOperation.RefreshConnectionStatus);
        Enum.Parse<ConnectionManagementOperation>("TestConnection").ShouldBe(ConnectionManagementOperation.TestConnection);
    }

    [Fact]
    public void ShouldThrowWhenParsingInvalidString()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() => Enum.Parse<ConnectionManagementOperation>("InvalidOperation"));
    }

    [Fact]
    public void ShouldSupportIsDefined()
    {
        // Arrange & Act & Assert
        Enum.IsDefined(ConnectionManagementOperation.ListConnections).ShouldBeTrue();
        Enum.IsDefined(ConnectionManagementOperation.TestConnection).ShouldBeTrue();
        Enum.IsDefined((ConnectionManagementOperation)999).ShouldBeFalse();
    }

    [Fact]
    public void ShouldSupportComparisonOperations()
    {
        // Arrange & Act & Assert
        (ConnectionManagementOperation.ListConnections < ConnectionManagementOperation.TestConnection).ShouldBeTrue();
        (ConnectionManagementOperation.TestConnection > ConnectionManagementOperation.ListConnections).ShouldBeTrue();
        ConnectionManagementOperation.RemoveConnection.ShouldBe(ConnectionManagementOperation.RemoveConnection);
        ConnectionManagementOperation.ListConnections.ShouldNotBe(ConnectionManagementOperation.TestConnection);
    }

    [Theory]
    [InlineData(ConnectionManagementOperation.ListConnections, false)]
    [InlineData(ConnectionManagementOperation.RemoveConnection, true)]
    [InlineData(ConnectionManagementOperation.GetConnectionMetadata, true)]
    [InlineData(ConnectionManagementOperation.RefreshConnectionStatus, true)]
    [InlineData(ConnectionManagementOperation.TestConnection, true)]
    public void ShouldIdentifyOperationsThatRequireConnectionName(ConnectionManagementOperation operation, bool requiresConnectionName)
    {
        // Arrange & Act
        var actualRequiresConnectionName = RequiresConnectionName(operation);

        // Assert
        actualRequiresConnectionName.ShouldBe(requiresConnectionName);
    }

    [Theory]
    [InlineData(ConnectionManagementOperation.RemoveConnection, true)]
    [InlineData(ConnectionManagementOperation.TestConnection, true)]
    [InlineData(ConnectionManagementOperation.ListConnections, false)]
    [InlineData(ConnectionManagementOperation.GetConnectionMetadata, false)]
    [InlineData(ConnectionManagementOperation.RefreshConnectionStatus, false)]
    public void ShouldIdentifyOperationsThatModifyConnections(ConnectionManagementOperation operation, bool modifiesConnections)
    {
        // Arrange & Act
        var actualModifiesConnections = ModifiesConnections(operation);

        // Assert
        actualModifiesConnections.ShouldBe(modifiesConnections);
    }

    [Theory]
    [InlineData(ConnectionManagementOperation.ListConnections, false)]
    [InlineData(ConnectionManagementOperation.RemoveConnection, false)]
    [InlineData(ConnectionManagementOperation.GetConnectionMetadata, true)]
    [InlineData(ConnectionManagementOperation.RefreshConnectionStatus, true)]
    [InlineData(ConnectionManagementOperation.TestConnection, true)]
    public void ShouldIdentifyOperationsThatRequireActiveConnection(ConnectionManagementOperation operation, bool requiresActiveConnection)
    {
        // Arrange & Act
        var actualRequiresActiveConnection = RequiresActiveConnection(operation);

        // Assert
        actualRequiresActiveConnection.ShouldBe(requiresActiveConnection);
    }

    private static bool RequiresConnectionName(ConnectionManagementOperation operation)
    {
        return operation != ConnectionManagementOperation.ListConnections;
    }

    private static bool ModifiesConnections(ConnectionManagementOperation operation)
    {
        return operation is ConnectionManagementOperation.RemoveConnection or ConnectionManagementOperation.TestConnection;
    }

    private static bool RequiresActiveConnection(ConnectionManagementOperation operation)
    {
        return operation is ConnectionManagementOperation.GetConnectionMetadata 
                         or ConnectionManagementOperation.RefreshConnectionStatus 
                         or ConnectionManagementOperation.TestConnection;
    }
}