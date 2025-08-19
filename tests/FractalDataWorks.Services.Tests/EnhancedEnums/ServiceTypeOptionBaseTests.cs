using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Configuration;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Services.EnhancedEnums;

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
    public void ConfigurationRegistryCoreWorksCorrectly()
    {
        // Arrange
        var configs = new List<TestEmailConfiguration>
        {
            new() { Id = 1, Name = "Config1", IsEnabled = true },
            new() { Id = 2, Name = "Config2", IsEnabled = false },
            new() { Id = 3, Name = "Config3", IsEnabled = true }
        };

        var registry = new ConfigurationRegistryCore<TestEmailConfiguration>(configs);

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

// Test type with custom RegisterAdditional

// Test type with custom configuration section