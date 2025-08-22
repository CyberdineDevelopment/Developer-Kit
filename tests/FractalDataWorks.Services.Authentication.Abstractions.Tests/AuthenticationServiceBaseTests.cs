using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using FractalDataWorks.Services.Authentication.Abstractions;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.Abstractions.Tests;

/// <summary>
/// Tests for AuthenticationServiceBase abstract class.
/// </summary>
public sealed class AuthenticationServiceBaseTests
{
    /// <summary>
    /// Mock authentication command for testing.
    /// </summary>
    private sealed class MockAuthenticationCommand : IAuthenticationCommand
    {
        public Guid CommandId { get; set; } = Guid.NewGuid();
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public IFdwConfiguration? Configuration { get; set; }
        
        public ValidationResult Validate() => new();
    }

    /// <summary>
    /// Mock authentication configuration for testing.
    /// </summary>
    private sealed class MockAuthenticationConfiguration : IAuthenticationConfiguration
    {
        public string SectionName { get; set; } = "MockAuth";
        public string ClientId { get; set; } = "mock-client-id";
        public string Authority { get; set; } = "https://mock.authority.com";
        public string RedirectUri { get; set; } = "https://mock.app.com/callback";
        public string[] Scopes { get; set; } = ["openid", "profile"];
        public bool EnableTokenCaching { get; set; } = true;
        public int TokenCacheLifetimeMinutes { get; set; } = 30;
        
        public ValidationResult Validate() => new();
    }

    /// <summary>
    /// Concrete implementation of AuthenticationServiceBase for testing.
    /// </summary>
    private sealed class MockAuthenticationService 
        : AuthenticationServiceBase<MockAuthenticationCommand, MockAuthenticationConfiguration, MockAuthenticationService>
    {
        public MockAuthenticationService(ILogger<MockAuthenticationService> logger, MockAuthenticationConfiguration configuration)
            : base(logger, configuration)
        {
            _testConfiguration = configuration;
        }

        // Store configuration for testing access
        private readonly MockAuthenticationConfiguration _testConfiguration;
        
        public MockAuthenticationConfiguration TestConfiguration => _testConfiguration;
        
        // Expose protected Logger property for testing
        public ILogger<MockAuthenticationService> TestLogger => Logger;

        // Abstract method implementations required by ServiceBase
        protected override Task<IFdwResult<T>> ExecuteCore<T>(MockAuthenticationCommand command)
        {
            var result = FdwResult<T>.Success(default(T)!);
            return Task.FromResult<IFdwResult<T>>(result);
        }

        public override Task<IFdwResult<TOut>> Execute<TOut>(MockAuthenticationCommand command, CancellationToken cancellationToken)
        {
            var result = FdwResult<TOut>.Success(default(TOut)!);
            return Task.FromResult<IFdwResult<TOut>>(result);
        }

        public override Task<IFdwResult> Execute(MockAuthenticationCommand command, CancellationToken cancellationToken)
        {
            var result = FdwResult.Success();
            return Task.FromResult<IFdwResult>(result);
        }
    }

    [Fact]
    public void ConstructorWithValidParametersCreatesInstance()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MockAuthenticationService>>();
        var configuration = new MockAuthenticationConfiguration();

        // Act
        var service = new MockAuthenticationService(mockLogger.Object, configuration);

        // Assert
        service.ShouldNotBeNull();
        service.TestConfiguration.ShouldBe(configuration);
        service.TestLogger.ShouldBe(mockLogger.Object);
    }

    [Fact]
    public void ConstructorWithNullLoggerThrowsArgumentNullException()
    {
        // Arrange
        var configuration = new MockAuthenticationConfiguration();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new MockAuthenticationService(null!, configuration))
            .ParamName.ShouldBe("logger");
    }

    [Fact]
    public void ConstructorWithNullConfigurationThrowsArgumentNullException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MockAuthenticationService>>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new MockAuthenticationService(mockLogger.Object, null!))
            .ParamName.ShouldBe("configuration");
    }

    [Fact]
    public void AuthenticationServiceShouldInheritFromServiceBase()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MockAuthenticationService>>();
        var configuration = new MockAuthenticationConfiguration();

        // Act
        var service = new MockAuthenticationService(mockLogger.Object, configuration);

        // Assert
        service.ShouldBeAssignableTo<ServiceBase<MockAuthenticationCommand, MockAuthenticationConfiguration, MockAuthenticationService>>();
    }

    [Fact]
    public void ConfigurationPropertyShouldReturnPassedConfiguration()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MockAuthenticationService>>();
        var configuration = new MockAuthenticationConfiguration
        {
            ClientId = "test-client-123",
            Authority = "https://test.authority.com",
            EnableTokenCaching = false
        };

        // Act
        var service = new MockAuthenticationService(mockLogger.Object, configuration);

        // Assert
        service.TestConfiguration.ClientId.ShouldBe("test-client-123");
        service.TestConfiguration.Authority.ShouldBe("https://test.authority.com");
        service.TestConfiguration.EnableTokenCaching.ShouldBeFalse();
    }

    [Fact]
    public void LoggerPropertyShouldReturnPassedLogger()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MockAuthenticationService>>();
        var configuration = new MockAuthenticationConfiguration();

        // Act
        var service = new MockAuthenticationService(mockLogger.Object, configuration);

        // Assert
        service.TestLogger.ShouldBeSameAs(mockLogger.Object);
    }

    [Fact]
    public void ServiceShouldAcceptCompleteConfiguration()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MockAuthenticationService>>();
        var configuration = new MockAuthenticationConfiguration
        {
            SectionName = "CompleteAuth",
            ClientId = "complete-client-id",
            Authority = "https://complete.authority.com/tenant",
            RedirectUri = "https://complete.app.com/auth/callback",
            Scopes = ["openid", "profile", "email", "user.read"],
            EnableTokenCaching = true,
            TokenCacheLifetimeMinutes = 60
        };

        // Act
        var service = new MockAuthenticationService(mockLogger.Object, configuration);

        // Assert
        service.TestConfiguration.SectionName.ShouldBe("CompleteAuth");
        service.TestConfiguration.ClientId.ShouldBe("complete-client-id");
        service.TestConfiguration.Authority.ShouldBe("https://complete.authority.com/tenant");
        service.TestConfiguration.RedirectUri.ShouldBe("https://complete.app.com/auth/callback");
        service.TestConfiguration.Scopes.ShouldBe(["openid", "profile", "email", "user.read"]);
        service.TestConfiguration.EnableTokenCaching.ShouldBeTrue();
        service.TestConfiguration.TokenCacheLifetimeMinutes.ShouldBe(60);
    }

    [Fact]
    public void ServiceShouldSupportMinimalConfiguration()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MockAuthenticationService>>();
        var configuration = new MockAuthenticationConfiguration
        {
            ClientId = "minimal-client",
            Authority = "https://minimal.com",
            RedirectUri = "https://app.com",
            Scopes = []
        };

        // Act
        var service = new MockAuthenticationService(mockLogger.Object, configuration);

        // Assert
        service.TestConfiguration.ClientId.ShouldBe("minimal-client");
        service.TestConfiguration.Authority.ShouldBe("https://minimal.com");
        service.TestConfiguration.RedirectUri.ShouldBe("https://app.com");
        service.TestConfiguration.Scopes.ShouldBeEmpty();
    }

    [Fact]
    public void ServiceShouldMaintainConfigurationStateAfterCreation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MockAuthenticationService>>();
        var configuration = new MockAuthenticationConfiguration
        {
            ClientId = "state-test-client",
            EnableTokenCaching = true,
            TokenCacheLifetimeMinutes = 45
        };

        // Act
        var service = new MockAuthenticationService(mockLogger.Object, configuration);
        
        // Modify original configuration object
        configuration.ClientId = "modified-client";
        configuration.EnableTokenCaching = false;

        // Assert - Service should maintain reference to same configuration object
        service.TestConfiguration.ClientId.ShouldBe("modified-client");
        service.TestConfiguration.EnableTokenCaching.ShouldBeFalse();
        service.TestConfiguration.TokenCacheLifetimeMinutes.ShouldBe(45);
    }

    [Fact]
    public void MultipleInstancesShouldBeIndependent()
    {
        // Arrange
        var mockLogger1 = new Mock<ILogger<MockAuthenticationService>>();
        var mockLogger2 = new Mock<ILogger<MockAuthenticationService>>();
        var config1 = new MockAuthenticationConfiguration { ClientId = "client-1" };
        var config2 = new MockAuthenticationConfiguration { ClientId = "client-2" };

        // Act
        var service1 = new MockAuthenticationService(mockLogger1.Object, config1);
        var service2 = new MockAuthenticationService(mockLogger2.Object, config2);

        // Assert
        service1.ShouldNotBeSameAs(service2);
        service1.TestConfiguration.ClientId.ShouldBe("client-1");
        service2.TestConfiguration.ClientId.ShouldBe("client-2");
        service1.TestLogger.ShouldNotBeSameAs(service2.TestLogger);
    }
}