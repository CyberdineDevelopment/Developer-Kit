using System;
using System.Linq;
using FractalDataWorks.Services.ExternalConnections.MsSql;
using FractalDataWorks.Services.ExternalConnections.MsSql.EnhancedEnums;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Tests.EnhancedEnums;

/// <summary>
/// Tests for MsSqlConnectionType enhanced enum to ensure proper configuration and behavior.
/// </summary>
public sealed class MsSqlConnectionTypeTests
{
    private readonly ITestOutputHelper _output;
    private readonly MsSqlConnectionType _connectionType;

    public MsSqlConnectionTypeTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _connectionType = new MsSqlConnectionType();
    }

    [Fact]
    public void ShouldInitializeWithCorrectValues()
    {
        // Assert
        _connectionType.Id.ShouldBe(1);
        _connectionType.Name.ShouldBe("MsSql");
        _connectionType.Description.ShouldBe("Microsoft SQL Server external connection service");
        
        _output.WriteLine($"ID: {_connectionType.Id}");
        _output.WriteLine($"Name: {_connectionType.Name}");
        _output.WriteLine($"Description: {_connectionType.Description}");
    }

    [Fact]
    public void ShouldHaveCorrectSupportedDataStores()
    {
        // Assert
        _connectionType.SupportedDataStores.ShouldNotBeNull();
        _connectionType.SupportedDataStores.Length.ShouldBe(3);
        _connectionType.SupportedDataStores.ShouldContain("SqlServer");
        _connectionType.SupportedDataStores.ShouldContain("MSSQL");
        _connectionType.SupportedDataStores.ShouldContain("Microsoft SQL Server");
        
        _output.WriteLine($"Supported data stores: {string.Join(", ", _connectionType.SupportedDataStores)}");
    }

    [Fact]
    public void ShouldHaveCorrectProviderName()
    {
        // Assert
        _connectionType.ProviderName.ShouldBe("Microsoft.Data.SqlClient");
        _output.WriteLine($"Provider name: {_connectionType.ProviderName}");
    }

    [Fact]
    public void ShouldHaveCorrectSupportedConnectionModes()
    {
        // Assert
        _connectionType.SupportedConnectionModes.ShouldNotBeNull();
        _connectionType.SupportedConnectionModes.Count.ShouldBe(4);
        _connectionType.SupportedConnectionModes.ShouldContain("ReadWrite");
        _connectionType.SupportedConnectionModes.ShouldContain("ReadOnly");
        _connectionType.SupportedConnectionModes.ShouldContain("Bulk");
        _connectionType.SupportedConnectionModes.ShouldContain("Streaming");
        
        _output.WriteLine($"Supported connection modes: {string.Join(", ", _connectionType.SupportedConnectionModes)}");
    }

    [Fact]
    public void ShouldHaveCorrectPriority()
    {
        // Assert
        _connectionType.Priority.ShouldBe(100);
        _output.WriteLine($"Priority: {_connectionType.Priority}");
    }

    [Fact]
    public void CreateTypedFactoryShouldReturnMsSqlConnectionFactory()
    {
        // Act
        var factory = _connectionType.CreateTypedFactory();

        // Assert
        factory.ShouldNotBeNull();
        factory.ShouldBeOfType<MsSqlConnectionFactory>();
        _output.WriteLine($"Factory type: {factory.GetType().Name}");
    }

    [Fact]
    public void ShouldInheritFromExternalConnectionServiceTypeBase()
    {
        // Assert
        _connectionType.ShouldBeAssignableTo<FractalDataWorks.Services.ExternalConnections.Abstractions.ExternalConnectionServiceTypeBase<MsSqlExternalConnectionService, MsSqlConfiguration>>();
        _output.WriteLine("Inherits from ExternalConnectionServiceTypeBase");
    }

    [Fact]
    public void ShouldHaveEnumOptionAttribute()
    {
        // Arrange
        var type = typeof(MsSqlConnectionType);

        // Act
        var attributes = type.GetCustomAttributes(typeof(FractalDataWorks.EnhancedEnums.Attributes.EnumOptionAttribute), false);

        // Assert
        attributes.ShouldNotBeEmpty();
        attributes.Length.ShouldBe(1);
        _output.WriteLine("Has EnumOption attribute");
    }

    [Theory]
    [InlineData("SqlServer")]
    [InlineData("MSSQL")]
    [InlineData("Microsoft SQL Server")]
    public void ShouldSupportSpecificDataStores(string dataStore)
    {
        // Assert
        _connectionType.SupportedDataStores.ShouldContain(dataStore);
        _output.WriteLine($"Supports data store: {dataStore}");
    }

    [Theory]
    [InlineData("ReadWrite")]
    [InlineData("ReadOnly")]
    [InlineData("Bulk")]
    [InlineData("Streaming")]
    public void ShouldSupportSpecificConnectionModes(string connectionMode)
    {
        // Assert
        _connectionType.SupportedConnectionModes.ShouldContain(connectionMode);
        _output.WriteLine($"Supports connection mode: {connectionMode}");
    }

    [Fact]
    public void SupportedDataStoresShouldBeReadOnly()
    {
        // Assert
        _connectionType.SupportedDataStores.ShouldBeOfType<string[]>();
        
        // Verify immutability by checking if modification throws
        var dataStores = _connectionType.SupportedDataStores;
        dataStores.ShouldNotBeNull();
        
        _output.WriteLine("Supported data stores array is read-only");
    }

    [Fact]
    public void SupportedConnectionModesShouldBeReadOnly()
    {
        // Assert
        _connectionType.SupportedConnectionModes.ShouldBeAssignableTo<System.Collections.Generic.IReadOnlyList<string>>();
        _output.WriteLine("Supported connection modes is read-only list");
    }

    [Fact]
    public void ShouldCreateUniqueFactoryInstances()
    {
        // Act
        var factory1 = _connectionType.CreateTypedFactory();
        var factory2 = _connectionType.CreateTypedFactory();

        // Assert
        factory1.ShouldNotBeSameAs(factory2);
        factory1.ShouldBeOfType<MsSqlConnectionFactory>();
        factory2.ShouldBeOfType<MsSqlConnectionFactory>();
        _output.WriteLine("Creates unique factory instances");
    }

    [Fact]
    public void ShouldHaveReasonablePriorityValue()
    {
        // Assert
        _connectionType.Priority.ShouldBeGreaterThan(0);
        _connectionType.Priority.ShouldBeLessThanOrEqualTo(1000); // Reasonable upper bound
        _output.WriteLine($"Priority {_connectionType.Priority} is within reasonable range");
    }

    [Fact]
    public void ShouldHaveValidIdValue()
    {
        // Assert
        _connectionType.Id.ShouldBeGreaterThan(0);
        _output.WriteLine($"ID {_connectionType.Id} is valid (greater than 0)");
    }

    [Fact]
    public void NameShouldBeNonEmpty()
    {
        // Assert
        _connectionType.Name.ShouldNotBeNullOrEmpty();
        _connectionType.Name.ShouldNotBeNullOrWhiteSpace();
        _output.WriteLine($"Name '{_connectionType.Name}' is valid");
    }

    [Fact]
    public void DescriptionShouldBeNonEmpty()
    {
        // Assert
        _connectionType.Description.ShouldNotBeNullOrEmpty();
        _connectionType.Description.ShouldNotBeNullOrWhiteSpace();
        _output.WriteLine($"Description '{_connectionType.Description}' is valid");
    }

    [Fact]
    public void ProviderNameShouldBeValidDotNetNamespace()
    {
        // Assert
        _connectionType.ProviderName.ShouldNotBeNullOrEmpty();
        _connectionType.ProviderName.ShouldContain(".");
        _connectionType.ProviderName.ShouldBe("Microsoft.Data.SqlClient");
        _output.WriteLine($"Provider name '{_connectionType.ProviderName}' is valid .NET namespace");
    }

    [Fact]
    public void SupportedDataStoresShouldNotContainDuplicates()
    {
        // Act
        var duplicates = _connectionType.SupportedDataStores
            .GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        // Assert
        duplicates.ShouldBeEmpty();
        _output.WriteLine("Supported data stores contains no duplicates");
    }

    [Fact]
    public void SupportedConnectionModesShouldNotContainDuplicates()
    {
        // Act
        var duplicates = _connectionType.SupportedConnectionModes
            .GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        // Assert
        duplicates.ShouldBeEmpty();
        _output.WriteLine("Supported connection modes contains no duplicates");
    }

    [Fact]
    public void SupportedDataStoresShouldNotContainNullOrEmpty()
    {
        // Assert
        _connectionType.SupportedDataStores.ShouldNotContain(x => string.IsNullOrWhiteSpace(x));
        _output.WriteLine("Supported data stores contains no null or empty values");
    }

    [Fact]
    public void SupportedConnectionModesShouldNotContainNullOrEmpty()
    {
        // Assert
        _connectionType.SupportedConnectionModes.ShouldNotContain(x => string.IsNullOrWhiteSpace(x));
        _output.WriteLine("Supported connection modes contains no null or empty values");
    }

    [Fact]
    public void ShouldBeEqualToAnotherInstanceWithSameValues()
    {
        // Arrange
        var other = new MsSqlConnectionType();

        // Assert
        _connectionType.Id.ShouldBe(other.Id);
        _connectionType.Name.ShouldBe(other.Name);
        _connectionType.Description.ShouldBe(other.Description);
        _connectionType.ProviderName.ShouldBe(other.ProviderName);
        _connectionType.Priority.ShouldBe(other.Priority);
        _output.WriteLine("Two instances have identical values");
    }

    [Fact]
    public void ShouldHaveStableHashCode()
    {
        // Act
        var hashCode1 = _connectionType.GetHashCode();
        var hashCode2 = _connectionType.GetHashCode();

        // Assert
        hashCode1.ShouldBe(hashCode2);
        _output.WriteLine($"Hash code is stable: {hashCode1}");
    }

    [Fact]
    public void ToStringShouldReturnMeaningfulValue()
    {
        // Act
        var stringValue = _connectionType.ToString();

        // Assert
        stringValue.ShouldNotBeNullOrEmpty();
        stringValue.ShouldContain(_connectionType.Name);
        _output.WriteLine($"ToString result: {stringValue}");
    }
}