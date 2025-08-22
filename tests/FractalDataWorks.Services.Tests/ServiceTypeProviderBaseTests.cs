using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

public class ServiceTypeProviderBaseTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ILogger<TestServiceTypeProvider>> _loggerMock;
    private readonly Mock<IConfigurationRegistry<ITestConfiguration>> _configRegistryMock;
    private readonly Mock<ITestService> _serviceMock;
    private readonly List<TestServiceType> _serviceTypes;

    public ServiceTypeProviderBaseTests(ITestOutputHelper output)
    {
        _output = output;
        _loggerMock = new Mock<ILogger<TestServiceTypeProvider>>();
        _configRegistryMock = new Mock<IConfigurationRegistry<ITestConfiguration>>();
        _serviceMock = new Mock<ITestService>();
        
        _serviceTypes = new List<TestServiceType>
        {
            new TestServiceType(1, "Service1", "Description1"),
            new TestServiceType(2, "Service2", "Description2")
        };
    }

    // Test Interfaces
    public interface ITestService : IFdwService
    {
        string TestProperty { get; }
    }

    public interface ITestConfiguration : IFdwConfiguration
    {
        string TestValue { get; }
    }

    // Concrete ServiceType Implementation
    public class TestServiceType : ServiceTypeBase<ITestService, ITestConfiguration>
    {
        private readonly Func<ITestConfiguration, IFdwResult<ITestService>>? _createFunc;

        public TestServiceType(int id, string name, string description, 
            Func<ITestConfiguration, IFdwResult<ITestService>>? createFunc = null)
            : base(id, name, description)
        {
            _createFunc = createFunc;
        }

        public override IServiceFactory<ITestService, ITestConfiguration> CreateTypedFactory()
        {
            var factory = new Mock<IServiceFactory<ITestService, ITestConfiguration>>();
            if (_createFunc != null)
            {
                factory.Setup(f => f.Create(It.IsAny<ITestConfiguration>())).Returns(_createFunc);
            }
            return factory.Object;
        }
    }

    // Concrete Provider Implementation
    public class TestServiceTypeProvider : ServiceTypeProviderBase<ITestService, TestServiceType, ITestConfiguration>
    {
        public TestServiceTypeProvider(
            ILogger<TestServiceTypeProvider> logger,
            IConfigurationRegistry<ITestConfiguration> configurationRegistry,
            IEnumerable<TestServiceType> serviceTypes)
            : base(logger, configurationRegistry, serviceTypes)
        {
        }

        public TestServiceTypeProvider(
            IConfigurationRegistry<ITestConfiguration> configurationRegistry,
            IEnumerable<TestServiceType> serviceTypes)
            : base(configurationRegistry, serviceTypes)
        {
        }

        // Expose protected method for testing
        public new Task<IFdwResult<ITestConfiguration>> GetConfigurationAsync(string serviceTypeName, CancellationToken cancellationToken)
        {
            return base.GetConfigurationAsync(serviceTypeName, cancellationToken);
        }
    }

    [Fact]
    public void ConstructorWithLoggerShouldInitializeCorrectly()
    {
        // Arrange & Act
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, _serviceTypes);

        // Assert
        provider.ShouldNotBeNull();
        provider.ServiceTypes.ShouldNotBeNull();
        provider.ServiceTypes.Count.ShouldBe(2);
        provider.ServiceTypes.ShouldContainKey("Service1");
        provider.ServiceTypes.ShouldContainKey("Service2");
        
        _output.WriteLine($"Provider initialized with {provider.ServiceTypes.Count} service types");
    }

    [Fact]
    public void ConstructorWithoutLoggerShouldInitializeCorrectly()
    {
        // Arrange & Act
        var provider = new TestServiceTypeProvider(_configRegistryMock.Object, _serviceTypes);

        // Assert
        provider.ShouldNotBeNull();
        provider.ServiceTypes.ShouldNotBeNull();
        provider.ServiceTypes.Count.ShouldBe(2);
        
        _output.WriteLine($"Provider initialized without logger with {provider.ServiceTypes.Count} service types");
    }

    [Fact]
    public async Task GetServiceAsyncWithValidServiceTypeNameShouldReturnService()
    {
        // Arrange
        var config = Mock.Of<ITestConfiguration>();
        var expectedService = _serviceMock.Object;
        
        _configRegistryMock.Setup(r => r.GetAll()).Returns(new[] { config });
        
        var serviceTypeWithFunc = new TestServiceType(1, "Service1", "Description1",
            c => FdwResult<ITestService>.Success(expectedService));
        var serviceTypesWithFunc = new List<TestServiceType> { serviceTypeWithFunc };
        
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, serviceTypesWithFunc);

        // Act
        var result = await provider.GetServiceAsync("Service1");

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedService);
        
        _output.WriteLine($"Service retrieval successful: {result.Value?.GetType().Name}");
    }

    [Fact]
    public async Task GetServiceAsyncWithEmptyServiceTypeNameShouldReturnFailure()
    {
        // Arrange
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, _serviceTypes);

        // Act
        var result = await provider.GetServiceAsync("");

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Service type name cannot be null or empty");
        
        _output.WriteLine($"Empty service type name handled: {result.Message}");
    }

    [Fact]
    public async Task GetServiceAsyncWithNullServiceTypeNameShouldReturnFailure()
    {
        // Arrange
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, _serviceTypes);

        // Act
        var result = await provider.GetServiceAsync(null!);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Service type name cannot be null or empty");
        
        _output.WriteLine($"Null service type name handled: {result.Message}");
    }

    [Fact]
    public async Task GetServiceAsyncWithUnknownServiceTypeNameShouldReturnFailure()
    {
        // Arrange
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, _serviceTypes);

        // Act
        var result = await provider.GetServiceAsync("UnknownService");

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Service type 'UnknownService' not found");
        
        _output.WriteLine($"Unknown service type handled: {result.Message}");
    }

    [Fact]
    public async Task GetServiceAsyncWithNoConfigurationShouldReturnFailure()
    {
        // Arrange
        _configRegistryMock.Setup(r => r.GetAll()).Returns(Enumerable.Empty<ITestConfiguration>());
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, _serviceTypes);

        // Act
        var result = await provider.GetServiceAsync("Service1");

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldContain("Configuration not found for service type 'Service1'");
        
        _output.WriteLine($"No configuration handled: {result.Message}");
    }

    [Fact]
    public async Task GetServiceAsyncWithServiceCreationFailureShouldReturnFailure()
    {
        // Arrange
        var config = Mock.Of<ITestConfiguration>();
        _configRegistryMock.Setup(r => r.GetAll()).Returns(new[] { config });
        
        var serviceTypeWithError = new TestServiceType(1, "Service1", "Description1",
            c => FdwResult<ITestService>.Failure("Creation failed"));
        var serviceTypesWithError = new List<TestServiceType> { serviceTypeWithError };
        
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, serviceTypesWithError);

        // Act
        var result = await provider.GetServiceAsync("Service1");

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Creation failed");
        
        _output.WriteLine($"Service creation failure handled: {result.Message}");
    }

    [Fact]
    public async Task GetServiceAsyncWithServiceTypeShouldCallNameBasedMethod()
    {
        // Arrange
        var config = Mock.Of<ITestConfiguration>();
        var expectedService = _serviceMock.Object;
        
        _configRegistryMock.Setup(r => r.GetAll()).Returns(new[] { config });
        
        var serviceTypeWithFunc = new TestServiceType(1, "Service1", "Description1",
            c => FdwResult<ITestService>.Success(expectedService));
        var serviceTypesWithFunc = new List<TestServiceType> { serviceTypeWithFunc };
        
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, serviceTypesWithFunc);

        // Act
        var result = await provider.GetServiceAsync(serviceTypeWithFunc);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedService);
        
        _output.WriteLine($"Service type-based retrieval successful: {result.Value?.GetType().Name}");
    }

    [Fact]
    public async Task GetServiceAsyncWithNullServiceTypeShouldReturnFailure()
    {
        // Arrange
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, _serviceTypes);

        // Act
        var result = await provider.GetServiceAsync((TestServiceType)null!);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Service type cannot be null");
        
        _output.WriteLine($"Null service type handled: {result.Message}");
    }

    [Fact]
    public void ValidateProviderWithValidSetupShouldReturnSuccess()
    {
        // Arrange
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, _serviceTypes);

        // Act
        var result = provider.ValidateProvider();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        
        _output.WriteLine($"Provider validation successful: {result.IsSuccess}");
    }

    [Fact]
    public void ValidateProviderWithNoServiceTypesShouldReturnFailure()
    {
        // Arrange
        var emptyServiceTypes = new List<TestServiceType>();
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, emptyServiceTypes);

        // Act
        var result = provider.ValidateProvider();

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("No service types registered");
        
        _output.WriteLine($"Empty service types validation: {result.Message}");
    }

    [Fact]
    public async Task GetAllServicesAsyncShouldReturnAllServiceResults()
    {
        // Arrange
        var config = Mock.Of<ITestConfiguration>();
        var expectedService1 = Mock.Of<ITestService>();
        var expectedService2 = Mock.Of<ITestService>();
        
        _configRegistryMock.Setup(r => r.GetAll()).Returns(new[] { config });
        
        var serviceTypesWithFuncs = new List<TestServiceType>
        {
            new TestServiceType(1, "Service1", "Description1", c => FdwResult<ITestService>.Success(expectedService1)),
            new TestServiceType(2, "Service2", "Description2", c => FdwResult<ITestService>.Success(expectedService2))
        };
        
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, serviceTypesWithFuncs);

        // Act
        var results = await provider.GetAllServicesAsync();

        // Assert
        results.ShouldNotBeNull();
        results.Count.ShouldBe(2);
        results.All(r => r.IsSuccess).ShouldBeTrue();
        
        _output.WriteLine($"Retrieved {results.Count} services, all successful: {results.All(r => r.IsSuccess)}");
    }

    [Fact]
    public async Task GetConfigurationAsyncShouldReturnFirstAvailableConfiguration()
    {
        // Arrange
        var config1 = Mock.Of<ITestConfiguration>();
        var config2 = Mock.Of<ITestConfiguration>();
        _configRegistryMock.Setup(r => r.GetAll()).Returns(new[] { config1, config2 });
        
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, _serviceTypes);

        // Act
        var result = await provider.GetConfigurationAsync("TestService", CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(config1);
        
        _output.WriteLine($"Configuration retrieval successful: First config returned");
    }

    [Fact]
    public async Task GetConfigurationAsyncWithNoConfigurationsShouldReturnFailure()
    {
        // Arrange
        _configRegistryMock.Setup(r => r.GetAll()).Returns(Enumerable.Empty<ITestConfiguration>());
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, _serviceTypes);

        // Act
        var result = await provider.GetConfigurationAsync("TestService", CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Configuration not found for service type 'TestService'");
        
        _output.WriteLine($"No configurations handled: {result.Message}");
    }

    [Theory]
    [InlineData("Service1")]
    [InlineData("service1")] // Test case insensitive
    [InlineData("SERVICE1")] // Test case insensitive
    public void ServiceTypesDictionaryShouldBeCaseInsensitive(string serviceName)
    {
        // Arrange
        var provider = new TestServiceTypeProvider(_loggerMock.Object, _configRegistryMock.Object, _serviceTypes);

        // Act
        var hasService = provider.ServiceTypes.ContainsKey(serviceName);

        // Assert
        hasService.ShouldBeTrue();
        
        _output.WriteLine($"Case insensitive lookup for '{serviceName}': {hasService}");
    }
}