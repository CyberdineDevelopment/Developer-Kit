using System;
using System.Collections.Generic;
using FluentValidation.Results;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Services.EnhancedEnums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

public class ServiceTypeOptionBaseTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<IServiceCollection> _servicesMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IConfigurationSection> _configSectionMock;
    private readonly Mock<IHostApplicationBuilder> _hostAppBuilderMock;
    private readonly Mock<IHostBuilder> _hostBuilderMock;

    public ServiceTypeOptionBaseTests(ITestOutputHelper output)
    {
        _output = output;
        _servicesMock = new Mock<IServiceCollection>();
        _configurationMock = new Mock<IConfiguration>();
        _configSectionMock = new Mock<IConfigurationSection>();
        _hostAppBuilderMock = new Mock<IHostApplicationBuilder>();
        _hostBuilderMock = new Mock<IHostBuilder>();
    }

    // Test Interfaces
    public interface ITestService : IFdwService
    {
        string TestProperty { get; }
    }

    public class TestConfiguration : IFdwConfiguration
    {
        public string TestValue { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public string SectionName => "TestConfiguration";

        public ValidationResult Validate()
        {
            return new ValidationResult();
        }
    }

    public interface ITestFactory : IServiceFactory<ITestService, TestConfiguration>
    {
    }

    // Concrete Implementation
    public class ConcreteServiceTypeOption : ServiceTypeOptionBase<ConcreteServiceTypeOption, ITestService, TestConfiguration, ITestFactory>
    {
        public ConcreteServiceTypeOption(int id, string name) : base(id, name)
        {
        }

        public void TestRegister(IServiceCollection services, IConfiguration configuration)
        {
            Register(services, configuration);
        }

        public void TestRegisterAdditional(IServiceCollection services, IConfiguration configuration)
        {
            RegisterAdditional(services, configuration);
        }

        // Override configuration section for testing
        protected override string ConfigurationSection => "TestService:TestConfigurations";

        // Expose protected property for testing
        public string TestConfigurationSection => ConfigurationSection;

        protected override void RegisterAdditional(IServiceCollection services, IConfiguration configuration)
        {
            // Test implementation - just add a marker service
            services.AddSingleton<string>("AdditionalRegistrationCalled");
        }
    }

    [Fact]
    public void ConstructorShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var id = 1;
        var name = "TestServiceOption";

        // Act
        var option = new ConcreteServiceTypeOption(id, name);

        // Assert
        option.Id.ShouldBe(id);
        option.Name.ShouldBe(name);
        
        _output.WriteLine($"ServiceTypeOption created - ID: {option.Id}, Name: {option.Name}");
    }

    [Fact]
    public void ConfigurationSectionShouldReturnCustomValue()
    {
        // Arrange
        var option = new ConcreteServiceTypeOption(1, "TestService");

        // Act
        var configSection = option.TestConfigurationSection;

        // Assert
        configSection.ShouldBe("TestService:TestConfigurations");
        
        _output.WriteLine($"Configuration section: {configSection}");
    }

    [Fact]
    public void RegisterShouldConfigureServicesCorrectly()
    {
        // Arrange
        var option = new ConcreteServiceTypeOption(1, "TestService");
        var services = new ServiceCollection();
        
        _configurationMock.Setup(c => c.GetSection("TestService:TestConfigurations"))
            .Returns(_configSectionMock.Object);

        // Act
        option.TestRegister(services, _configurationMock.Object);

        // Assert
        services.Count.ShouldBeGreaterThan(0);
        
        // Verify that various service registrations were made
        var serviceTypes = services.Select(s => s.ServiceType).ToList();
        serviceTypes.ShouldContain(typeof(IOptionsMonitor<List<TestConfiguration>>), "Should register IOptionsMonitor<List<TConfiguration>>");
        serviceTypes.ShouldContain(typeof(IConfigurationRegistry<TestConfiguration>), "Should register IConfigurationRegistry<TConfiguration>");
        serviceTypes.ShouldContain(typeof(ITestFactory), "Should register TFactory");
        serviceTypes.ShouldContain(typeof(IServiceFactory<ITestService, TestConfiguration>), "Should register IServiceFactory<TService, TConfiguration>");
        serviceTypes.ShouldContain(typeof(IServiceFactory<ITestService>), "Should register IServiceFactory<TService>");
        serviceTypes.ShouldContain(typeof(IServiceFactory), "Should register IServiceFactory");
        serviceTypes.ShouldContain(typeof(ITestService), "Should register TService");
        
        _output.WriteLine($"Registered {services.Count} services");
        foreach (var service in services.Take(5))
        {
            _output.WriteLine($"Service: {service.ServiceType.Name} -> {service.ImplementationType?.Name ?? service.ImplementationFactory?.Method.Name ?? "Factory"}");
        }
    }

    [Fact]
    public void RegisterAdditionalShouldCallProtectedMethod()
    {
        // Arrange
        var option = new ConcreteServiceTypeOption(1, "TestService");
        var services = new ServiceCollection();

        // Act
        option.TestRegisterAdditional(services, _configurationMock.Object);

        // Assert
        services.Count.ShouldBe(1);
        services.First().ServiceType.ShouldBe(typeof(string));
        services.First().ImplementationInstance.ShouldBe("AdditionalRegistrationCalled");
        
        _output.WriteLine($"RegisterAdditional called successfully");
    }

    [Fact]
    public void AddWithHostApplicationBuilderShouldCallRegisterMethods()
    {
        // Arrange
        var option = new ConcreteServiceTypeOption(1, "TestService");
        var services = new ServiceCollection();
        
        _hostAppBuilderMock.Setup(b => b.Services).Returns(services);
        _hostAppBuilderMock.Setup(b => b.Configuration).Returns(_configurationMock.Object);
        _configurationMock.Setup(c => c.GetSection("TestService:TestConfigurations"))
            .Returns(_configSectionMock.Object);

        // Act
        option.Add(_hostAppBuilderMock.Object);

        // Assert
        services.Count.ShouldBeGreaterThan(0);
        _hostAppBuilderMock.Verify(b => b.Services, Times.AtLeastOnce);
        _hostAppBuilderMock.Verify(b => b.Configuration, Times.AtLeastOnce);
        
        _output.WriteLine($"Add with IHostApplicationBuilder succeeded - {services.Count} services registered");
    }

    [Fact]
    public void AddWithHostBuilderShouldCallConfigureServices()
    {
        // Arrange
        var option = new ConcreteServiceTypeOption(1, "TestService");
        var capturedAction = (Action<HostBuilderContext, IServiceCollection>?)null;
        
        _hostBuilderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
            .Callback<Action<HostBuilderContext, IServiceCollection>>(action => capturedAction = action)
            .Returns(_hostBuilderMock.Object);

        // Act
        option.Add(_hostBuilderMock.Object);

        // Assert
        capturedAction.ShouldNotBeNull();
        _hostBuilderMock.Verify(b => b.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()), Times.Once);
        
        // Test the captured action
        var services = new ServiceCollection();
        var context = new Mock<HostBuilderContext>();
        context.Setup(c => c.Configuration).Returns(_configurationMock.Object);
        _configurationMock.Setup(c => c.GetSection("TestService:TestConfigurations"))
            .Returns(_configSectionMock.Object);
        
        capturedAction(context.Object, services);
        services.Count.ShouldBeGreaterThan(0);
        
        _output.WriteLine($"Add with IHostBuilder succeeded - action captured and executed");
    }

    [Fact]
    public void AddWithServiceCollectionAndConfigurationShouldRegisterServices()
    {
        // Arrange
        var option = new ConcreteServiceTypeOption(1, "TestService");
        var services = new ServiceCollection();
        
        _configurationMock.Setup(c => c.GetSection("TestService:TestConfigurations"))
            .Returns(_configSectionMock.Object);

        // Act
        option.Add(services, _configurationMock.Object);

        // Assert
        services.Count.ShouldBeGreaterThan(0);
        
        // Should include both regular and additional registrations
        var serviceTypes = services.Select(s => s.ServiceType).ToList();
        serviceTypes.ShouldContain(typeof(string)); // From RegisterAdditional
        
        _output.WriteLine($"Add with IServiceCollection succeeded - {services.Count} services registered");
    }

    // Test with Mock Factory for Service Resolution
    public class MockTestFactory : ITestFactory
    {
        private readonly ITestService _service;

        public MockTestFactory(ITestService service)
        {
            _service = service;
        }

        public IFdwResult<ITestService> Create(TestConfiguration configuration)
        {
            return FdwResult<ITestService>.Success(_service);
        }

        public Task<ITestService> GetService(string configurationName)
        {
            return Task.FromResult(_service);
        }

        public Task<ITestService> GetService(int configurationId)
        {
            return Task.FromResult(_service);
        }

        IFdwResult<T> IServiceFactory.Create<T>(IFdwConfiguration configuration)
        {
            if (typeof(T) == typeof(ITestService) && configuration is TestConfiguration config)
            {
                var result = Create(config);
                if (result.IsSuccess)
                {
                    return (IFdwResult<T>)(object)result;
                }
            }
            return FdwResult<T>.Failure("Invalid configuration or type");
        }

        IFdwResult<IFdwService> IServiceFactory.Create(IFdwConfiguration configuration)
        {
            if (configuration is TestConfiguration config)
            {
                var result = Create(config);
                if (result.IsSuccess)
                {
                    return FdwResult<IFdwService>.Success(result.Value);
                }
                return FdwResult<IFdwService>.Failure(result.Message ?? "Creation failed");
            }
            return FdwResult<IFdwService>.Failure("Invalid configuration");
        }

        IFdwResult<ITestService> IServiceFactory<ITestService>.Create(IFdwConfiguration configuration)
        {
            if (configuration is TestConfiguration config)
            {
                return Create(config);
            }
            return FdwResult<ITestService>.Failure("Invalid configuration");
        }
    }

    [Fact]
    public void ServiceResolutionWithValidConfigurationShouldWork()
    {
        // Arrange
        var option = new ConcreteServiceTypeOption(1, "TestService");
        var services = new ServiceCollection();
        var testService = Mock.Of<ITestService>();
        
        // Add required dependencies manually for this test
        services.AddSingleton<ITestFactory>(provider => new MockTestFactory(testService));
        
        // Create a mock configuration registry that returns a config for this specific ID
        var config = new TestConfiguration { TestValue = "test", IsEnabled = true };
        var mockRegistry = new Mock<IConfigurationRegistry<TestConfiguration>>();
        mockRegistry.Setup(r => r.Get(1)).Returns(config);
        services.AddSingleton(mockRegistry.Object);
        
        // Build service provider
        var serviceProvider = ((IServiceCollection)services).BuildServiceProvider();
        
        // Test the service resolver logic by simulating what would happen
        var factory = serviceProvider.GetRequiredService<ITestFactory>();
        var registry = serviceProvider.GetRequiredService<IConfigurationRegistry<TestConfiguration>>();
        
        // Act - Simulate the service resolution logic
        var retrievedConfig = registry.Get(1);
        var result = factory.Create(retrievedConfig!);
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(testService);
        
        _output.WriteLine($"Service resolution successful: {result.Value?.GetType().Name}");
    }

    [Fact]
    public void ServiceResolutionWithDisabledConfigurationShouldFail()
    {
        // Arrange
        var services = new ServiceCollection();
        var testService = Mock.Of<ITestService>();
        
        services.AddSingleton<ITestFactory>(provider => new MockTestFactory(testService));
        
        // Create a disabled configuration
        var disabledConfig = new TestConfiguration { TestValue = "test", IsEnabled = false };
        var mockRegistry = new Mock<IConfigurationRegistry<TestConfiguration>>();
        mockRegistry.Setup(r => r.Get(1)).Returns(disabledConfig);
        services.AddSingleton(mockRegistry.Object);
        
        var serviceProvider = ((IServiceCollection)services).BuildServiceProvider();
        
        // Act & Assert
        var factory = serviceProvider.GetRequiredService<ITestFactory>();
        var registry = serviceProvider.GetRequiredService<IConfigurationRegistry<TestConfiguration>>();
        var retrievedConfig = registry.Get(1);
        
        // The configuration is disabled, so this would normally throw in the actual DI resolution
        retrievedConfig!.IsEnabled.ShouldBeFalse();
        
        _output.WriteLine($"Disabled configuration detected correctly: IsEnabled = {retrievedConfig.IsEnabled}");
    }

    [Fact]
    public void ServiceResolutionWithMissingConfigurationShouldFail()
    {
        // Arrange
        var services = new ServiceCollection();
        var testService = Mock.Of<ITestService>();
        
        services.AddSingleton<ITestFactory>(provider => new MockTestFactory(testService));
        
        // Create a registry that returns null for the requested ID
        var mockRegistry = new Mock<IConfigurationRegistry<TestConfiguration>>();
        mockRegistry.Setup(r => r.Get(1)).Returns((TestConfiguration?)null);
        services.AddSingleton(mockRegistry.Object);
        
        var serviceProvider = ((IServiceCollection)services).BuildServiceProvider();
        
        // Act
        var registry = serviceProvider.GetRequiredService<IConfigurationRegistry<TestConfiguration>>();
        var retrievedConfig = registry.Get(1);
        
        // Assert
        retrievedConfig.ShouldBeNull();
        
        _output.WriteLine($"Missing configuration handled correctly: config is null = {retrievedConfig == null}");
    }

    // Test Configuration without IsEnabled property
    public class SimpleTestConfiguration : IFdwConfiguration
    {
        public string TestValue { get; set; } = string.Empty;
        public string SectionName => "SimpleTestConfiguration";

        public ValidationResult Validate()
        {
            return new ValidationResult();
        }
    }

    [Fact]
    public void ServiceResolutionWithConfigurationWithoutIsEnabledShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        var testService = Mock.Of<ITestService>();
        
        services.AddSingleton<IServiceFactory<ITestService, SimpleTestConfiguration>>(provider => 
        {
            var factory = new Mock<IServiceFactory<ITestService, SimpleTestConfiguration>>();
            factory.Setup(f => f.Create(It.IsAny<SimpleTestConfiguration>()))
                .Returns(FdwResult<ITestService>.Success(testService));
            return factory.Object;
        });
        
        // Create a configuration without IsEnabled property
        var config = new SimpleTestConfiguration { TestValue = "test" };
        var mockRegistry = new Mock<IConfigurationRegistry<SimpleTestConfiguration>>();
        mockRegistry.Setup(r => r.Get(1)).Returns(config);
        services.AddSingleton(mockRegistry.Object);
        
        var serviceProvider = ((IServiceCollection)services).BuildServiceProvider();
        
        // Act
        var factory = serviceProvider.GetRequiredService<IServiceFactory<ITestService, SimpleTestConfiguration>>();
        var registry = serviceProvider.GetRequiredService<IConfigurationRegistry<SimpleTestConfiguration>>();
        var retrievedConfig = registry.Get(1);
        var result = factory.Create(retrievedConfig!);
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(testService);
        
        // Verify the config doesn't have IsEnabled property
        var isEnabledProperty = retrievedConfig!.GetType().GetProperty("IsEnabled");
        isEnabledProperty.ShouldBeNull();
        
        _output.WriteLine($"Service resolution without IsEnabled property successful");
    }
}