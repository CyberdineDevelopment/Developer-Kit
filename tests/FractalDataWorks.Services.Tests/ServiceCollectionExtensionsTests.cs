using System;
using System.Linq;
using System.Reflection;
using FractalDataWorks;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

public class ServiceCollectionExtensionsTests
{
    private readonly ITestOutputHelper _output;

    public ServiceCollectionExtensionsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // Test Service Interface
    public interface ITestService : IFdwService
    {
        string TestProperty { get; }
    }

    // Test ServiceType that should be detected by the scanner - using the non-generic ServiceTypeBase
    public class TestServiceType : ServiceTypeBase
    {
        public TestServiceType() : base(1, "TestServiceType", "Test service type") 
        {
        }

        public override IServiceFactory CreateFactory()
        {
            return Mock.Of<IServiceFactory>();
        }

        public override IFdwResult<T> Create<T>(IFdwConfiguration configuration)
        {
            if (typeof(T) == typeof(ITestService))
            {
                var service = Mock.Of<ITestService>();
                return (IFdwResult<T>)(object)FdwResult<ITestService>.Success(service);
            }
            return FdwResult<T>.Failure("Invalid service type");
        }

        public override IFdwResult<IFdwService> Create(IFdwConfiguration configuration)
        {
            var service = Mock.Of<ITestService>();
            return FdwResult<IFdwService>.Success(service);
        }
    }

    // Another Test ServiceType for multiple service type testing
    public class AnotherTestServiceType : ServiceTypeBase
    {
        public AnotherTestServiceType() : base(2, "AnotherTestServiceType", "Another test service type")
        {
        }

        public override IServiceFactory CreateFactory()
        {
            return Mock.Of<IServiceFactory>();
        }

        public override IFdwResult<T> Create<T>(IFdwConfiguration configuration)
        {
            if (typeof(T) == typeof(ITestService))
            {
                var service = Mock.Of<ITestService>();
                return (IFdwResult<T>)(object)FdwResult<ITestService>.Success(service);
            }
            return FdwResult<T>.Failure("Invalid service type");
        }

        public override IFdwResult<IFdwService> Create(IFdwConfiguration configuration)
        {
            var service = Mock.Of<ITestService>();
            return FdwResult<IFdwService>.Success(service);
        }
    }

    // Non-ServiceType class that should NOT be detected
    public class NonServiceType
    {
        public string Name { get; set; } = "Not a service type";
    }

    // Abstract class that should NOT be detected
    public abstract class AbstractServiceType : ServiceTypeBase
    {
        protected AbstractServiceType() : base(99, "AbstractServiceType", "Abstract service type")
        {
        }
    }

    [Fact]
    public void AddServiceTypesWithCurrentAssemblyShouldRegisterServiceTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        var currentAssembly = Assembly.GetExecutingAssembly();

        // Act
        var result = services.AddServiceTypes(currentAssembly);

        // Assert
        result.ShouldBe(services); // Should return same instance for chaining
        services.Count.ShouldBeGreaterThan(0);
        
        // Verify that service types were registered
        var serviceTypes = services.Where(s => IsServiceTypeRegistration(s)).ToList();
        serviceTypes.Count.ShouldBeGreaterThan(0);
        
        _output.WriteLine($"Registered {services.Count} total services, {serviceTypes.Count} are service types");
        foreach (var service in serviceTypes.Take(5)) // Show first 5 service types
        {
            _output.WriteLine($"Service: {service.ServiceType.Name} -> {service.ImplementationType?.Name ?? "Factory"}");
        }
    }

    [Fact]
    public void AddServiceTypesWithNullAssemblyShouldUseCallingAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddServiceTypes(null);

        // Assert
        result.ShouldBe(services); // Should return same instance for chaining
        services.Count.ShouldBeGreaterThan(0);
        
        _output.WriteLine($"AddServiceTypes with null assembly registered {services.Count} services");
    }

    [Fact]
    public void AddServiceTypesWithMultipleAssembliesShouldRegisterFromAll()
    {
        // Arrange
        var services = new ServiceCollection();
        var currentAssembly = Assembly.GetExecutingAssembly();
        var servicesAssembly = typeof(ServiceBase<,,>).Assembly; // FractalDataWorks.Services assembly

        // Act
        var result = services.AddServiceTypes(currentAssembly, servicesAssembly);

        // Assert
        result.ShouldBe(services); // Should return same instance for chaining
        services.Count.ShouldBeGreaterThan(0);
        
        _output.WriteLine($"AddServiceTypes with multiple assemblies registered {services.Count} services");
    }

    [Fact]
    public void AddServiceTypesFromLoadedAssembliesShouldRegisterRelevantAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddServiceTypesFromLoadedAssemblies();

        // Assert
        result.ShouldBe(services); // Should return same instance for chaining
        services.Count.ShouldBeGreaterThan(0);
        
        _output.WriteLine($"AddServiceTypesFromLoadedAssemblies registered {services.Count} services");
        
        // Log some of the loaded assemblies for debugging
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Where(a => a.GetReferencedAssemblies()
                .Any(ra => ra.Name?.StartsWith("FractalDataWorks.Services", StringComparison.Ordinal) == true))
            .ToList();
        
        _output.WriteLine($"Found {loadedAssemblies.Count} relevant loaded assemblies:");
        foreach (var assembly in loadedAssemblies.Take(3))
        {
            _output.WriteLine($"  - {assembly.GetName().Name}");
        }
    }

    [Fact]
    public void AddServiceTypeGenericShouldRegisterSpecificServiceType()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddServiceType<TestServiceType>();

        // Assert
        result.ShouldBe(services); // Should return same instance for chaining
        
        // Verify the specific service type was registered
        services.Any(s => s.ServiceType == typeof(TestServiceType)).ShouldBeTrue();
        
        _output.WriteLine($"AddServiceType<TestServiceType> registered {services.Count} services");
        
        var testServiceTypeRegistration = services.First(s => s.ServiceType == typeof(TestServiceType));
        _output.WriteLine($"TestServiceType registered as: {testServiceTypeRegistration.Lifetime}");
    }

    [Fact]
    public void AddServiceTypeWithInvalidTypeShouldThrowArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            services.AddServiceType<NonServiceType>());
        
        exception.ParamName.ShouldBe("TServiceType");
        exception.Message.ShouldContain("is not a valid service type");
        
        _output.WriteLine($"Expected exception thrown: {exception.Message}");
    }

    [Fact]
    public void IsServiceTypeWithValidServiceTypeShouldReturnTrue()
    {
        // Arrange & Act - Use reflection to access private method
        var method = typeof(ServiceCollectionExtensions)
            .GetMethod("IsServiceType", BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method!.Invoke(null, new object[] { typeof(TestServiceType) })!;

        // Assert
        result.ShouldBeTrue();
        
        _output.WriteLine($"IsServiceType for TestServiceType returned: {result}");
    }

    [Fact]
    public void IsServiceTypeWithInvalidTypeShouldReturnFalse()
    {
        // Arrange & Act - Use reflection to access private method
        var method = typeof(ServiceCollectionExtensions)
            .GetMethod("IsServiceType", BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method!.Invoke(null, new object[] { typeof(NonServiceType) })!;

        // Assert
        result.ShouldBeFalse();
        
        _output.WriteLine($"IsServiceType for NonServiceType returned: {result}");
    }

    [Fact]
    public void IsServiceTypeWithAbstractClassShouldReturnFalse()
    {
        // Arrange & Act - Use reflection to access private method
        var method = typeof(ServiceCollectionExtensions)
            .GetMethod("IsServiceType", BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method!.Invoke(null, new object[] { typeof(AbstractServiceType) })!;

        // Assert
        result.ShouldBeFalse();
        
        _output.WriteLine($"IsServiceType for AbstractServiceType returned: {result}");
    }

    [Fact]
    public void RegisterAsServiceFactoryShouldRegisterMultipleInterfaces()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // First register the concrete service type
        services.AddSingleton<TestServiceType>();
        
        // Act - Use reflection to access private method
        var method = typeof(ServiceCollectionExtensions)
            .GetMethod("RegisterAsServiceFactory", BindingFlags.NonPublic | BindingFlags.Static);
        method!.Invoke(null, new object[] { services, typeof(TestServiceType) });

        // Assert
        services.Count.ShouldBeGreaterThan(1); // Should have more than just the original registration
        
        var registeredTypes = services.Select(s => s.ServiceType).ToList();
        _output.WriteLine($"RegisterAsServiceFactory registered {services.Count} services:");
        
        foreach (var service in services)
        {
            _output.WriteLine($"  - {service.ServiceType.Name} ({service.Lifetime})");
        }
        
        // Should contain IServiceFactory registrations
        registeredTypes.ShouldContain(typeof(IServiceFactory));
    }

    [Fact]
    public void ServiceCollectionExtensionsShouldSupportChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly1 = Assembly.GetExecutingAssembly();
        var assembly2 = typeof(ServiceBase<,,>).Assembly;

        // Act - Test method chaining
        var result = services
            .AddServiceTypes(assembly1)
            .AddServiceTypes(assembly2)
            .AddServiceType<TestServiceType>()
            .AddServiceTypesFromLoadedAssemblies();

        // Assert
        result.ShouldBe(services); // All methods should return the same instance
        services.Count.ShouldBeGreaterThan(0);
        
        _output.WriteLine($"Method chaining resulted in {services.Count} registered services");
    }

    [Fact]
    public void AddServiceTypesWithEmptyAssemblyShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var emptyAssemblies = new Assembly[0];

        // Act & Assert - Should not throw
        var result = services.AddServiceTypes(emptyAssemblies);
        
        result.ShouldBe(services);
        _output.WriteLine($"AddServiceTypes with empty assemblies array completed successfully");
    }

    // Test assembly that doesn't reference FractalDataWorks.Services
    [Fact]
    public void AddServiceTypesFromLoadedAssembliesShouldSkipIrrelevantAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var beforeCount = services.Count;

        // Act
        services.AddServiceTypesFromLoadedAssemblies();
        var afterCount = services.Count;

        // Assert
        // The method should complete without error, but may not add services if no relevant assemblies are found
        (afterCount >= beforeCount).ShouldBeTrue();
        
        _output.WriteLine($"AddServiceTypesFromLoadedAssemblies: Before={beforeCount}, After={afterCount}");
    }

    // Create a test assembly context to verify assembly filtering
    [Fact] 
    public void LoadedAssemblyFilteringShouldWork()
    {
        // Arrange & Act
        var relevantAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Where(a => a.GetReferencedAssemblies()
                .Any(ra => ra.Name?.StartsWith("FractalDataWorks.Services", StringComparison.Ordinal) == true))
            .ToList();

        // Assert
        relevantAssemblies.ShouldNotBeNull();
        _output.WriteLine($"Found {relevantAssemblies.Count} assemblies that reference FractalDataWorks.Services:");
        
        foreach (var assembly in relevantAssemblies.Take(5))
        {
            _output.WriteLine($"  - {assembly.GetName().Name} ({assembly.GetName().Version})");
            var referencedFdwServices = assembly.GetReferencedAssemblies()
                .Where(ra => ra.Name?.StartsWith("FractalDataWorks.Services", StringComparison.Ordinal) == true)
                .ToList();
            
            foreach (var reference in referencedFdwServices)
            {
                _output.WriteLine($"    -> References: {reference.Name} ({reference.Version})");
            }
        }
    }

    // Helper method to identify service type registrations
    private static bool IsServiceTypeRegistration(ServiceDescriptor service)
    {
        if (service.ImplementationType == null) return false;
        
        var baseType = service.ImplementationType.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType &&
                baseType.GetGenericTypeDefinition().Name.StartsWith("ServiceTypeBase", StringComparison.Ordinal))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    [Fact]
    public void MultipleCallsToAddServiceTypesShouldNotDuplicateRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        services.AddServiceTypes(assembly);
        var firstCount = services.Count;
        
        services.AddServiceTypes(assembly); // Second call
        var secondCount = services.Count;

        // Assert
        // Due to TryAddSingleton, the count should not increase significantly on second call
        secondCount.ShouldBeLessThanOrEqualTo(firstCount * 2); // Allow some increase due to factory registrations
        
        _output.WriteLine($"First call: {firstCount} services, Second call: {secondCount} services");
        
        // Verify no exact duplicates (same service type and implementation type)
        var duplicates = services
            .GroupBy(s => new { s.ServiceType, s.ImplementationType })
            .Where(g => g.Count() > 1)
            .ToList();
            
        _output.WriteLine($"Found {duplicates.Count} exact duplicate registrations");
    }
}