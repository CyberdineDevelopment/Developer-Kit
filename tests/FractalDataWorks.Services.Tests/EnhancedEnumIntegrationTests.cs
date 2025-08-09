using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services.Extensions;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Integration tests for Enhanced Enum workflow with ServiceTypes, ConnectionTypes, and ToolTypes.
/// </summary>
public class EnhancedEnumIntegrationTests
{
    [Fact]
    public void ConcreteEnumOptionServiceTypeCanBeCreated()
    {
        // This test verifies that concrete implementations with [EnumOption] work correctly
        // Arrange
        var testServiceType = new TestServiceTypeOption();

        // Assert
        testServiceType.Id.ShouldBe(1, $"Id should be set correctly");
        testServiceType.Name.ShouldBe("TestService", $"Name should be set correctly");
        testServiceType.Description.ShouldBe("Test service for unit testing", $"Description should be set correctly");
    }

    [Fact]
    public void ServiceCollectionExtensionsRegistersServiceTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddServiceTypes(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        // Assert
        var registeredService = provider.GetService<TestServiceTypeOption>();
        registeredService.ShouldNotBeNull($"TestServiceTypeOption should be registered");
        
        var factory = provider.GetService<IServiceFactory<ITestService, TestConfiguration>>();
        factory.ShouldNotBeNull($"IServiceFactory should be registered");
        // TestServiceTypeOption implements the factory pattern
    }

    [Fact]
    public void ServiceTypeWithEnumOptionAttributeIsDetected()
    {
        // Arrange
        var testServiceType = typeof(TestServiceTypeOption);

        // Act
        var enumOptionAttr = testServiceType.GetCustomAttribute<EnumOptionAttribute>();
        var baseType = testServiceType.BaseType;

        // Assert
        enumOptionAttr.ShouldNotBeNull($"TestServiceTypeOption should have [EnumOption] attribute");
        // EnumOptionAttribute properties will be validated when Enhanced Enums analyzer is active
        // EnumOption attribute doesn't have a Name property - name comes from class
        
        baseType.ShouldNotBeNull($"Should have a base type");
        baseType.GetGenericTypeDefinition().ShouldBe(typeof(ServiceTypeBase<,>), 
            $"Should inherit from ServiceTypeBase<,>");
    }

    [Fact]
    public async Task ServiceFactoryCreateMethodWorks()
    {
        // Arrange
        var factory = new TestServiceTypeOption();
        var config = new TestConfiguration { IsValid = true };

        // Act
        var typedFactory = factory.CreateTypedFactory();
        var result = await Task.FromResult(typedFactory.Create(config));

        // Assert
        result.ShouldNotBeNull($"Create should return a result");
        result.IsSuccess.ShouldBeTrue($"Create should be successful");
        result.Value.ShouldNotBeNull($"Result should contain a service instance");
        result.Value.ShouldBeOfType<TestService>($"Should create TestService instance");
    }

    [Fact]
    public void MultipleServiceTypesCanCoexist()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddServiceTypes(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        // Act
        var testService = provider.GetService<TestServiceTypeOption>();
        var anotherService = provider.GetService<AnotherServiceTypeOption>();

        // Assert
        testService.ShouldNotBeNull($"TestServiceTypeOption should be registered");
        anotherService.ShouldNotBeNull($"AnotherServiceTypeOption should be registered");
        testService.Id.ShouldNotBe(anotherService.Id, $"Different service types should have different IDs");
    }

    // Test implementations
    public interface ITestService : IFdwService
    {
        string TestMethod();
    }

    public class TestService : ITestService
    {
        public string Id => Guid.NewGuid().ToString();
        public string ServiceType => "TestService";
        public bool IsAvailable => true;
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Interface implementation")]
        public string Name => "TestService";

        public string TestMethod() => "Test";

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Interface implementation")]
        public Task<IFdwResult> Execute(ICommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult<IFdwResult>(FdwResult.Success());
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Interface implementation")]
        public Task<IFdwResult<TOut>> Execute<TOut>(ICommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult<IFdwResult<TOut>>(FdwResult<TOut>.Success(default(TOut)!));
        }
    }

    public class TestConfiguration : IFdwConfiguration
    {
        public string SectionName => "TestConfiguration";
        public bool IsValid { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public DateTime? Modified { get; set; }
        public bool IsEnabled { get; set; }
        
        public bool Validate() => IsValid;
        
        public IFdwConfiguration Clone() => new TestConfiguration { IsValid = IsValid };
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Interface implementation")]
        public void CopyTo(IFdwConfiguration target) { }
    }

    // Test service factory implementation
    public class TestServiceFactory : IServiceFactory<ITestService, TestConfiguration>
    {
        public IFdwResult<ITestService> Create(TestConfiguration configuration)
        {
            return FdwResult<ITestService>.Success(new TestService());
        }

        public IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwService
        {
            if (typeof(T) == typeof(ITestService))
            {
                return (IFdwResult<T>)(object)Create((TestConfiguration)configuration);
            }
            return FdwResult<T>.Failure("Invalid command type");
        }

        IFdwResult<ITestService> IServiceFactory<ITestService>.Create(IFdwConfiguration configuration)
        {
            return Create((TestConfiguration)configuration);
        }

        public IFdwResult<IFdwService> Create(IFdwConfiguration configuration)
        {
            var result = Create((TestConfiguration)configuration);
            if (result.IsSuccess)
            {
                return FdwResult<IFdwService>.Success(result.Value);
            }
            return FdwResult<IFdwService>.Failure(result.Message ?? "Invalid command type");
        }

        public Task<ITestService> GetService(string configurationName)
        {
            return Task.FromResult<ITestService>(new TestService());
        }

        public Task<ITestService> GetService(int configurationId)
        {
            return Task.FromResult<ITestService>(new TestService());
        }
    }

    [EnumOption]
    public class TestServiceTypeOption : ServiceTypeBase<ITestService, TestConfiguration>
    {
        public TestServiceTypeOption() : base(1, "TestService", "Test service for unit testing")
        {
        }

        public override IServiceFactory<ITestService, TestConfiguration> CreateTypedFactory()
        {
            return new TestServiceFactory();
        }
    }

    [EnumOption]
    public class AnotherServiceTypeOption : ServiceTypeBase<ITestService, TestConfiguration>
    {
        public AnotherServiceTypeOption() : base(2, "AnotherService", "Another test service")
        {
        }

        public override IServiceFactory<ITestService, TestConfiguration> CreateTypedFactory()
        {
            return new TestServiceFactory();
        }
    }
}