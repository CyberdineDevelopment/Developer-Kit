using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Extensions;
using FractalDataWorks.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Tests for ServiceCollectionExtensions.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddServiceTypesRegistersAllServiceTypesFromAssembly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        
        // Create a test assembly with service types
        var assembly = typeof(TestServiceType).Assembly;

        // Act
        services.AddServiceTypes(assembly);
        var provider = services.BuildServiceProvider();

        // Assert
        var serviceType = provider.GetService<TestServiceType>();
        serviceType.ShouldNotBeNull();
    }

    [Fact]
    public void AddServiceTypesUsesCallingAssemblyWhenNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        // Act - This will scan the test assembly
        services.AddServiceTypes();
        var provider = services.BuildServiceProvider();

        // Assert
        var serviceType = provider.GetService<TestServiceType>();
        serviceType.ShouldNotBeNull();
    }

    [Fact]
    public void AddServiceTypesRegistersAsServiceFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddSingleton<IConfigurationRegistry<TestServiceConfiguration>, TestConfigurationRegistry>();

        // Act
        services.AddServiceTypes(typeof(TestServiceType).Assembly);
        var provider = services.BuildServiceProvider();

        // Assert
        var factory = provider.GetService<IServiceFactory<ITestService, TestServiceConfiguration>>();
        factory.ShouldNotBeNull();
        factory.ShouldBeOfType<TestServiceType>();
    }

    [Fact]
    public void AddServiceTypesRegistersAsGenericServiceFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddSingleton<IConfigurationRegistry<TestServiceConfiguration>, TestConfigurationRegistry>();

        // Act
        services.AddServiceTypes(typeof(TestServiceType).Assembly);
        var provider = services.BuildServiceProvider();

        // Assert
        var factory = provider.GetService<IServiceFactory<ITestService>>();
        factory.ShouldNotBeNull();
        factory.ShouldBeOfType<TestServiceType>();
    }

    [Fact]
    public void AddServiceTypesRegistersAsNonGenericServiceFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddSingleton<IConfigurationRegistry<TestServiceConfiguration>, TestConfigurationRegistry>();

        // Act
        services.AddServiceTypes(typeof(TestServiceType).Assembly);
        var provider = services.BuildServiceProvider();

        // Assert
        var factory = provider.GetService<IServiceFactory>();
        factory.ShouldNotBeNull();
        factory.ShouldBeOfType<TestServiceType>();
    }

    [Fact]
    public void AddServiceTypesDoesNotRegisterAbstractTypes()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddServiceTypes(typeof(TestServiceType).Assembly);
        var provider = services.BuildServiceProvider();

        // Assert
        var abstractType = provider.GetService<AbstractTestServiceType>();
        abstractType.ShouldBeNull();
    }

    [Fact]
    public void AddServiceTypesDoesNotRegisterNonServiceTypes()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddServiceTypes(typeof(TestServiceType).Assembly);
        var provider = services.BuildServiceProvider();

        // Assert
        var nonServiceType = provider.GetService<NonServiceType>();
        nonServiceType.ShouldBeNull();
    }

    [Fact]
    public void AddServiceTypesFromMultipleAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        
        var assembly1 = typeof(TestServiceType).Assembly;
        var assembly2 = typeof(ServiceCollectionExtensionsTests).Assembly; // Same for this test

        // Act
        services.AddServiceTypes(assembly1, assembly2);
        var provider = services.BuildServiceProvider();

        // Assert
        var serviceType = provider.GetService<TestServiceType>();
        serviceType.ShouldNotBeNull();
    }

    [Fact]
    public void AddServiceTypesFromLoadedAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        // Act
        services.AddServiceTypesFromLoadedAssemblies();
        var provider = services.BuildServiceProvider();

        // Assert - Should find types from test assembly which references FractalDataWorks.Services
        var serviceType = provider.GetService<TestServiceType>();
        serviceType.ShouldNotBeNull();
    }

    [Fact]
    public void AddServiceTypeRegistersSpecificType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddSingleton<IConfigurationRegistry<TestServiceConfiguration>, TestConfigurationRegistry>();

        // Act
        services.AddServiceType<TestServiceType>();
        var provider = services.BuildServiceProvider();

        // Assert
        var serviceType = provider.GetService<TestServiceType>();
        serviceType.ShouldNotBeNull();
    }

    [Fact]
    public void AddServiceTypeThrowsForNonServiceType()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Should.Throw<ArgumentException>(() => services.AddServiceType<NonServiceType>())
            .Message.ShouldContain("not a valid service type");
    }

    [Fact]
    public void ServiceTypesAreRegisteredAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddSingleton<IConfigurationRegistry<TestServiceConfiguration>, TestConfigurationRegistry>();

        // Act
        services.AddServiceTypes(typeof(TestServiceType).Assembly);
        var provider = services.BuildServiceProvider();

        // Assert - Multiple resolves should return same instance
        var instance1 = provider.GetService<TestServiceType>();
        var instance2 = provider.GetService<TestServiceType>();
        instance1.ShouldBeSameAs(instance2);
    }

    [Fact]
    public void TryAddDoesNotOverwriteExistingRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddSingleton<IConfigurationRegistry<TestServiceConfiguration>, TestConfigurationRegistry>();
        
        var customInstance = new TestServiceType(null!, null!);
        services.AddSingleton(customInstance);

        // Act
        services.AddServiceTypes(typeof(TestServiceType).Assembly);
        var provider = services.BuildServiceProvider();

        // Assert - Should get our custom instance, not a new one
        var resolved = provider.GetService<TestServiceType>();
        resolved.ShouldBeSameAs(customInstance);
    }

    // Test types
    public interface ITestService : IFdwService
    {
    }

    public class TestServiceType : IServiceFactory<ITestService, TestServiceConfiguration>, IServiceFactory<ITestService>, IServiceFactory
    {
        private readonly ILogger<TestServiceType>? _logger;
        private readonly IConfigurationRegistry<TestServiceConfiguration> _configurations;

        public TestServiceType(ILogger<TestServiceType>? logger, IConfigurationRegistry<TestServiceConfiguration> configurations) 
        {
            _logger = logger;
            _configurations = configurations;
        }

        public IFdwResult<ITestService> Create(TestServiceConfiguration configuration)
        {
            return FdwResult<ITestService>.Success(new TestService());
        }

        public IFdwResult<ITestService> Create(IFdwConfiguration configuration)
        {
            return Create((TestServiceConfiguration)configuration);
        }

        IFdwResult<IFdwService> IServiceFactory.Create(IFdwConfiguration configuration)
        {
            var result = Create((TestServiceConfiguration)configuration);
            return result.IsSuccess 
                ? FdwResult<IFdwService>.Success(result.Value!) 
                : FdwResult<IFdwService>.Failure(result.Message!);
        }

        public IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwService
        {
            if (typeof(T) == typeof(ITestService))
            {
                var result = Create((TestServiceConfiguration)configuration);
                return result.IsSuccess
                    ? FdwResult<T>.Success((T)result.Value!)
                    : FdwResult<T>.Failure(result.Message!);
            }
            return FdwResult<T>.Failure("Invalid command type");
        }

        public async Task<ITestService> GetService(string configurationName)
        {
            var config = _configurations.GetAll().FirstOrDefault();
            if (config == null) throw new InvalidOperationException("No configuration found");
            var result = Create(config);
            if (!result.IsSuccess) throw new InvalidOperationException("Failed to create service");
            return await Task.FromResult(result.Value!);
        }

        public async Task<ITestService> GetService(int configurationId)
        {
            var config = _configurations.Get(configurationId);
            if (config == null) throw new InvalidOperationException("No configuration found");
            var result = Create(config);
            if (!result.IsSuccess) throw new InvalidOperationException("Failed to create service");
            return await Task.FromResult(result.Value!);
        }
    }

    public abstract class AbstractTestServiceType
    {
        protected AbstractTestServiceType(ILogger<AbstractTestServiceType>? logger, IConfigurationRegistry<TestServiceConfiguration> configurations) 
        {
        }
    }

    public class NonServiceType
    {
        public string Name { get; set; } = "Not a service";
    }

    public class TestService : ITestService
    {
        public string Id => Guid.NewGuid().ToString();
        public string ServiceType => "TestService";
        public bool IsAvailable => true;
        public string Name => "TestService";

        public Task<IFdwResult<TOut>> Execute<TOut>(ICommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IFdwResult<TOut>>(FdwResult<TOut>.Success(default(TOut)!));
        }

        public Task<IFdwResult> Execute(ICommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IFdwResult>(FdwResult.Success());
        }
    }

    public class TestServiceConfiguration : ConfigurationBase<TestServiceConfiguration>
    {
        public override string SectionName => "TestService";

        protected override IValidator<TestServiceConfiguration> GetValidator()
        {
            return null!;
        }
    }

    public class TestConfigurationRegistry : IConfigurationRegistry<TestServiceConfiguration>
    {
        public TestServiceConfiguration? Get(int id)
        {
            return new TestServiceConfiguration();
        }

        public IEnumerable<TestServiceConfiguration> GetAll()
        {
            return new[] { new TestServiceConfiguration() };
        }

        public bool TryGet(int id, out TestServiceConfiguration? configuration)
        {
            configuration = new TestServiceConfiguration();
            return true;
        }
    }
}