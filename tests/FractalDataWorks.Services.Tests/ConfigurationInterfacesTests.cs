using FractalDataWorks.Configuration;
using FractalDataWorks.Services.Configuration;
using FluentValidation.Results;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

public class ConfigurationInterfacesTests
{
    private readonly ITestOutputHelper _output;

    public ConfigurationInterfacesTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region IRetryPolicyConfiguration Tests

    // Mock implementation of IRetryPolicyConfiguration
    public class MockRetryPolicyConfiguration : IRetryPolicyConfiguration
    {
        public int MaxRetries { get; set; } = 3;
        public int InitialDelayMs { get; set; } = 1000;
        public double BackoffMultiplier { get; set; } = 2.0;
        public int MaxDelayMs { get; set; } = 30000;
    }

    [Fact]
    public void IRetryPolicyConfigurationPropertiesShouldBeAccessible()
    {
        // Arrange
        var retryConfig = new MockRetryPolicyConfiguration
        {
            MaxRetries = 5,
            InitialDelayMs = 500,
            BackoffMultiplier = 1.5,
            MaxDelayMs = 60000
        };

        // Act & Assert
        retryConfig.MaxRetries.ShouldBe(5);
        retryConfig.InitialDelayMs.ShouldBe(500);
        retryConfig.BackoffMultiplier.ShouldBe(1.5);
        retryConfig.MaxDelayMs.ShouldBe(60000);
        
        _output.WriteLine($"Retry Policy - MaxRetries: {retryConfig.MaxRetries}, InitialDelay: {retryConfig.InitialDelayMs}ms");
        _output.WriteLine($"BackoffMultiplier: {retryConfig.BackoffMultiplier}, MaxDelay: {retryConfig.MaxDelayMs}ms");
    }

    [Theory]
    [InlineData(0, 100, 1.0, 1000)]
    [InlineData(1, 500, 2.0, 5000)]
    [InlineData(10, 2000, 3.5, 120000)]
    [InlineData(-1, 0, 0.5, 100)] // Edge case with invalid values
    public void IRetryPolicyConfigurationShouldSupportVariousValues(int maxRetries, int initialDelay, double backoffMultiplier, int maxDelay)
    {
        // Arrange
        var retryConfig = new MockRetryPolicyConfiguration
        {
            MaxRetries = maxRetries,
            InitialDelayMs = initialDelay,
            BackoffMultiplier = backoffMultiplier,
            MaxDelayMs = maxDelay
        };

        // Act & Assert
        retryConfig.MaxRetries.ShouldBe(maxRetries);
        retryConfig.InitialDelayMs.ShouldBe(initialDelay);
        retryConfig.BackoffMultiplier.ShouldBe(backoffMultiplier);
        retryConfig.MaxDelayMs.ShouldBe(maxDelay);
        
        _output.WriteLine($"Test values - MaxRetries: {maxRetries}, InitialDelay: {initialDelay}ms, " +
                         $"Backoff: {backoffMultiplier}, MaxDelay: {maxDelay}ms");
    }

    [Fact]
    public void IRetryPolicyConfigurationMockShouldWork()
    {
        // Arrange
        var mockRetryConfig = new Mock<IRetryPolicyConfiguration>();
        mockRetryConfig.Setup(r => r.MaxRetries).Returns(7);
        mockRetryConfig.Setup(r => r.InitialDelayMs).Returns(750);
        mockRetryConfig.Setup(r => r.BackoffMultiplier).Returns(2.5);
        mockRetryConfig.Setup(r => r.MaxDelayMs).Returns(45000);

        // Act
        var config = mockRetryConfig.Object;

        // Assert
        config.MaxRetries.ShouldBe(7);
        config.InitialDelayMs.ShouldBe(750);
        config.BackoffMultiplier.ShouldBe(2.5);
        config.MaxDelayMs.ShouldBe(45000);
        
        mockRetryConfig.Verify(r => r.MaxRetries, Times.Once);
        mockRetryConfig.Verify(r => r.InitialDelayMs, Times.Once);
        mockRetryConfig.Verify(r => r.BackoffMultiplier, Times.Once);
        mockRetryConfig.Verify(r => r.MaxDelayMs, Times.Once);
        
        _output.WriteLine($"Mock retry config verified - MaxRetries: {config.MaxRetries}");
    }

    #endregion

    #region IServiceConfiguration Tests

    // Mock implementation of IServiceConfiguration
    public class MockServiceConfiguration : IServiceConfiguration
    {
        public string ServiceType { get; set; } = "MockService";
        public IRetryPolicyConfiguration? RetryPolicy { get; set; }
        public int TimeoutMs { get; set; } = 30000;
        public string SectionName => "MockServiceConfiguration";

        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            
            if (string.IsNullOrEmpty(ServiceType))
                result.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(ServiceType), "ServiceType cannot be empty"));
                
            if (TimeoutMs <= 0)
                result.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(TimeoutMs), "TimeoutMs must be positive"));
                
            return result;
        }
    }

    [Fact]
    public void IServiceConfigurationPropertiesShouldBeAccessible()
    {
        // Arrange
        var retryPolicy = new MockRetryPolicyConfiguration();
        var serviceConfig = new MockServiceConfiguration
        {
            ServiceType = "TestService",
            RetryPolicy = retryPolicy,
            TimeoutMs = 15000
        };

        // Act & Assert
        serviceConfig.ServiceType.ShouldBe("TestService");
        serviceConfig.RetryPolicy.ShouldBe(retryPolicy);
        serviceConfig.TimeoutMs.ShouldBe(15000);
        
        // Should implement IFdwConfiguration
        serviceConfig.ShouldBeAssignableTo<IFdwConfiguration>();
        
        _output.WriteLine($"Service Config - Type: {serviceConfig.ServiceType}, Timeout: {serviceConfig.TimeoutMs}ms");
        _output.WriteLine($"Has Retry Policy: {serviceConfig.RetryPolicy != null}");
    }

    [Fact]
    public void IServiceConfigurationValidationShouldWork()
    {
        // Arrange
        var validConfig = new MockServiceConfiguration
        {
            ServiceType = "ValidService",
            TimeoutMs = 5000
        };
        
        var invalidConfig = new MockServiceConfiguration
        {
            ServiceType = "", // Invalid - empty
            TimeoutMs = -1000 // Invalid - negative
        };

        // Act
        var validResult = validConfig.Validate();
        var invalidResult = invalidConfig.Validate();

        // Assert
        validResult.IsValid.ShouldBeTrue();
        validResult.Errors.Count.ShouldBe(0);
        
        invalidResult.IsValid.ShouldBeFalse();
        invalidResult.Errors.Count.ShouldBe(2);
        invalidResult.Errors.ShouldContain(e => e.PropertyName == nameof(MockServiceConfiguration.ServiceType));
        invalidResult.Errors.ShouldContain(e => e.PropertyName == nameof(MockServiceConfiguration.TimeoutMs));
        
        _output.WriteLine($"Valid config validation: {validResult.IsValid} (errors: {validResult.Errors.Count})");
        _output.WriteLine($"Invalid config validation: {invalidResult.IsValid} (errors: {invalidResult.Errors.Count})");
        
        foreach (var error in invalidResult.Errors)
        {
            _output.WriteLine($"  - {error.PropertyName}: {error.ErrorMessage}");
        }
    }

    [Fact]
    public void IServiceConfigurationWithNullRetryPolicyShouldWork()
    {
        // Arrange
        var serviceConfig = new MockServiceConfiguration
        {
            ServiceType = "ServiceWithoutRetry",
            RetryPolicy = null,
            TimeoutMs = 10000
        };

        // Act & Assert
        serviceConfig.ServiceType.ShouldBe("ServiceWithoutRetry");
        serviceConfig.RetryPolicy.ShouldBeNull();
        serviceConfig.TimeoutMs.ShouldBe(10000);
        
        var validationResult = serviceConfig.Validate();
        validationResult.IsValid.ShouldBeTrue();
        
        _output.WriteLine($"Service config without retry policy is valid: {validationResult.IsValid}");
    }

    [Fact]
    public void IServiceConfigurationMockShouldWork()
    {
        // Arrange
        var mockRetryPolicy = new Mock<IRetryPolicyConfiguration>();
        var mockServiceConfig = new Mock<IServiceConfiguration>();
        
        mockServiceConfig.Setup(s => s.ServiceType).Returns("MockedService");
        mockServiceConfig.Setup(s => s.RetryPolicy).Returns(mockRetryPolicy.Object);
        mockServiceConfig.Setup(s => s.TimeoutMs).Returns(25000);
        mockServiceConfig.Setup(s => s.Validate()).Returns(new ValidationResult());

        // Act
        var config = mockServiceConfig.Object;

        // Assert
        config.ServiceType.ShouldBe("MockedService");
        config.RetryPolicy.ShouldBe(mockRetryPolicy.Object);
        config.TimeoutMs.ShouldBe(25000);
        config.Validate().IsValid.ShouldBeTrue();
        
        mockServiceConfig.Verify(s => s.ServiceType, Times.Once);
        mockServiceConfig.Verify(s => s.RetryPolicy, Times.Once);
        mockServiceConfig.Verify(s => s.TimeoutMs, Times.Once);
        mockServiceConfig.Verify(s => s.Validate(), Times.Once);
        
        _output.WriteLine($"Mock service config verified - Type: {config.ServiceType}");
    }

    [Theory]
    [InlineData("ConnectionService", 5000)]
    [InlineData("DataProviderService", 30000)]
    [InlineData("TransformationService", 60000)]
    [InlineData("", 1000)] // Edge case
    public void IServiceConfigurationShouldSupportVariousServiceTypes(string serviceType, int timeoutMs)
    {
        // Arrange
        var config = new MockServiceConfiguration
        {
            ServiceType = serviceType,
            TimeoutMs = timeoutMs
        };

        // Act & Assert
        config.ServiceType.ShouldBe(serviceType);
        config.TimeoutMs.ShouldBe(timeoutMs);
        
        // Validation should reflect whether the values are valid
        var validationResult = config.Validate();
        var shouldBeValid = !string.IsNullOrEmpty(serviceType) && timeoutMs > 0;
        validationResult.IsValid.ShouldBe(shouldBeValid);
        
        _output.WriteLine($"Service '{serviceType}' with timeout {timeoutMs}ms is valid: {validationResult.IsValid}");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ServiceConfigurationWithRetryPolicyIntegrationShouldWork()
    {
        // Arrange
        var retryPolicy = new MockRetryPolicyConfiguration
        {
            MaxRetries = 3,
            InitialDelayMs = 1000,
            BackoffMultiplier = 2.0,
            MaxDelayMs = 10000
        };
        
        var serviceConfig = new MockServiceConfiguration
        {
            ServiceType = "IntegratedService",
            RetryPolicy = retryPolicy,
            TimeoutMs = 20000
        };

        // Act
        var validationResult = serviceConfig.Validate();

        // Assert
        validationResult.IsValid.ShouldBeTrue();
        serviceConfig.RetryPolicy.ShouldNotBeNull();
        serviceConfig.RetryPolicy!.MaxRetries.ShouldBe(3);
        serviceConfig.RetryPolicy.InitialDelayMs.ShouldBe(1000);
        serviceConfig.RetryPolicy.BackoffMultiplier.ShouldBe(2.0);
        serviceConfig.RetryPolicy.MaxDelayMs.ShouldBe(10000);
        
        _output.WriteLine($"Integration test - Service: {serviceConfig.ServiceType}");
        _output.WriteLine($"Timeout: {serviceConfig.TimeoutMs}ms, Max Retries: {serviceConfig.RetryPolicy.MaxRetries}");
        _output.WriteLine($"Retry Initial Delay: {serviceConfig.RetryPolicy.InitialDelayMs}ms");
    }

    [Fact]
    public void InterfaceInheritanceShouldWorkCorrectly()
    {
        // Arrange
        var serviceConfig = new MockServiceConfiguration();

        // Act & Assert
        // IServiceConfiguration should inherit from IFdwConfiguration
        serviceConfig.ShouldBeAssignableTo<IServiceConfiguration>();
        serviceConfig.ShouldBeAssignableTo<IFdwConfiguration>();
        
        // Should have all expected members
        var serviceConfigInterface = typeof(IServiceConfiguration);
        var fdwConfigInterface = typeof(IFdwConfiguration);
        
        serviceConfigInterface.IsAssignableFrom(typeof(MockServiceConfiguration)).ShouldBeTrue();
        fdwConfigInterface.IsAssignableFrom(typeof(MockServiceConfiguration)).ShouldBeTrue();
        
        // Should be able to call inherited methods
        var validationResult = ((IFdwConfiguration)serviceConfig).Validate();
        validationResult.ShouldNotBeNull();
        
        _output.WriteLine($"Interface inheritance verified:");
        _output.WriteLine($"  - IServiceConfiguration assignable: {serviceConfig is IServiceConfiguration}");
        _output.WriteLine($"  - IFdwConfiguration assignable: {serviceConfig is IFdwConfiguration}");
        _output.WriteLine($"  - Validation method accessible: {validationResult != null}");
    }

    #endregion
}