using System;
using System.Threading.Tasks;
using FluentValidation.Results;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

public class ServiceFactoryBaseTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<ITestConfiguration> _configMock;
    private readonly Mock<ITestService> _serviceMock;

    public ServiceFactoryBaseTests()
    {
        _loggerMock = new Mock<ILogger>();
        _configMock = new Mock<ITestConfiguration>();
        _serviceMock = new Mock<ITestService>();
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

    // Concrete Test Factory Implementation
    public class TestServiceFactory : ServiceFactoryBase<ITestService, ITestConfiguration>
    {
        private readonly Func<ITestConfiguration, IFdwResult<ITestService>>? _createFunc;

        public TestServiceFactory(ILogger? logger = null, Func<ITestConfiguration, IFdwResult<ITestService>>? createFunc = null)
            : base(logger)
        {
            _createFunc = createFunc;
        }

        protected override IFdwResult<ITestService> CreateCore(ITestConfiguration configuration)
        {
            return _createFunc?.Invoke(configuration) ?? base.CreateCore(configuration);
        }

        public IFdwResult<ITestService> TestCreateCore(ITestConfiguration configuration)
        {
            return CreateCore(configuration);
        }

        public override Task<ITestService> GetService(string configurationName)
        {
            return Task.FromResult(Mock.Of<ITestService>());
        }

        public override Task<ITestService> GetService(int configurationId)
        {
            return Task.FromResult(Mock.Of<ITestService>());
        }
    }

    [Fact]
    public void ConstructorWithNullLoggerShouldUseNullLogger()
    {
        // Arrange & Act
        var factory = new TestServiceFactory();

        // Assert
        factory.ShouldNotBeNull();
        
        // Output: "Factory created successfully with null logger");
    }

    [Fact]
    public void ConstructorWithValidLoggerShouldUseProvidedLogger()
    {
        // Arrange & Act
        var factory = new TestServiceFactory(_loggerMock.Object);

        // Assert
        factory.ShouldNotBeNull();
        
        // Output: "Factory created successfully with provided logger");
    }

    [Fact]
    public void CreateCoreWithValidConfigurationShouldCreateService()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var expectedService = _serviceMock.Object;
        
        var factory = new TestServiceFactory(_loggerMock.Object, 
            config => FdwResult<ITestService>.Success(expectedService));

        // Act
        var result = factory.TestCreateCore(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedService);
        
        // Output: $"Service created successfully: {result.Value?.GetType().Name}");
    }

    [Fact]
    public void CreateCoreWithExceptionShouldReturnFailure()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        
        var factory = new TestServiceFactory(_loggerMock.Object, 
            config => throw new InvalidOperationException("Test exception"));

        // Act
        var result = factory.TestCreateCore(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldStartWith("Failed to create service:");
        
        // Output: $"Exception handled correctly: {result.Message}");
    }

    [Fact]
    public void ValidateConfigurationWithNullConfigurationShouldReturnFailure()
    {
        // Arrange
        var factory = new TestServiceFactory(_loggerMock.Object);

        // Use reflection to access protected method
        var method = typeof(ServiceFactoryBase<ITestService, ITestConfiguration>)
            .GetMethod("ValidateConfiguration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var parameters = new object[] { null, null! };
        var result = method!.Invoke(factory, parameters) as IFdwResult<ITestConfiguration>;

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Configuration cannot be null");
        
        // Output: $"Null configuration validation: {result.Message}");
    }

    [Fact]
    public void ValidateConfigurationWithValidConfigurationShouldReturnSuccess()
    {
        // Arrange
        var factory = new TestServiceFactory(_loggerMock.Object);
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());

        // Use reflection to access protected method
        var method = typeof(ServiceFactoryBase<ITestService, ITestConfiguration>)
            .GetMethod("ValidateConfiguration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var parameters = new object[] { _configMock.Object, null! };
        var result = method!.Invoke(factory, parameters) as IFdwResult<ITestConfiguration>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(_configMock.Object);
        
        // Output: $"Valid configuration validation: Success = {result.IsSuccess}");
    }

    [Fact]
    public void ValidateConfigurationWithInvalidConfigurationTypeShouldReturnFailure()
    {
        // Arrange
        var factory = new TestServiceFactory(_loggerMock.Object);
        var invalidConfig = new Mock<IFdwConfiguration>();
        invalidConfig.Setup(c => c.Validate()).Returns(new ValidationResult());

        // Use reflection to access protected method
        var method = typeof(ServiceFactoryBase<ITestService, ITestConfiguration>)
            .GetMethod("ValidateConfiguration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var parameters = new object[] { invalidConfig.Object, null! };
        var result = method!.Invoke(factory, parameters) as IFdwResult<ITestConfiguration>;

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Invalid configuration type");
        
        // Output: $"Invalid configuration type validation: {result.Message}");
    }

    [Fact]
    public void CreateGenericWithValidTypeShouldReturnService()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var expectedService = _serviceMock.Object;
        
        var factory = new TestServiceFactory(_loggerMock.Object, 
            config => FdwResult<ITestService>.Success(expectedService));

        // Act
        var result = factory.Create<ITestService>(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedService);
        
        // Output: $"Generic create succeeded: {result.Value?.GetType().Name}");
    }

    [Fact]
    public void CreateGenericWithInvalidTypeShouldReturnFailure()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var factory = new TestServiceFactory(_loggerMock.Object);

        // Act - Try to create with incompatible type
        var result = factory.Create<IFdwService>(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Invalid service type");
        
        // Output: $"Invalid service type creation: {result.Message}");
    }

    [Fact]
    public void IServiceFactoryCreateShouldReturnIFdwService()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var expectedService = _serviceMock.Object;
        
        var factory = new TestServiceFactory(_loggerMock.Object, 
            config => FdwResult<ITestService>.Success(expectedService)) as IServiceFactory;

        // Act
        var result = factory.Create(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedService);
        
        // Output: $"IServiceFactory create succeeded: {result.Value?.GetType().Name}");
    }

    [Fact]
    public void IServiceFactoryCreateWithInvalidServiceShouldReturnFailure()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var nonIFdwService = Mock.Of<ITestService>(); // This doesn't implement IFdwService directly
        
        var factory = new TestServiceFactory(_loggerMock.Object, 
            config => FdwResult<ITestService>.Success(nonIFdwService)) as IServiceFactory;

        // Act
        var result = factory.Create(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldContain("does not implement IFdwService");
        
        // Output: $"Non-IFdwService creation handled: {result.Message}");
    }

    [Fact]
    public void IServiceFactoryGenericCreateShouldUseStronglyTypedConfiguration()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var expectedService = _serviceMock.Object;
        
        var factory = new TestServiceFactory(_loggerMock.Object, 
            config => FdwResult<ITestService>.Success(expectedService)) as IServiceFactory<ITestService>;

        // Act
        var result = factory.Create(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedService);
        
        // Output: $"Strongly-typed factory create succeeded: {result.Value?.GetType().Name}");
    }

    [Fact]
    public void CreateWithStronglyTypedConfigurationShouldSucceed()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var expectedService = _serviceMock.Object;
        
        var factory = new TestServiceFactory(_loggerMock.Object, 
            config => FdwResult<ITestService>.Success(expectedService));

        // Act
        var result = factory.Create(_configMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedService);
        
        // Output: $"Strongly-typed create succeeded: {result.Value?.GetType().Name}");
    }

    [Fact]
    public void CreateWithNullStronglyTypedConfigurationShouldReturnFailure()
    {
        // Arrange
        var factory = new TestServiceFactory(_loggerMock.Object);

        // Act
        var result = factory.Create((ITestConfiguration)null!);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Configuration cannot be null");
        
        // Output: $"Null strongly-typed config handled: {result.Message}");
    }

    [Fact]
    public void CreateWithInvalidStronglyTypedConfigurationShouldReturnFailure()
    {
        // Arrange
        var factory = new TestServiceFactory(_loggerMock.Object);
        var invalidConfig = new Mock<ITestConfiguration>();
        invalidConfig.Setup(c => c.Validate()).Returns(new ValidationResult(new[] 
        { 
            new FluentValidation.Results.ValidationFailure("Property", "Error") 
        }));

        // Act
        var result = factory.Create(invalidConfig.Object);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Configuration validation failed");
        
        // Output: $"Invalid strongly-typed config handled: {result.Message}");
    }

    [Fact]
    public async Task GetServiceByNameShouldReturnService()
    {
        // Arrange
        var factory = new TestServiceFactory(_loggerMock.Object);
        var configurationName = "test-config";

        // Act
        var result = await factory.GetService(configurationName);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeAssignableTo<ITestService>();
        
        // Output: $"GetService by name succeeded: {result.GetType().Name}");
    }

    [Fact]
    public async Task GetServiceByIdShouldReturnService()
    {
        // Arrange
        var factory = new TestServiceFactory(_loggerMock.Object);
        var configurationId = 123;

        // Act
        var result = await factory.GetService(configurationId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeAssignableTo<ITestService>();
        
        // Output: $"GetService by ID succeeded: {result.GetType().Name}");
    }
}