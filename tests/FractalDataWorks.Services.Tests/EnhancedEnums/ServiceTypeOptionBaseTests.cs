using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Services.EnhancedEnums;
using FractalDataWorks.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Services.Tests.EnhancedEnums;

/// <summary>
/// Tests for ServiceTypeOptionBase functionality.
/// </summary>
public class ServiceTypeOptionBaseTests
{

    [Fact]
    public void ServiceTypeOptionBaseInheritsFromEnumOptionBase()
    {
        // Arrange & Act
        var serviceType = TestEmailServiceType.SendGrid;

        // Assert
        serviceType.ShouldNotBeNull();
        serviceType.Id.ShouldBe(1);
        serviceType.Name.ShouldBe("SendGrid");
    }

    [Fact]
    public void AddWithIServiceCollectionRegistersAllComponents()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        TestEmailServiceType.SendGrid.Add(services, configuration);
        var provider = services.BuildServiceProvider();

        // Assert

        // Verify configuration registration
        var optionsMonitor = provider.GetService<IOptionsMonitor<List<TestEmailConfiguration>>>();
        optionsMonitor.ShouldNotBeNull();

        var optionsSnapshot = provider.GetService<IOptionsSnapshot<List<TestEmailConfiguration>>>();
        optionsSnapshot.ShouldNotBeNull();

        // Verify configuration registry
        var configRegistry = provider.GetService<IConfigurationRegistry<TestEmailConfiguration>>();
        configRegistry.ShouldNotBeNull();
        configRegistry.GetAll().ShouldNotBeEmpty();

        // Verify factory registration at all levels
        var factory = provider.GetService<TestEmailServiceFactory>();
        factory.ShouldNotBeNull();

        var typedFactory = provider.GetService<IServiceFactory<ITestEmailService, TestEmailConfiguration>>();
        typedFactory.ShouldNotBeNull();

        var genericFactory = provider.GetService<IServiceFactory<ITestEmailService>>();
        genericFactory.ShouldNotBeNull();

        var baseFactory = provider.GetService<IServiceFactory>();
        baseFactory.ShouldNotBeNull();

        // Verify service registration
        var emailService = provider.GetService<ITestEmailService>();
        emailService.ShouldNotBeNull();
    }

    [Fact]
    public void AddWithIServiceCollectionRegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        TestEmailServiceType.SendGrid.Add(services, configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        var emailService = provider.GetService<ITestEmailService>();
        emailService.ShouldNotBeNull();
        emailService.ShouldBeOfType<TestEmailService>();

        var configRegistry = provider.GetService<IConfigurationRegistry<TestEmailConfiguration>>();
        configRegistry.ShouldNotBeNull();
        
        var configs = configRegistry.GetAll().ToList();
        configs.ShouldNotBeEmpty();
        configs.Any(c => ((dynamic)c).Id == 1 && ((dynamic)c).IsEnabled).ShouldBeTrue();
    }

    [Fact]
    public void ConfigurationRegistryImplWorksCorrectly()
    {
        // Arrange
        var configs = new List<TestEmailConfiguration>
        {
            new() { Id = 1, Name = "Config1", IsEnabled = true },
            new() { Id = 2, Name = "Config2", IsEnabled = false },
            new() { Id = 3, Name = "Config3", IsEnabled = true }
        };

        var registry = new ConfigurationRegistryImpl<TestEmailConfiguration>(configs);

        // Act & Assert
        registry.Get(1).ShouldNotBeNull();
        registry.Get(1)!.Name.ShouldBe("Config1");

        registry.Get(999).ShouldBeNull();

        registry.GetAll().Count().ShouldBe(3);

        registry.TryGet(2, out var config2).ShouldBeTrue();
        config2.ShouldNotBeNull();
        config2.Name.ShouldBe("Config2");

        registry.TryGet(999, out var configNotFound).ShouldBeFalse();
        configNotFound.ShouldBeNull();
    }

    [Fact]
    public void RegisterAdditionalIsCalledAfterRegister()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        TestEmailServiceTypeWithAdditional.Custom.Add(services, configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var additionalService = provider.GetService<IAdditionalTestService>();
        additionalService.ShouldNotBeNull();
        additionalService.ShouldBeOfType<AdditionalTestService>();
    }

    [Fact]
    public void CustomConfigurationSectionIsUsed()
    {
        // Arrange
        var services = new ServiceCollection();
        var configDict = new Dictionary<string, string>
        {
            {"CustomEmail:Configurations:0:Id", "1"},
            {"CustomEmail:Configurations:0:Name", "CustomConfig"},
            {"CustomEmail:Configurations:0:IsEnabled", "true"},
            {"CustomEmail:Configurations:0:SectionName", "CustomEmail"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        // Act
        TestEmailServiceTypeCustomSection.CustomSection.Add(services, configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var configRegistry = provider.GetService<IConfigurationRegistry<TestEmailConfiguration>>();
        configRegistry.ShouldNotBeNull();
        
        var configs = configRegistry.GetAll().ToList();
        configs.ShouldNotBeEmpty();
        configs.First().Name.ShouldBe("CustomConfig");
    }

    [Fact]
    public void ThrowsWhenNoEnabledConfigurationFound()
    {
        // Arrange
        var services = new ServiceCollection();
        var configDict = new Dictionary<string, string>
        {
            {"ITestEmailService:Configurations:0:Id", "1"}, // Same ID as service type but disabled
            {"ITestEmailService:Configurations:0:Name", "DisabledConfig"},
            {"ITestEmailService:Configurations:0:IsEnabled", "false"},
            {"ITestEmailService:Configurations:0:SectionName", "test"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        TestEmailServiceType.SendGrid.Add(services, configuration);
        var provider = services.BuildServiceProvider();

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => provider.GetService<ITestEmailService>());
        exception.Message.ShouldContain("No enabled configuration found for service type 'SendGrid' with ID 1");
    }

    private static IConfiguration CreateTestConfiguration()
    {
        var configDict = new Dictionary<string, string>
        {
            {"ITestEmailService:Configurations:0:Id", "1"},
            {"ITestEmailService:Configurations:0:Name", "TestConfig"},
            {"ITestEmailService:Configurations:0:IsEnabled", "true"},
            {"ITestEmailService:Configurations:0:SectionName", "test"},
            {"ITestEmailService:Configurations:1:Id", "2"},
            {"ITestEmailService:Configurations:1:Name", "TestConfig2"},
            {"ITestEmailService:Configurations:1:IsEnabled", "false"},
            {"ITestEmailService:Configurations:1:SectionName", "test2"}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();
    }

}

// ========== TEST TYPES ==========

public interface ITestEmailService : IFdwService
{
    Task<IFdwResult> SendEmailAsync(string to, string subject, string body);
}

public sealed class TestEmailService : ITestEmailService
{
    private readonly TestEmailConfiguration _configuration;

    public TestEmailService(TestEmailConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Id => _configuration.Id.ToString();
    public string ServiceType => _configuration.Name;
    public bool IsAvailable => _configuration.IsEnabled;

    public Task<IFdwResult> SendEmailAsync(string to, string subject, string body)
    {
        return Task.FromResult(FdwResult.Success());
    }
}

public sealed class TestEmailConfiguration : IFdwConfiguration
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public string SectionName { get; set; } = string.Empty;

    public bool Validate() => !string.IsNullOrEmpty(Name);
}

public sealed class TestEmailServiceFactory : IServiceFactory<ITestEmailService, TestEmailConfiguration>
{
    public IFdwResult<ITestEmailService> Create(TestEmailConfiguration configuration)
    {
        var service = new TestEmailService(configuration);
        return FdwResult<ITestEmailService>.Success(service);
    }

    // IServiceFactory<ITestEmailService> explicit implementation
    IFdwResult<ITestEmailService> IServiceFactory<ITestEmailService>.Create(IFdwConfiguration configuration)
    {
        if (configuration is TestEmailConfiguration emailConfig)
        {
            return Create(emailConfig);
        }
        return FdwResult<ITestEmailService>.Failure("Invalid configuration type");
    }

    // IServiceFactory generic Create<T> implementation
    public IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwService
    {
        if (typeof(T).IsAssignableFrom(typeof(ITestEmailService)))
        {
            var result = ((IServiceFactory<ITestEmailService>)this).Create(configuration);
            if (result.IsSuccess && result.Value is T typedService)
            {
                return FdwResult<T>.Success(typedService);
            }
            return FdwResult<T>.Failure(result.Message ?? "Service creation failed");
        }
        return FdwResult<T>.Failure("Invalid service type");
    }

    // IServiceFactory base Create implementation
    public IFdwResult<IFdwService> Create(IFdwConfiguration configuration)
    {
        var result = ((IServiceFactory<ITestEmailService>)this).Create(configuration);
        if (result.IsSuccess && result.Value is IFdwService fdwService)
        {
            return FdwResult<IFdwService>.Success(fdwService);
        }
        return FdwResult<IFdwService>.Failure(result.Message ?? "Service creation failed");
    }

    public Task<ITestEmailService> GetService(string configurationName)
    {
        throw new NotImplementedException();
    }

    public Task<ITestEmailService> GetService(int configurationId)
    {
        throw new NotImplementedException();
    }
}

public sealed class TestEmailServiceType : ServiceTypeOptionBase<TestEmailServiceType, ITestEmailService, TestEmailConfiguration, TestEmailServiceFactory>
{
    public static readonly TestEmailServiceType SendGrid = new(1, "SendGrid");
    public static readonly TestEmailServiceType Smtp = new(2, "SMTP");

    private TestEmailServiceType(int id, string name) : base(id, name)
    {
    }
}

// Test type with custom RegisterAdditional
public interface IAdditionalTestService
{
    string GetMessage();
}

public sealed class AdditionalTestService : IAdditionalTestService
{
    public string GetMessage() => "Additional service registered";
}

public sealed class TestEmailServiceTypeWithAdditional : ServiceTypeOptionBase<TestEmailServiceTypeWithAdditional, ITestEmailService, TestEmailConfiguration, TestEmailServiceFactory>
{
    public static readonly TestEmailServiceTypeWithAdditional Custom = new(1, "Custom");

    private TestEmailServiceTypeWithAdditional(int id, string name) : base(id, name)
    {
    }

    protected override void RegisterAdditional(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAdditionalTestService, AdditionalTestService>();
    }
}

// Test type with custom configuration section
public sealed class TestEmailServiceTypeCustomSection : ServiceTypeOptionBase<TestEmailServiceTypeCustomSection, ITestEmailService, TestEmailConfiguration, TestEmailServiceFactory>
{
    public static readonly TestEmailServiceTypeCustomSection CustomSection = new(1, "CustomSection");

    private TestEmailServiceTypeCustomSection(int id, string name) : base(id, name)
    {
    }

    protected override string ConfigurationSection => "CustomEmail:Configurations";
}