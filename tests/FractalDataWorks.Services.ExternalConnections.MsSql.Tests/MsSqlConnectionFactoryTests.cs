using System;
using System.Threading.Tasks;
using FractalDataWorks.Services.ExternalConnections.MsSql;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Tests;

/// <summary>
/// Tests for MsSqlConnectionFactory to ensure proper factory behavior and public interface.
/// </summary>
public sealed class MsSqlConnectionFactoryTests
{
    private readonly ITestOutputHelper _output;
    private readonly ILoggerFactory _loggerFactory;

    public MsSqlConnectionFactoryTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _loggerFactory = NullLoggerFactory.Instance;
    }

    [Fact]
    public void ShouldCreateWithLoggerAndLoggerFactory()
    {
        // Arrange
        var logger = _loggerFactory.CreateLogger<MsSqlConnectionFactory>();

        // Act
        var factory = new MsSqlConnectionFactory(logger, _loggerFactory);

        // Assert
        factory.ShouldNotBeNull();

        _output.WriteLine("Factory created successfully with logger and logger factory");
    }

    [Fact]
    public void ShouldCreateWithParameterlessConstructor()
    {
        // Act
        var factory = new MsSqlConnectionFactory();

        // Assert
        factory.ShouldNotBeNull();

        _output.WriteLine("Factory created successfully with parameterless constructor");
    }

    [Fact]
    public void ConstructorShouldThrowWhenLoggerFactoryIsNull()
    {
        // Arrange
        var logger = _loggerFactory.CreateLogger<MsSqlConnectionFactory>();

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => new MsSqlConnectionFactory(logger, null));
        exception.ParamName.ShouldBe("loggerFactory");

        _output.WriteLine("Constructor correctly throws ArgumentNullException when loggerFactory is null");
    }

    [Fact]
    public async Task GetServiceByNameShouldThrowNotSupportedException()
    {
        // Arrange
        var factory = new MsSqlConnectionFactory();

        // Act & Assert
        var exception = await Should.ThrowAsync<NotSupportedException>(() => factory.GetService("TestConfig"));
        exception.Message.ShouldContain("Configuration name-based service creation requires a configuration provider");

        _output.WriteLine("GetService(string) correctly throws NotSupportedException");
    }

    [Fact]
    public async Task GetServiceByIdShouldThrowNotSupportedException()
    {
        // Arrange
        var factory = new MsSqlConnectionFactory();

        // Act & Assert
        var exception = await Should.ThrowAsync<NotSupportedException>(() => factory.GetService(123));
        exception.Message.ShouldContain("Configuration ID-based service creation requires a configuration provider");

        _output.WriteLine("GetService(int) correctly throws NotSupportedException");
    }

    [Fact]
    public void ShouldInheritFromServiceFactoryBase()
    {
        // Arrange & Act
        var factory = new MsSqlConnectionFactory();

        // Assert
        factory.ShouldBeAssignableTo<FractalDataWorks.Services.ServiceFactoryBase<MsSqlExternalConnectionService, MsSqlConfiguration>>();

        _output.WriteLine("MsSqlConnectionFactory correctly inherits from ServiceFactoryBase");
    }

    [Fact]
    public void ShouldHandleNullLoggerInParameterlessConstructor()
    {
        // Act
        var factory = new MsSqlConnectionFactory();

        // Assert
        factory.ShouldNotBeNull();
        // The factory should be able to operate with a null logger (handled internally)

        _output.WriteLine("Parameterless constructor handles null logger gracefully");
    }

    [Fact]
    public void ShouldCreateMultipleInstances()
    {
        // Act
        var factory1 = new MsSqlConnectionFactory();
        var factory2 = new MsSqlConnectionFactory();

        // Assert
        factory1.ShouldNotBeSameAs(factory2);
        factory1.ShouldNotBeNull();
        factory2.ShouldNotBeNull();

        _output.WriteLine("Multiple factory instances can be created");
    }

    [Fact]
    public void FactoryWithLoggerFactoryShouldNotBeNull()
    {
        // Arrange
        var logger = _loggerFactory.CreateLogger<MsSqlConnectionFactory>();

        // Act
        var factory = new MsSqlConnectionFactory(logger, _loggerFactory);

        // Assert
        factory.ShouldNotBeNull();

        _output.WriteLine("Factory created with logger factory is not null");
    }

    [Fact]
    public void ShouldAcceptNullLogger()
    {
        // Act
        var factory = new MsSqlConnectionFactory(null, _loggerFactory);

        // Assert
        factory.ShouldNotBeNull();

        _output.WriteLine("Factory accepts null logger parameter");
    }
}