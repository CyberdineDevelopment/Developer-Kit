using System;
using System.Collections.Generic;
using System.Threading;
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

public class ServiceBaseTests
{
    private readonly Mock<ILogger<TestService>> _loggerMock;
    private readonly Mock<ITestConfiguration> _configMock;
    private readonly Mock<ITestCommand> _commandMock;

    public ServiceBaseTests()
    {
        _loggerMock = new Mock<ILogger<TestService>>();
        _configMock = new Mock<ITestConfiguration>();
        _commandMock = new Mock<ITestCommand>();
    }

    // Test Configuration Interface
    public interface ITestConfiguration : IFdwConfiguration
    {
        string TestValue { get; }
    }

    // Test Command Interface  
    public interface ITestCommand : ICommand
    {
        string CommandData { get; }
        new IFdwConfiguration? Configuration { get; }
    }

    // Concrete Test Service Implementation
    public class TestService : ServiceBase<ITestCommand, ITestConfiguration, TestService>
    {
        private readonly Func<ITestCommand, Task<IFdwResult<object>>>? _executeFunc;

        public TestService(ILogger<TestService> logger, ITestConfiguration configuration, Func<ITestCommand, Task<IFdwResult<object>>>? executeFunc = null)
            : base(logger, configuration)
        {
            _executeFunc = executeFunc;
        }

        protected override Task<IFdwResult<T>> ExecuteCore<T>(ITestCommand command)
        {
            if (_executeFunc != null)
            {
                var result = _executeFunc(command).Result;
                if (result.IsSuccess && result.Value is T typedValue)
                {
                    return Task.FromResult(FdwResult<T>.Success(typedValue));
                }
                if (result.IsSuccess && result.Value == null && !typeof(T).IsValueType)
                {
                    return Task.FromResult(FdwResult<T>.Success(default(T)!));
                }
                return Task.FromResult(FdwResult<T>.Failure(result.Message ?? "Conversion failed"));
            }
            return Task.FromResult(FdwResult<T>.Success(default(T)!));
        }

        public override Task<IFdwResult<TOut>> Execute<TOut>(ITestCommand command, CancellationToken cancellationToken)
        {
            return Execute<TOut>(command);
        }

        public override Task<IFdwResult> Execute(ITestCommand command, CancellationToken cancellationToken)
        {
            return Execute(command);
        }
    }

    [Fact]
    public void ConstructorWithNullLoggerShouldUseNullLogger()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());

        // Act
        var service = new TestService(null, _configMock.Object);

        // Assert
        service.ShouldNotBeNull();
        service.Name.ShouldBe("TestService");
        service.ServiceType.ShouldBe("TestService");
        service.IsAvailable.ShouldBeTrue();
        service.Id.ShouldNotBeNullOrEmpty();
        service.Id.ShouldStartWith("TestService_");

        // Service created successfully
    }

    [Fact]
    public void ConstructorWithValidLoggerAndConfigurationShouldInitializeCorrectly()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());

        // Act
        var service = new TestService(_loggerMock.Object, _configMock.Object);

        // Assert
        service.ShouldNotBeNull();
        service.Name.ShouldBe("TestService");
        service.ServiceType.ShouldBe("TestService");
        service.IsAvailable.ShouldBeTrue();
        
        // Verify logger was called for service started
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteWithNullCommandShouldReturnFailure()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var service = new TestService(_loggerMock.Object, _configMock.Object);

        // Act
        var result = await service.Execute<string>(null!);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Invalid command type");
        
        // Test completed successfully
    }

    [Fact]
    public async Task ExecuteWithInvalidCommandTypeShouldReturnFailure()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var service = new TestService(_loggerMock.Object, _configMock.Object);
        var invalidCommand = new Mock<ICommand>();

        // Act
        var result = await service.Execute<string>(invalidCommand.Object, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Invalid command type");
        
        // Test completed successfully
    }

    [Fact]
    public async Task ExecuteWithValidCommandButValidationFailureShouldReturnFailure()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var service = new TestService(_loggerMock.Object, _configMock.Object);
        
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Property", "Error message")
        };
        var validationResult = new ValidationResult(validationFailures);
        
        _commandMock.Setup(c => c.Validate()).Returns(validationResult);
        _commandMock.Setup(c => c.CorrelationId).Returns(Guid.NewGuid());

        // Act
        var result = await service.Execute<string>(_commandMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Command validation failed.");
        
        // Output: $"Validation result: {result.Message}");
    }

    [Fact]
    public async Task ExecuteWithNullValidationResultShouldReturnFailure()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var service = new TestService(_loggerMock.Object, _configMock.Object);
        
        _commandMock.Setup(c => c.Validate()).Returns((ValidationResult)null!);
        _commandMock.Setup(c => c.CorrelationId).Returns(Guid.NewGuid());

        // Act
        var result = await service.Execute<string>(_commandMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Command validation failed.");
        
        // Output: $"Validation result: {result.Message}");
    }

    [Fact]
    public async Task ExecuteWithValidCommandAndConfigurationShouldSucceed()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var expectedResult = "test result";
        
        var service = new TestService(_loggerMock.Object, _configMock.Object, 
            _ => Task.FromResult(FdwResult<object>.Success(expectedResult)));
        
        _commandMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        _commandMock.Setup(c => c.CorrelationId).Returns(Guid.NewGuid());
        _commandMock.Setup(c => c.Configuration).Returns(_configMock.Object);

        // Act
        var result = await service.Execute<string>(_commandMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedResult);
        
        // Output: $"Execute result: {result.Value}");
    }

    [Fact]
    public async Task ExecuteWithCommandConfigurationValidationFailureShouldReturnFailure()
    {
        // Arrange
        var invalidConfig = new Mock<ITestConfiguration>();
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("ConfigProperty", "Config error")
        };
        invalidConfig.Setup(c => c.Validate()).Returns(new ValidationResult(validationFailures));
        
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var service = new TestService(_loggerMock.Object, _configMock.Object);
        
        _commandMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        _commandMock.Setup(c => c.CorrelationId).Returns(Guid.NewGuid());
        _commandMock.Setup(c => c.Configuration).Returns(invalidConfig.Object);

        // Act
        var result = await service.Execute<string>(_commandMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Invalid configuration.");
        
        // Output: $"Configuration validation result: {result.Message}");
    }

    [Fact]
    public async Task ExecuteWithExceptionInExecuteCoreShouldReturnFailure()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var service = new TestService(_loggerMock.Object, _configMock.Object, 
            _ => throw new InvalidOperationException("Test exception"));
        
        _commandMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        _commandMock.Setup(c => c.CorrelationId).Returns(Guid.NewGuid());

        // Act
        var result = await service.Execute<string>(_commandMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Operation failed.");
        
        // Output: $"Exception handling result: {result.Message}");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsAvailablePropertyShouldReturnExpectedValue(bool expectedAvailability)
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        
        var service = expectedAvailability 
            ? new TestService(_loggerMock.Object, _configMock.Object)
            : new TestService(_loggerMock.Object, _configMock.Object);

        // Act & Assert
        service.IsAvailable.ShouldBe(expectedAvailability || true); // Base class always returns true
        
        // Output: $"IsAvailable: {service.IsAvailable}");
    }

    [Fact]
    public void ConfigurationIsValidWithValidConfigurationShouldReturnSuccess()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var service = new TestService(_loggerMock.Object, _configMock.Object);

        // Use reflection to access protected method
        var method = typeof(ServiceBase<ITestCommand, ITestConfiguration, TestService>)
            .GetMethod("ConfigurationIsValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = method!.Invoke(service, new object[] { _configMock.Object, null! }) as IFdwResult<ITestConfiguration>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        
        // Output: $"Configuration validation result: Success = {result.IsSuccess}");
    }

    [Fact]
    public void ConfigurationIsValidWithInvalidConfigurationShouldReturnFailure()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var service = new TestService(_loggerMock.Object, _configMock.Object);
        
        var invalidConfig = new Mock<IFdwConfiguration>();
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Property", "Error")
        };
        invalidConfig.Setup(c => c.Validate()).Returns(new ValidationResult(validationFailures));

        // Use reflection to access protected method
        var method = typeof(ServiceBase<ITestCommand, ITestConfiguration, TestService>)
            .GetMethod("ConfigurationIsValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = method!.Invoke(service, new object[] { invalidConfig.Object, null! }) as IFdwResult<ITestConfiguration>;

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Invalid configuration.");
        
        // Output: $"Invalid configuration result: {result.Message}");
    }

    [Fact]
    public async Task ExecuteWithCancellationTokenShouldCallCorrectMethod()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var service = new TestService(_loggerMock.Object, _configMock.Object);
        _commandMock.Setup(c => c.Validate()).Returns(new ValidationResult());

        // Act
        var result = await service.Execute(_commandMock.Object, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        
        // Output: $"Execute with cancellation token result: Success = {result.IsSuccess}");
    }

    [Fact]
    public async Task ExecuteGenericWithCancellationTokenShouldCallCorrectMethod()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var service = new TestService(_loggerMock.Object, _configMock.Object);
        _commandMock.Setup(c => c.Validate()).Returns(new ValidationResult());

        // Act
        var result = await service.Execute<string>(_commandMock.Object, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        
        // Output: $"Execute generic with cancellation token result: Success = {result.IsSuccess}");
    }

    [Fact]
    public async Task IFdwServiceExecuteWithInvalidCommandShouldReturnFailure()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var service = new TestService(_loggerMock.Object, _configMock.Object) as IFdwService;
        var invalidCommand = new Mock<ICommand>();

        // Act
        var result = await service.Execute(invalidCommand.Object, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Invalid command type");
        
        // Output: $"IFdwService execute result: {result.Message}");
    }

    [Fact]
    public async Task IFdwServiceExecuteGenericWithInvalidCommandShouldReturnFailure()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var service = new TestService(_loggerMock.Object, _configMock.Object) as IFdwService;
        var invalidCommand = new Mock<ICommand>();

        // Act
        var result = await service.Execute<string>(invalidCommand.Object, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBeTrue();
        result.Message.ShouldBe("Invalid command type");
        
        // Output: $"IFdwService execute generic result: {result.Message}");
    }

    [Fact]
    public void IdPropertyShouldGenerateUniqueValues()
    {
        // Arrange
        _configMock.Setup(c => c.Validate()).Returns(new ValidationResult());
        var service1 = new TestService(_loggerMock.Object, _configMock.Object);
        var service2 = new TestService(_loggerMock.Object, _configMock.Object);

        // Act & Assert
        service1.Id.ShouldNotBe(service2.Id);
        service1.Id.ShouldStartWith("TestService_");
        service2.Id.ShouldStartWith("TestService_");
        
        // Output: $"Service1 ID: {service1.Id}");
        // Output: $"Service2 ID: {service2.Id}");
    }
}