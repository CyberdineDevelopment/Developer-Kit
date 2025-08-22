using System;
using FractalDataWorks;
using FractalDataWorks.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

public class IServiceTypeTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceCollection> _servicesMock;

    public IServiceTypeTests(ITestOutputHelper output)
    {
        _output = output;
        _serviceProviderMock = new Mock<IServiceProvider>();
        _servicesMock = new Mock<IServiceCollection>();
    }

    // Test Service Interface
    public interface ITestService : IFdwService
    {
        string TestProperty { get; }
    }

    // Mock Implementation of IServiceType
    public class MockServiceType : IServiceType
    {
        public Type ServiceType { get; }
        public string Category { get; }

        public MockServiceType(Type serviceType, string category)
        {
            ServiceType = serviceType;
            Category = category;
        }

        public object CreateService(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService(ServiceType) ?? Mock.Of<ITestService>();
        }

        public void RegisterService(IServiceCollection services)
        {
            services.AddSingleton(ServiceType, CreateService);
        }
    }

    [Fact]
    public void ServiceTypePropertyShouldReturnCorrectType()
    {
        // Arrange
        var expectedType = typeof(ITestService);
        var mockServiceType = new MockServiceType(expectedType, "TestCategory");

        // Act
        var actualType = mockServiceType.ServiceType;

        // Assert
        actualType.ShouldBe(expectedType);
        actualType.Name.ShouldBe("ITestService");
        
        _output.WriteLine($"ServiceType property returned: {actualType.Name}");
    }

    [Theory]
    [InlineData("Connection")]
    [InlineData("DataProvider")]
    [InlineData("Transformation")]
    [InlineData("Scheduling")]
    [InlineData("")]
    public void CategoryPropertyShouldReturnCorrectCategory(string category)
    {
        // Arrange
        var mockServiceType = new MockServiceType(typeof(ITestService), category);

        // Act
        var actualCategory = mockServiceType.Category;

        // Assert
        actualCategory.ShouldBe(category);
        
        _output.WriteLine($"Category property returned: '{actualCategory}'");
    }

    [Fact]
    public void CreateServiceShouldReturnServiceFromProvider()
    {
        // Arrange
        var expectedService = Mock.Of<ITestService>();
        var mockServiceType = new MockServiceType(typeof(ITestService), "Test");
        
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(ITestService)))
            .Returns(expectedService);

        // Act
        var result = mockServiceType.CreateService(_serviceProviderMock.Object);

        // Assert
        result.ShouldBe(expectedService);
        result.ShouldBeAssignableTo<ITestService>();
        
        _serviceProviderMock.Verify(sp => sp.GetService(typeof(ITestService)), Times.Once);
        
        _output.WriteLine($"CreateService returned: {result.GetType().Name}");
    }

    [Fact]
    public void CreateServiceWithNullFromProviderShouldReturnMockService()
    {
        // Arrange
        var mockServiceType = new MockServiceType(typeof(ITestService), "Test");
        
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(ITestService)))
            .Returns(null);

        // Act
        var result = mockServiceType.CreateService(_serviceProviderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeAssignableTo<ITestService>();
        
        _serviceProviderMock.Verify(sp => sp.GetService(typeof(ITestService)), Times.Once);
        
        _output.WriteLine($"CreateService with null provider returned: {result.GetType().Name}");
    }

    [Fact]
    public void RegisterServiceShouldAddServiceToCollection()
    {
        // Arrange
        var mockServiceType = new MockServiceType(typeof(ITestService), "Test");
        var services = new ServiceCollection();

        // Act
        mockServiceType.RegisterService(services);

        // Assert
        services.Count.ShouldBe(1);
        services[0].ServiceType.ShouldBe(typeof(ITestService));
        services[0].Lifetime.ShouldBe(ServiceLifetime.Singleton);
        
        _output.WriteLine($"RegisterService added {services.Count} service(s):");
        foreach (var service in services)
        {
            _output.WriteLine($"  - {service.ServiceType.Name} ({service.Lifetime})");
        }
    }

    [Fact]
    public void RegisterServiceWithNullServiceCollectionShouldThrow()
    {
        // Arrange
        var mockServiceType = new MockServiceType(typeof(ITestService), "Test");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => mockServiceType.RegisterService(null!));
        
        _output.WriteLine("RegisterService with null collection threw ArgumentNullException as expected");
    }

    [Fact]
    public void CreateServiceWithNullServiceProviderShouldThrow()
    {
        // Arrange
        var mockServiceType = new MockServiceType(typeof(ITestService), "Test");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => mockServiceType.CreateService(null!));
        
        _output.WriteLine("CreateService with null provider threw ArgumentNullException as expected");
    }

    // Test different service types
    public interface IAnotherService : IFdwService
    {
        int AnotherProperty { get; }
    }

    [Theory]
    [InlineData(typeof(ITestService))]
    [InlineData(typeof(IAnotherService))]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    public void ServiceTypePropertyShouldSupportDifferentTypes(Type serviceType)
    {
        // Arrange
        var mockServiceType = new MockServiceType(serviceType, "Test");

        // Act
        var result = mockServiceType.ServiceType;

        // Assert
        result.ShouldBe(serviceType);
        
        _output.WriteLine($"ServiceType supports type: {serviceType.Name}");
    }

    [Fact]
    public void InterfaceContractShouldBeImplementedCorrectly()
    {
        // Arrange
        var mockServiceType = new MockServiceType(typeof(ITestService), "InterfaceTest");

        // Act & Assert
        // Verify all interface members are implemented
        mockServiceType.ShouldBeAssignableTo<IServiceType>();
        
        // Verify properties are not null
        mockServiceType.ServiceType.ShouldNotBeNull();
        mockServiceType.Category.ShouldNotBeNull();
        
        // Verify methods can be called without throwing
        Should.NotThrow(() => mockServiceType.CreateService(_serviceProviderMock.Object));
        Should.NotThrow(() => mockServiceType.RegisterService(new ServiceCollection()));
        
        _output.WriteLine($"Interface contract verified for: {mockServiceType.GetType().Name}");
    }

    // Test with complex service types
    public class ComplexService : IFdwService
    {
        public string Name => "ComplexService";
        public string Id => Guid.NewGuid().ToString();
        public string ServiceType => "Complex";
        public bool IsAvailable => true;
        
        public ComplexService(string dependency1, int dependency2)
        {
            Dependency1 = dependency1;
            Dependency2 = dependency2;
        }
        
        public string Dependency1 { get; }
        public int Dependency2 { get; }
    }

    public class ComplexServiceType : IServiceType
    {
        public Type ServiceType => typeof(ComplexService);
        public string Category => "Complex";

        public object CreateService(IServiceProvider serviceProvider)
        {
            // Create service with dependencies
            return new ComplexService("TestDependency", 42);
        }

        public void RegisterService(IServiceCollection services)
        {
            services.AddSingleton<ComplexService>(provider => (ComplexService)CreateService(provider));
            services.AddSingleton<string>("TestDependency");
        }
    }

    [Fact]
    public void ComplexServiceTypeShouldWork()
    {
        // Arrange
        var complexServiceType = new ComplexServiceType();
        var services = new ServiceCollection();

        // Act
        var service = complexServiceType.CreateService(_serviceProviderMock.Object);
        complexServiceType.RegisterService(services);

        // Assert
        service.ShouldBeOfType<ComplexService>();
        var complexService = (ComplexService)service;
        complexService.Dependency1.ShouldBe("TestDependency");
        complexService.Dependency2.ShouldBe(42);
        
        services.Count.ShouldBe(2); // ComplexService and string dependency
        services.ShouldContain(s => s.ServiceType == typeof(ComplexService));
        services.ShouldContain(s => s.ServiceType == typeof(string));
        
        _output.WriteLine($"Complex service created with dependencies: {complexService.Dependency1}, {complexService.Dependency2}");
        _output.WriteLine($"Registered {services.Count} services for complex service type");
    }

    // Test implementation that demonstrates the interface flexibility
    public class FlexibleServiceType : IServiceType
    {
        private readonly Func<IServiceProvider, object> _serviceFactory;
        private readonly Action<IServiceCollection> _registrationAction;

        public Type ServiceType { get; }
        public string Category { get; }

        public FlexibleServiceType(
            Type serviceType, 
            string category, 
            Func<IServiceProvider, object> serviceFactory,
            Action<IServiceCollection> registrationAction)
        {
            ServiceType = serviceType;
            Category = category;
            _serviceFactory = serviceFactory;
            _registrationAction = registrationAction;
        }

        public object CreateService(IServiceProvider serviceProvider)
        {
            return _serviceFactory(serviceProvider);
        }

        public void RegisterService(IServiceCollection services)
        {
            _registrationAction(services);
        }
    }

    [Fact]
    public void FlexibleServiceTypeShouldAllowCustomization()
    {
        // Arrange
        var customService = Mock.Of<ITestService>(s => s.TestProperty == "CustomValue");
        var serviceCreated = false;
        var serviceRegistered = false;
        
        var flexibleServiceType = new FlexibleServiceType(
            typeof(ITestService),
            "Flexible",
            provider => { serviceCreated = true; return customService; },
            services => { serviceRegistered = true; services.AddSingleton(typeof(ITestService)); }
        );

        var services = new ServiceCollection();

        // Act
        var result = flexibleServiceType.CreateService(_serviceProviderMock.Object);
        flexibleServiceType.RegisterService(services);

        // Assert
        result.ShouldBe(customService);
        serviceCreated.ShouldBeTrue();
        serviceRegistered.ShouldBeTrue();
        services.Count.ShouldBe(1);
        
        _output.WriteLine($"Flexible service type created custom service: {serviceCreated}");
        _output.WriteLine($"Flexible service type registered service: {serviceRegistered}");
    }
}