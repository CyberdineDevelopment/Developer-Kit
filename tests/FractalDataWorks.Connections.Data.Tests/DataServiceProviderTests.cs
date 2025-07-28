using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Connections;
using FractalDataWorks.Connections.Data;
using FractalDataWorks.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Connections.Data.Tests;

public class DataServiceProviderTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DataProviderConfiguration _configuration;
    private readonly ILogger<DataServiceProvider> _logger;
    private readonly DataServiceProvider _sut;
    
    public DataServiceProviderTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        _logger = Substitute.For<ILogger<DataServiceProvider>>();
        
        _configuration = new DataProviderConfiguration
        {
            Connections = new Dictionary<string, ConnectionConfiguration>
            {
                ["test-connection"] = new ConnectionConfiguration
                {
                    DataStore = "TestProvider",
                    Enabled = true,
                    Settings = new Dictionary<string, object>
                    {
                        ["ConnectionString"] = "TestConnectionString"
                    }
                },
                ["disabled-connection"] = new ConnectionConfiguration
                {
                    DataStore = "TestProvider",
                    Enabled = false
                }
            },
            DefaultConnectionId = "test-connection"
        };
        
        _sut = new DataServiceProvider(_serviceProvider, _configuration, _logger);
    }
    
    [Fact]
    public void ConstructorShouldThrowWhenServiceProviderIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            new DataServiceProvider(null!, _configuration, _logger))
            .ParamName.ShouldBe("serviceProvider");
    }
    
    [Fact]
    public void ConstructorShouldThrowWhenConfigurationIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            new DataServiceProvider(_serviceProvider, null!, _logger))
            .ParamName.ShouldBe("configuration");
    }
    
    [Fact]
    public void ConstructorShouldThrowWhenLoggerIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            new DataServiceProvider(_serviceProvider, _configuration, null!))
            .ParamName.ShouldBe("logger");
    }
    
    [Fact]
    public void GetConnectionShouldReturnErrorForUnknownDataStore()
    {
        // Act
        var result = _sut.GetConnection("UnknownProvider");
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldContain("Unknown data store type");
    }
    
    [Fact]
    public void GetConnectionShouldReturnErrorWhenConnectionNotRegistered()
    {
        // Arrange
        MockDataProviders.Setup("TestProvider", typeof(TestConnection));
        
        // Act
        var result = _sut.GetConnection("TestProvider");
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldContain("not registered in DI container");
    }
    
    [Fact]
    public void GetConnectionShouldReturnSuccessWhenConnectionExists()
    {
        // Arrange
        var mockConnection = Substitute.For<IDataConnection>();
        MockDataProviders.Setup("TestProvider", typeof(TestConnection));
        _serviceProvider.GetService(typeof(TestConnection)).Returns(mockConnection);
        
        // Act
        var result = _sut.GetConnection("TestProvider");
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(mockConnection);
    }
    
    [Fact]
    public void GetConnectionByIdShouldReturnErrorForUnknownId()
    {
        // Act
        var result = _sut.GetConnectionById("unknown-id");
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldContain("not found in configuration");
    }
    
    [Fact]
    public void GetConnectionByIdShouldReturnErrorForDisabledConnection()
    {
        // Act
        var result = _sut.GetConnectionById("disabled-connection");
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldContain("is disabled");
    }
    
    [Fact]
    public void GetConnectionByIdShouldReturnSuccessForEnabledConnection()
    {
        // Arrange
        var mockConnection = Substitute.For<IDataConnection>();
        MockDataProviders.Setup("TestProvider", typeof(TestConnection));
        _serviceProvider.GetService(typeof(TestConnection)).Returns(mockConnection);
        
        // Act
        var result = _sut.GetConnectionById("test-connection");
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(mockConnection);
    }
    
    [Fact]
    public void GetConnectionByIdShouldApplySettingsToConfigurableConnection()
    {
        // Arrange
        var mockConnection = Substitute.For<IDataConnection, IConfigurableConnection>();
        MockDataProviders.Setup("TestProvider", typeof(TestConnection));
        _serviceProvider.GetService(typeof(TestConnection)).Returns(mockConnection);
        
        // Act
        var result = _sut.GetConnectionById("test-connection");
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        ((IConfigurableConnection)mockConnection).Received(1)
            .ApplySettings(Arg.Is<Dictionary<string, object>>(d => 
                d.ContainsKey("ConnectionString") && 
                d["ConnectionString"].Equals("TestConnectionString")));
    }
    
    [Fact]
    public async Task TestConnectionShouldReturnFailureWhenConnectionNotFound()
    {
        // Act
        var result = await _sut.TestConnection("unknown-id");
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldContain("not found in configuration");
    }
    
    [Fact]
    public async Task TestConnectionShouldReturnSuccessWhenConnectionTestPasses()
    {
        // Arrange
        var mockConnection = Substitute.For<IDataConnection>();
        mockConnection.TestConnection(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(GenericResult<bool>.Success(true)));
        
        MockDataProviders.Setup("TestProvider", typeof(TestConnection));
        _serviceProvider.GetService(typeof(TestConnection)).Returns(mockConnection);
        
        // Act
        var result = await _sut.TestConnection("test-connection");
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }
    
    [Fact]
    public async Task TestConnectionShouldReturnFailureWhenConnectionTestFails()
    {
        // Arrange
        var mockConnection = Substitute.For<IDataConnection>();
        mockConnection.TestConnection(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(GenericResult<bool>.Failure("Connection failed")));
        
        MockDataProviders.Setup("TestProvider", typeof(TestConnection));
        _serviceProvider.GetService(typeof(TestConnection)).Returns(mockConnection);
        
        // Act
        var result = await _sut.TestConnection("test-connection");
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldContain("Connection failed");
    }
    
    [Fact]
    public void GetAvailableConnectionsShouldReturnOnlyEnabledConnections()
    {
        // Act
        var result = _sut.GetAvailableConnections();
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(new[] { "test-connection" });
        result.Value.ShouldNotContain("disabled-connection");
    }
    
    [Fact]
    public void GetAvailableConnectionsShouldReturnEmptyArrayWhenNoEnabledConnections()
    {
        // Arrange
        _configuration.Connections["test-connection"].Enabled = false;
        var provider = new DataServiceProvider(_serviceProvider, _configuration, _logger);
        
        // Act
        var result = provider.GetAvailableConnections();
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }
    
    [Fact]
    public void GetAvailableConnectionsShouldReturnSortedConnections()
    {
        // Arrange
        _configuration.Connections["z-connection"] = new ConnectionConfiguration 
        { 
            DataStore = "Test", 
            Enabled = true 
        };
        _configuration.Connections["a-connection"] = new ConnectionConfiguration 
        { 
            DataStore = "Test", 
            Enabled = true 
        };
        
        var provider = new DataServiceProvider(_serviceProvider, _configuration, _logger);
        
        // Act
        var result = provider.GetAvailableConnections();
        
        // Assert
        result.Value.ShouldBe(new[] { "a-connection", "test-connection", "z-connection" });
    }
    
    // Test helpers
    private class TestConnection : IDataConnection
    {
        public string ProviderName => "TestProvider";
        public ProviderCapabilities Capabilities => ProviderCapabilities.BasicCrud;
        
        public Task<IGenericResult<TResult>> Execute<TResult>(IDataCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResult<TResult>.Success(default(TResult)!));
        
        public Task<IGenericResult<bool>> TestConnection(CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResult<bool>.Success(true));
        
        // Other interface members would be implemented here
    }
    
    // Mock for DataProviders enhanced enum
    private static class MockDataProviders
    {
        private static readonly Dictionary<string, IDataProviderType> _providers = new();
        
        public static IEnumerable<IDataProviderType> All => _providers.Values;
        
        public static void Setup(string name, Type connectionType)
        {
            _providers[name] = new MockProviderType 
            { 
                Name = name, 
                ConnectionType = connectionType 
            };
        }
        
        private class MockProviderType : IDataProviderType
        {
            public int Id => 1;
            public string Name { get; init; } = "";
            public string ProviderName => Name;
            public Type ConnectionType { get; init; } = typeof(object);
            public Type TranslatorType => typeof(object);
            public Type ConfigurationType => typeof(ConnectionConfiguration);
            public ProviderCapabilities Capabilities => ProviderCapabilities.BasicCrud;
        }
    }
}

// TODO: Remove when real DataProviders enhanced enum is available