using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

namespace FractalDataWorks.Services.Tests;

public sealed class ServiceBaseTests
{
    [Fact]
    public void ServiceBaseWhenConstructedWithNullLoggerShouldUseNullLogger()
    {
        // Arrange & Act
        var testService = new TestService(null, new TestConfiguration());

        // Assert
        testService.PublicLogger.ShouldNotBeNull();
        testService.Name.ShouldBe("TestService");
    }

    [Fact]
    public void ServiceBaseWhenConstructedWithLoggerShouldUseProvidedLogger()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TestService>>();
        var configuration = new TestConfiguration();

        // Act
        var testService = new TestService(mockLogger.Object, configuration);

        // Assert
        testService.PublicLogger.ShouldBe(mockLogger.Object);
        testService.Name.ShouldBe("TestService");
    }

    [Fact]
    public void IdShouldReturnUniqueIdentifierWithServiceTypeName()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());

        // Act
        var id = testService.Id;

        // Assert
        id.ShouldStartWith("TestService_");
        id.Length.ShouldBe(44); // "TestService_" (12 chars) + 32 character GUID (N format)
    }

    [Fact]
    public void ServiceTypeShouldReturnServiceTypeName()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());

        // Act
        var serviceType = testService.ServiceType;

        // Assert
        serviceType.ShouldBe("TestService");
    }

    [Fact]
    public void IsAvailableShouldReturnTrueByDefault()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());

        // Act
        var isAvailable = testService.IsAvailable;

        // Assert
        isAvailable.ShouldBeTrue();
    }

    [Fact]
    public void ConfigurationIsValidWhenValidConfigurationShouldReturnSuccess()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var validConfig = new TestConfiguration { IsValid = true };

        // Act
        var result = testService.TestConfigurationIsValid(validConfig, out var validConfiguration);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        validConfiguration.ShouldBe(validConfig);
    }

    [Fact]
    public void ConfigurationIsValidWhenInvalidConfigurationShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var invalidConfig = new TestConfiguration { IsValid = false };

        // Act
        var result = testService.TestConfigurationIsValid(invalidConfig, out var validConfiguration);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Invalid configuration.");
        validConfiguration.ShouldBeNull();
    }

    [Fact]
    public void ConfigurationIsValidWhenNullConfigurationShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());

        // Act
        var result = testService.TestConfigurationIsValid(null, out var validConfiguration);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Invalid configuration.");
        validConfiguration.ShouldBeNull();
    }

    [Fact]
    public void ConfigurationIsValidWhenWrongConfigurationTypeShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var wrongConfig = new WrongConfiguration();

        // Act
        var result = testService.TestConfigurationIsValid(wrongConfig, out var validConfiguration);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Invalid configuration.");
        validConfiguration.ShouldBeNull();
    }

    [Fact]
    public async Task ValidateCommandWhenValidCommandShouldReturnSuccess()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var validCommand = new TestCommand { IsValid = true };

        // Act
        var result = await testService.TestValidateCommand(validCommand);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(validCommand);
    }

    [Fact]
    public async Task ValidateCommandWhenInvalidCommandShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var invalidCommand = new TestCommand { IsValid = false };

        // Act
        var result = await testService.TestValidateCommand(invalidCommand);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Command validation failed.");
    }

    [Fact]
    public async Task ValidateCommandWhenWrongCommandTypeShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var wrongCommand = new WrongCommand();

        // Act
        var result = await testService.TestValidateCommand(wrongCommand);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Invalid command type.");
    }

    [Fact]
    public async Task ValidateCommandWhenCommandValidationReturnsNullShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var commandWithNullValidation = new TestCommandThatReturnsNull();

        // Act
        var result = await testService.TestValidateCommand(commandWithNullValidation);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Command validation failed.");
    }

    [Fact]
    public async Task ValidateCommandWhenCommandHasInvalidConfigurationShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var invalidConfig = new TestConfiguration { IsValid = false };
        var command = new TestCommand { IsValid = true, Configuration = invalidConfig };

        // Act
        var result = await testService.TestValidateCommand(command);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Invalid configuration.");
    }

    [Fact]
    public async Task ExecuteGenericWhenNullCommandShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());

        // Act
        var result = await testService.Execute<string>(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Invalid command type");
    }

    [Fact]
    public async Task ExecuteGenericWhenValidCommandShouldReturnSuccess()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var validCommand = new TestCommand { IsValid = true };
        var expectedResult = "test result";
        testService.SetExecuteResult(FdwResult<string>.Success(expectedResult));

        // Act
        var result = await testService.Execute<string>(validCommand);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task ExecuteGenericWhenCommandValidationFailsShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var invalidCommand = new TestCommand { IsValid = false };

        // Act
        var result = await testService.Execute<string>(invalidCommand);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Command validation failed.");
    }

    [Fact]
    public async Task ExecuteGenericWhenExecuteCoreFailsShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var validCommand = new TestCommand { IsValid = true };
        var failureMessage = "execution failed";
        testService.SetExecuteResult(FdwResult<string>.Failure(failureMessage));

        // Act
        var result = await testService.Execute<string>(validCommand);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe(failureMessage);
    }

    [Fact]
    public async Task ExecuteGenericWhenExceptionThrownShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var validCommand = new TestCommand { IsValid = true };
        testService.ThrowExceptionInExecuteCore = true;

        // Act
        var result = await testService.Execute<string>(validCommand);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Operation failed.");
    }

    [Fact]
    public async Task ExecuteGenericWhenOutOfMemoryExceptionThrownShouldRethrow()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var validCommand = new TestCommand { IsValid = true };
        testService.ThrowOutOfMemoryException = true;

        // Act & Assert
        await Should.ThrowAsync<OutOfMemoryException>(async () => 
            await testService.Execute<string>(validCommand));
    }

    [Fact]
    public async Task ExecuteNonGenericICommandWhenValidCommandShouldReturnSuccess()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var validCommand = new TestCommand { IsValid = true };
        testService.SetExecuteResultNonGeneric(FdwResult.Success());

        // Act  
        var result = await testService.Execute(validCommand as ICommand, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteNonGenericICommandWhenInvalidCommandTypeShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var wrongCommand = new WrongCommand();

        // Act
        var result = await testService.Execute(wrongCommand, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Invalid command type");
    }

    [Fact]
    public async Task ExecuteGenericICommandWhenValidCommandShouldReturnSuccess()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var validCommand = new TestCommand { IsValid = true };
        var expectedResult = "test result";
        testService.SetExecuteGenericResult(FdwResult<string>.Success(expectedResult));

        // Act
        var result = await testService.Execute<string>(validCommand as ICommand, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task ExecuteGenericICommandWhenInvalidCommandTypeShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var wrongCommand = new WrongCommand();

        // Act
        var result = await testService.Execute<string>(wrongCommand, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Invalid command type");
    }

    [Fact]
    public async Task ExecuteTypedCommandNonGenericShouldReturnSuccess()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var validCommand = new TestCommand { IsValid = true };
        testService.SetExecuteGenericResult(FdwResult<object>.Success("test"));

        // Act
        var result = await testService.Execute(validCommand);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteTypedCommandNonGenericWhenFailureShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var validCommand = new TestCommand { IsValid = true };
        var failureMessage = "execution failed";
        testService.SetExecuteGenericResult(FdwResult<object>.Failure(failureMessage));

        // Act
        var result = await testService.Execute(validCommand);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe(failureMessage);
    }

    [Theory]
    [InlineData("TestCommand")]
    [InlineData("AnotherTestCommand")]
    public async Task ExecuteGenericShouldHandleDifferentCommandTypes(string commandTypeName)
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var command = new TestCommand { IsValid = true, CommandTypeName = commandTypeName };
        testService.SetExecuteResult(FdwResult<string>.Success("test"));

        // Act
        var result = await testService.Execute<string>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ServiceBaseShouldUseLoggingScopeWithCorrelationId()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TestService>>();
        var testService = new TestService(mockLogger.Object, new TestConfiguration());
        var correlationId = Guid.NewGuid();
        var command = new TestCommand { IsValid = true, CorrelationId = correlationId };
        testService.SetExecuteResult(FdwResult<string>.Success("test"));

        // Act
        await testService.Execute<string>(command);

        // Assert
        // Verify that BeginScope was called with a dictionary containing the correlation ID
        mockLogger.Verify(
            x => x.BeginScope(It.Is<Dictionary<string, object>>(
                d => d.ContainsKey("CorrelationId") && d["CorrelationId"].Equals(correlationId))),
            Times.Once);
    }

    [Fact]
    public void IdShouldGenerateUniqueIdentifiersForDifferentInstances()
    {
        // Arrange & Act
        var service1 = new TestService(null, new TestConfiguration());
        var service2 = new TestService(null, new TestConfiguration());

        // Assert
        service1.Id.ShouldNotBe(service2.Id);
    }

    [Fact]
    public async Task ExecuteGenericShouldMeasureExecutionTime()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var validCommand = new TestCommand { IsValid = true };
        testService.SetExecuteResult(FdwResult<string>.Success("test"));
        testService.ExecutionDelay = TimeSpan.FromMilliseconds(50);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await testService.Execute<string>(validCommand);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.ShouldBeGreaterThan(40);
    }

    [Fact]
    public async Task ExecuteGenericWhenCommandConfigurationValidationFailsShouldReturnFailure()
    {
        // Arrange
        var testService = new TestService(null, new TestConfiguration());
        var invalidConfig = new TestConfiguration { IsValid = false };
        var command = new TestCommand 
        { 
            IsValid = true, 
            Configuration = invalidConfig 
        };

        // Act
        var result = await testService.Execute<string>(command);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Invalid configuration.");
    }
}

// Test helper classes
public sealed class TestService : ServiceBase<TestCommand, TestConfiguration, TestService>
{
    private object? _executeResult;
    private object? _executeGenericResult;
    private IFdwResult? _executeNonGenericResult;

    public bool ThrowExceptionInExecuteCore { get; set; }
    public bool ThrowOutOfMemoryException { get; set; }
    public TimeSpan ExecutionDelay { get; set; } = TimeSpan.Zero;

    public TestService(ILogger<TestService>? logger, TestConfiguration configuration) 
        : base(logger, configuration)
    {
    }

    public ILogger<TestService> PublicLogger => Logger;

    public void SetExecuteResult<T>(IFdwResult<T> result)
    {
        _executeResult = result;
    }

    public void SetExecuteGenericResult<T>(IFdwResult<T> result)
    {
        _executeGenericResult = result;
    }

    public void SetExecuteResultNonGeneric(IFdwResult result)
    {
        _executeNonGenericResult = result;
    }

    public IFdwResult<TestConfiguration> TestConfigurationIsValid(
        IFdwConfiguration? configuration, 
        out TestConfiguration? validConfiguration)
    {
        return ConfigurationIsValid(configuration!, out validConfiguration);
    }

    public Task<IFdwResult<TestCommand>> TestValidateCommand(ICommand command)
    {
        return ValidateCommand(command);
    }

    protected override async Task<IFdwResult<T>> ExecuteCore<T>(TestCommand command)
    {
        if (ExecutionDelay > TimeSpan.Zero)
        {
            await Task.Delay(ExecutionDelay);
        }

        if (ThrowOutOfMemoryException)
        {
            throw new OutOfMemoryException("Test OOM exception");
        }

        if (ThrowExceptionInExecuteCore)
        {
            throw new InvalidOperationException("Test exception");
        }

        if (_executeResult != null)
        {
            return (IFdwResult<T>)_executeResult;
        }

        return FdwResult<T>.Success(default!);
    }

    public override Task<IFdwResult<TOut>> Execute<TOut>(TestCommand command, CancellationToken cancellationToken)
    {
        if (_executeGenericResult != null)
        {
            return Task.FromResult((IFdwResult<TOut>)_executeGenericResult);
        }

        return Task.FromResult(FdwResult<TOut>.Success(default!));
    }

    public override Task<IFdwResult> Execute(TestCommand command, CancellationToken cancellationToken)
    {
        if (_executeNonGenericResult != null)
        {
            return Task.FromResult(_executeNonGenericResult);
        }

        return Task.FromResult(FdwResult.Success());
    }
}

public sealed class TestConfiguration : IFdwConfiguration
{
    public bool IsValid { get; set; } = true;
    public FluentValidation.Results.ValidationResult? ValidationResult { get; set; }
    public string SectionName => "TestConfiguration";

    public FluentValidation.Results.ValidationResult Validate()
    {
        if (ValidationResult != null)
            return ValidationResult;

        var result = new FluentValidation.Results.ValidationResult();
        if (!IsValid)
        {
            result.Errors.Add(new FluentValidation.Results.ValidationFailure("Test", "Test validation failure"));
        }
        return result;
    }
}

public sealed class WrongConfiguration : IFdwConfiguration
{
    public string SectionName => "WrongConfiguration";

    public FluentValidation.Results.ValidationResult Validate()
    {
        return new FluentValidation.Results.ValidationResult();
    }
}

public class TestCommand : ICommand
{
    public bool IsValid { get; set; } = true;
    public string CommandTypeName { get; set; } = "TestCommand";
    public Guid CommandId { get; set; } = Guid.NewGuid();
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public IFdwConfiguration? Configuration { get; set; }
    public FluentValidation.Results.ValidationResult? ValidationResult { get; set; }

    public new Type GetType() => typeof(TestCommand);

    public virtual FluentValidation.Results.ValidationResult Validate()
    {
        if (ValidationResult != null)
            return ValidationResult;

        var result = new FluentValidation.Results.ValidationResult();
        if (!IsValid)
        {
            result.Errors.Add(new FluentValidation.Results.ValidationFailure("Command", "Command validation failure"));
        }
        return result;
    }
}

public sealed class WrongCommand : ICommand
{
    public Guid CommandId { get; set; } = Guid.NewGuid();
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public IFdwConfiguration? Configuration { get; set; }

    public new Type GetType() => typeof(WrongCommand);

    public FluentValidation.Results.ValidationResult Validate()
    {
        return new FluentValidation.Results.ValidationResult();
    }
}

public sealed class TestCommandThatReturnsNull : TestCommand
{
    public override FluentValidation.Results.ValidationResult Validate()
    {
        return null!; // This is unsafe but needed to test the null validation scenario
    }
}