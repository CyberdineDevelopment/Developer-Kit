using System;
using System.Threading.Tasks;
using FluentValidation.Results;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

public class ServiceTypeBaseTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ITestService> _serviceMock;
    private readonly Mock<ITestConfiguration> _configMock;
    private readonly Mock<IServiceFactory<ITestService, ITestConfiguration>> _factoryMock;

    public ServiceTypeBaseTests(ITestOutputHelper output)
    {
        _output = output;
        _serviceMock = new Mock<ITestService>();
        _configMock = new Mock<ITestConfiguration>();
        _factoryMock = new Mock<IServiceFactory<ITestService, ITestConfiguration>>();
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

    // Concrete Implementation for Non-Generic ServiceTypeBase
    public class ConcreteServiceType : ServiceTypeBase
    {
        private readonly Func<IFdwResult<IServiceFactory>>? _createFactoryFunc;
        private readonly Func<IFdwConfiguration, IFdwResult<IFdwService>>? _createServiceFunc;

        public ConcreteServiceType(
            int id, 
            string name, 
            string description, 
            Func<IFdwResult<IServiceFactory>>? createFactoryFunc = null,
            Func<IFdwConfiguration, IFdwResult<IFdwService>>? createServiceFunc = null) 
            : base(id, name, description)
        {
            _createFactoryFunc = createFactoryFunc;
            _createServiceFunc = createServiceFunc;
        }

        public override IServiceFactory CreateFactory()
        {
            if (_createFactoryFunc != null)
            {
                var result = _createFactoryFunc();
                if (result.IsSuccess)
                    return result.Value!;
                throw new InvalidOperationException(result.Message);
            }
            return base.CreateFactory();
        }

        public override IFdwResult<T> Create<T>(IFdwConfiguration configuration)
        {
            if (_createServiceFunc != null)
            {
                var result = _createServiceFunc(configuration);
                if (result.IsSuccess && result.Value is T typedService)
                {
                    return FdwResult<T>.Success(typedService);
                }
                return FdwResult<T>.Failure(result.Message ?? "Service creation failed");
            }
            return base.Create<T>(configuration);
        }

        public override IFdwResult<IFdwService> Create(IFdwConfiguration configuration)
        {
            if (_createServiceFunc != null)
            {
                return _createServiceFunc(configuration);
            }
            return base.Create(configuration);
        }
    }

    // Concrete Implementation for Generic ServiceTypeBase
    public class ConcreteGenericServiceType : ServiceTypeBase<ITestService, ITestConfiguration>
    {
        private readonly Func<IServiceFactory<ITestService, ITestConfiguration>>? _createFactoryFunc;

        public ConcreteGenericServiceType(
            int id, 
            string name, 
            string description,
            Func<IServiceFactory<ITestService, ITestConfiguration>>? createFactoryFunc = null) 
            : base(id, name, description)
        {
            _createFactoryFunc = createFactoryFunc;
        }

        public override IServiceFactory<ITestService, ITestConfiguration> CreateTypedFactory()
        {
            return _createFactoryFunc?.Invoke() ?? _factoryMock.Object;
        }
    }

    #region Non-Generic ServiceTypeBase Tests

    [Fact]
    public void NonGenericServiceTypeConstructorShouldInitializeProperties()
    {
        // Arrange
        var id = 1;
        var name = "TestService";
        var description = "Test service description";

        // Act
        var serviceType = new ConcreteServiceType(id, name, description);

        // Assert
        serviceType.Id.ShouldBe(id);
        serviceType.Name.ShouldBe(name);
        serviceType.Description.ShouldBe(description);
        
        _output.WriteLine($"ServiceType created - ID: {serviceType.Id}, Name: {serviceType.Name}");
    }

    [Fact]
    public void NonGenericCreateFactoryDefaultShouldThrowNotSupportedException()
    {
        // Arrange
        var serviceType = new ConcreteServiceType(1, "TestService", "Description");

        // Act & Assert
        var exception = Should.Throw<NotSupportedException>(() => serviceType.CreateFactory());
        exception.Message.ShouldContain("TestService does not support factory creation");
        
        _output.WriteLine($"Expected exception: {exception.Message}");
    }

    [Fact]
    public void NonGenericCreateFactoryWithCustomImplementationShouldReturnFactory()
    {
        // Arrange
        var expectedFactory = Mock.Of<IServiceFactory>();
        var serviceType = new ConcreteServiceType(1, "TestService", "Description", 
            () => FdwResult<IServiceFactory>.Success(expectedFactory));

        // Act
        var result = serviceType.CreateFactory();

        // Assert
        result.ShouldBe(expectedFactory);
        
        _output.WriteLine($"Custom factory creation succeeded: {result.GetType().Name}");
    }

    [Fact]
    public void NonGenericCreateGenericDefaultShouldThrowNotSupportedException()
    {
        // Arrange
        var serviceType = new ConcreteServiceType(1, "TestService", "Description");
        var config = Mock.Of<IFdwConfiguration>();

        // Act & Assert
        var exception = Should.Throw<NotSupportedException>(() => serviceType.Create<IFdwService>(config));
        exception.Message.ShouldContain("TestService does not support direct service creation");
        
        _output.WriteLine($"Expected exception: {exception.Message}");
    }

    [Fact]
    public void NonGenericCreateGenericWithCustomImplementationShouldReturnService()
    {
        // Arrange
        var expectedService = Mock.Of<IFdwService>();
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        
        var serviceType = new ConcreteServiceType(1, "TestService", "Description",
            null,
            config => FdwResult<IFdwService>.Success(expectedService));

        // Act
        var result = serviceType.Create<IFdwService>(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedService);
        
        _output.WriteLine($"Custom generic service creation succeeded");
    }

    [Fact]
    public void NonGenericCreateDefaultShouldThrowNotSupportedException()
    {
        // Arrange
        var serviceType = new ConcreteServiceType(1, "TestService", "Description");
        var config = Mock.Of<IFdwConfiguration>();

        // Act & Assert
        var exception = Should.Throw<NotSupportedException>(() => serviceType.Create(config));
        exception.Message.ShouldContain("TestService does not support direct service creation");
        
        _output.WriteLine($"Expected exception: {exception.Message}");
    }

    [Fact]
    public void NonGenericCreateWithCustomImplementationShouldReturnService()
    {
        // Arrange
        var expectedService = Mock.Of<IFdwService>();
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        
        var serviceType = new ConcreteServiceType(1, "TestService", "Description",
            null,
            config => FdwResult<IFdwService>.Success(expectedService));

        // Act
        var result = serviceType.Create(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedService);
        
        _output.WriteLine($"Custom service creation succeeded");
    }

    #endregion

    #region Generic ServiceTypeBase Tests

    [Fact]
    public void GenericServiceTypeConstructorShouldInitializeProperties()
    {
        // Arrange
        var id = 2;
        var name = "GenericTestService";
        var description = "Generic test service description";

        // Act
        var serviceType = new ConcreteGenericServiceType(id, name, description);

        // Assert
        serviceType.Id.ShouldBe(id);
        serviceType.Name.ShouldBe(name);
        serviceType.Description.ShouldBe(description);
        
        _output.WriteLine($"Generic ServiceType created - ID: {serviceType.Id}, Name: {serviceType.Name}");
    }

    [Fact]
    public void GenericCreateTypedFactoryShouldReturnTypedFactory()
    {
        // Arrange
        var serviceType = new ConcreteGenericServiceType(1, "TestService", "Description");

        // Act
        var result = serviceType.CreateTypedFactory();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(_factoryMock.Object);
        
        _output.WriteLine($"Typed factory creation succeeded: {result.GetType().Name}");
    }

    [Fact]
    public void GenericCreateFactoryShouldReturnTypedFactory()
    {
        // Arrange
        var serviceType = new ConcreteGenericServiceType(1, "TestService", "Description");

        // Act
        var result = serviceType.CreateFactory();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(_factoryMock.Object);
        
        _output.WriteLine($"Factory creation via override succeeded: {result.GetType().Name}");
    }

    [Fact]
    public void GenericCreateWithTypedConfigurationShouldCallFactory()
    {
        // Arrange
        var expectedService = _serviceMock.Object;
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        _factoryMock.Setup(f => f.Create(_configMock.Object))
            .Returns(FdwResult<ITestService>.Success(expectedService));
        
        var serviceType = new ConcreteGenericServiceType(1, "TestService", "Description");

        // Act
        var result = serviceType.Create(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedService);
        
        _factoryMock.Verify(f => f.Create(_configMock.Object), Times.Once);
        _output.WriteLine($"Typed configuration service creation succeeded");
    }

    [Fact]
    public void GenericIServiceFactoryCreateShouldHandleValidConfiguration()
    {
        // Arrange
        var expectedService = _serviceMock.Object;
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        _factoryMock.Setup(f => f.Create(_configMock.Object))
            .Returns(FdwResult<ITestService>.Success(expectedService));
        
        var serviceType = new ConcreteGenericServiceType(1, "TestService", "Description");
        var factory = serviceType as IServiceFactory<ITestService>;

        // Act
        var result = factory!.Create(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedService);
        
        _output.WriteLine($"IServiceFactory<T> create succeeded");
    }

    [Fact]
    public void GenericIServiceFactoryCreateWithInvalidConfigurationShouldReturnFailure()
    {
        // Arrange
        var invalidConfig = Mock.Of<IFdwConfiguration>();
        var serviceType = new ConcreteGenericServiceType(1, "TestService", "Description");
        var factory = serviceType as IServiceFactory<ITestService>;

        // Act
        var result = factory!.Create(invalidConfig);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Invalid command type.");
        
        _output.WriteLine($"Invalid configuration handled: {result.Message}");
    }

    [Fact]
    public void GenericCreateGenericWithMatchingTypeShouldReturnService()
    {
        // Arrange
        var expectedService = _serviceMock.Object;
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        _factoryMock.Setup(f => f.Create(_configMock.Object))
            .Returns(FdwResult<ITestService>.Success(expectedService));
        
        var serviceType = new ConcreteGenericServiceType(1, "TestService", "Description");

        // Act
        var result = serviceType.Create<ITestService>(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedService);
        
        _output.WriteLine($"Generic create with matching type succeeded");
    }

    [Fact]
    public void GenericCreateGenericWithDifferentTypeShouldReturnFailure()
    {
        // Arrange
        var serviceType = new ConcreteGenericServiceType(1, "TestService", "Description");
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());

        // Act
        var result = serviceType.Create<IFdwService>(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Invalid command type.");
        
        _output.WriteLine($"Different type handled: {result.Message}");
    }

    [Fact]
    public void GenericCreateIFdwServiceShouldReturnService()
    {
        // Arrange
        var expectedService = _serviceMock.Object;
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        _factoryMock.Setup(f => f.Create(_configMock.Object))
            .Returns(FdwResult<ITestService>.Success(expectedService));
        
        var serviceType = new ConcreteGenericServiceType(1, "TestService", "Description");

        // Act
        var result = serviceType.Create(_configMock.Object as IFdwConfiguration);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedService);
        
        _output.WriteLine($"IFdwService create succeeded");
    }

    [Fact]
    public void GenericCreateIFdwServiceWithFactoryFailureShouldReturnFailure()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        _factoryMock.Setup(f => f.Create(_configMock.Object))
            .Returns(FdwResult<ITestService>.Failure("Factory error"));
        
        var serviceType = new ConcreteGenericServiceType(1, "TestService", "Description");

        // Act
        var result = serviceType.Create(_configMock.Object as IFdwConfiguration);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Factory error");
        
        _output.WriteLine($"Factory failure handled: {result.Message}");
    }

    [Fact]
    public async Task GenericGetServiceByNameShouldCallFactory()
    {
        // Arrange
        var configurationName = "test-config";
        var expectedService = _serviceMock.Object;
        _factoryMock.Setup(f => f.GetService(configurationName))
            .Returns(Task.FromResult(expectedService));
        
        var serviceType = new ConcreteGenericServiceType(1, "TestService", "Description");

        // Act
        var result = await serviceType.GetService(configurationName);

        // Assert
        result.ShouldBe(expectedService);
        _factoryMock.Verify(f => f.GetService(configurationName), Times.Once);
        
        _output.WriteLine($"GetService by name succeeded: {result.GetType().Name}");
    }

    [Fact]
    public async Task GenericGetServiceByIdShouldCallFactory()
    {
        // Arrange
        var configurationId = 123;
        var expectedService = _serviceMock.Object;
        _factoryMock.Setup(f => f.GetService(configurationId))
            .Returns(Task.FromResult(expectedService));
        
        var serviceType = new ConcreteGenericServiceType(1, "TestService", "Description");

        // Act
        var result = await serviceType.GetService(configurationId);

        // Assert
        result.ShouldBe(expectedService);
        _factoryMock.Verify(f => f.GetService(configurationId), Times.Once);
        
        _output.WriteLine($"GetService by ID succeeded: {result.GetType().Name}");
    }

    #endregion
}