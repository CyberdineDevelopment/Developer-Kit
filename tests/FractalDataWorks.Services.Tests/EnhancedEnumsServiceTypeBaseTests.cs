using System;
using FractalDataWorks;
using FractalDataWorks.Services;
using FractalDataWorks.Services.EnhancedEnums;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

public class EnhancedEnumsServiceTypeBaseTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceCollection> _servicesMock;

    public EnhancedEnumsServiceTypeBaseTests(ITestOutputHelper output)
    {
        _output = output;
        _serviceProviderMock = new Mock<IServiceProvider>();
        _servicesMock = new Mock<IServiceCollection>();
    }

    // Test Interfaces
    public interface ITestService : IFdwService
    {
        string TestProperty { get; }
    }

    public class TestFactory
    {
        public ITestService CreateService() => Mock.Of<ITestService>();
    }

    public class TestConfiguration
    {
        public string Value { get; set; } = string.Empty;
    }

    // Concrete Implementation of Enhanced Enums ServiceTypeBase
    public class ConcreteEnhancedServiceType : ServiceTypeBase<ITestService, TestFactory, TestConfiguration>
    {
        public ConcreteEnhancedServiceType(int id, string name, string description, Type serviceType, Type configurationType, string category)
            : base(id, name, description, serviceType, configurationType, category)
        {
        }

        public override ITestService CreateService(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<ITestService>() ?? Mock.Of<ITestService>();
        }

        public override void RegisterService(IServiceCollection services)
        {
            services.AddSingleton<ITestService>(provider => CreateService(provider));
            services.AddSingleton<TestFactory>(new TestFactory());
        }

        public override TestFactory Factory()
        {
            return new TestFactory();
        }
    }

    [Fact]
    public void ConstructorShouldInitializeAllProperties()
    {
        // Arrange
        var id = 1;
        var name = "TestServiceType";
        var description = "Test service type description";
        var serviceType = typeof(ITestService);
        var configurationType = typeof(TestConfiguration);
        var category = "TestCategory";

        // Act
        var serviceTypeBase = new ConcreteEnhancedServiceType(id, name, description, serviceType, configurationType, category);

        // Assert
        serviceTypeBase.Id.ShouldBe(id);
        serviceTypeBase.Name.ShouldBe(name);
        serviceTypeBase.Description.ShouldBe(description);
        serviceTypeBase.ServiceType.ShouldBe(serviceType);
        serviceTypeBase.ConfigurationType.ShouldBe(configurationType);
        serviceTypeBase.Category.ShouldBe(category);
        
        _output.WriteLine($"ServiceTypeBase created - ID: {serviceTypeBase.Id}, Name: {serviceTypeBase.Name}");
        _output.WriteLine($"ServiceType: {serviceTypeBase.ServiceType.Name}, ConfigurationType: {serviceTypeBase.ConfigurationType.Name}");
        _output.WriteLine($"Category: {serviceTypeBase.Category}, Description: {serviceTypeBase.Description}");
    }

    [Fact]
    public void CreateServiceShouldCallOverriddenMethod()
    {
        // Arrange
        var serviceTypeBase = new ConcreteEnhancedServiceType(
            1, "TestService", "Description", 
            typeof(ITestService), typeof(TestConfiguration), "Test");
        
        var expectedService = Mock.Of<ITestService>();
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(ITestService)))
            .Returns(expectedService);

        // Act
        var result = serviceTypeBase.CreateService(_serviceProviderMock.Object);

        // Assert
        result.ShouldBe(expectedService);
        _serviceProviderMock.Verify(sp => sp.GetService(typeof(ITestService)), Times.Once);
        
        _output.WriteLine($"CreateService returned: {result.GetType().Name}");
    }

    [Fact]
    public void CreateServiceWithNullFromServiceProviderShouldReturnMockService()
    {
        // Arrange
        var serviceTypeBase = new ConcreteEnhancedServiceType(
            1, "TestService", "Description", 
            typeof(ITestService), typeof(TestConfiguration), "Test");
        
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(ITestService)))
            .Returns(null);

        // Act
        var result = serviceTypeBase.CreateService(_serviceProviderMock.Object);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeAssignableTo<ITestService>();
        
        _output.WriteLine($"CreateService with null from provider returned: {result.GetType().Name}");
    }

    [Fact]
    public void RegisterServiceShouldCallOverriddenMethod()
    {
        // Arrange
        var serviceTypeBase = new ConcreteEnhancedServiceType(
            1, "TestService", "Description", 
            typeof(ITestService), typeof(TestConfiguration), "Test");

        var services = new ServiceCollection();

        // Act
        serviceTypeBase.RegisterService(services);

        // Assert
        services.Count.ShouldBe(2); // Should register ITestService and TestFactory
        services.ShouldContain(s => s.ServiceType == typeof(ITestService));
        services.ShouldContain(s => s.ServiceType == typeof(TestFactory));
        
        _output.WriteLine($"RegisterService added {services.Count} services:");
        foreach (var service in services)
        {
            _output.WriteLine($"  - {service.ServiceType.Name} ({service.Lifetime})");
        }
    }

    [Fact]
    public void FactoryShouldReturnFactoryInstance()
    {
        // Arrange
        var serviceTypeBase = new ConcreteEnhancedServiceType(
            1, "TestService", "Description", 
            typeof(ITestService), typeof(TestConfiguration), "Test");

        // Act
        var factory = serviceTypeBase.Factory();

        // Assert
        factory.ShouldNotBeNull();
        factory.ShouldBeAssignableTo<TestFactory>();
        
        _output.WriteLine($"Factory returned: {factory.GetType().Name}");
    }

    [Fact]
    public void IServiceTypeCreateServiceShouldDelegateToTypedMethod()
    {
        // Arrange
        var serviceTypeBase = new ConcreteEnhancedServiceType(
            1, "TestService", "Description", 
            typeof(ITestService), typeof(TestConfiguration), "Test");
        
        var expectedService = Mock.Of<ITestService>();
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(ITestService)))
            .Returns(expectedService);

        var iServiceType = (IServiceType)serviceTypeBase;

        // Act
        var result = iServiceType.CreateService(_serviceProviderMock.Object);

        // Assert
        result.ShouldBe(expectedService);
        result.ShouldBeAssignableTo<ITestService>();
        
        _output.WriteLine($"IServiceType.CreateService returned: {result.GetType().Name}");
    }

    [Fact]
    public void InheritanceFromEnumOptionBaseShouldWork()
    {
        // Arrange & Act
        var serviceTypeBase = new ConcreteEnhancedServiceType(
            42, "InheritanceTest", "Testing inheritance", 
            typeof(ITestService), typeof(TestConfiguration), "Inheritance");

        // Assert
        // Should inherit from EnumOptionBase functionality
        serviceTypeBase.ShouldBeAssignableTo<FractalDataWorks.EnhancedEnums.EnumOptionBase<ConcreteEnhancedServiceType>>();
        serviceTypeBase.ShouldBeAssignableTo<IServiceType>();
        
        // EnumOptionBase properties should work
        serviceTypeBase.Id.ShouldBe(42);
        serviceTypeBase.Name.ShouldBe("InheritanceTest");
        
        _output.WriteLine($"Inheritance test passed - ID: {serviceTypeBase.Id}, Name: {serviceTypeBase.Name}");
        _output.WriteLine($"Is EnumOptionBase: {serviceTypeBase is FractalDataWorks.EnhancedEnums.EnumOptionBase<ConcreteEnhancedServiceType>}");
        _output.WriteLine($"Is IServiceType: {serviceTypeBase is IServiceType}");
    }

    [Theory]
    [InlineData(typeof(ITestService), "ITestService")]
    [InlineData(typeof(string), "String")]
    [InlineData(typeof(int), "Int32")]
    public void ServiceTypePropertyShouldReflectPassedType(Type expectedType, string expectedName)
    {
        // Arrange & Act
        var serviceTypeBase = new ConcreteEnhancedServiceType(
            1, "TypeTest", "Testing service type property", 
            expectedType, typeof(TestConfiguration), "TypeTest");

        // Assert
        serviceTypeBase.ServiceType.ShouldBe(expectedType);
        serviceTypeBase.ServiceType.Name.ShouldBe(expectedName);
        
        _output.WriteLine($"ServiceType property test - Type: {serviceTypeBase.ServiceType.Name}");
    }

    [Theory]
    [InlineData(typeof(TestConfiguration), "TestConfiguration")]
    [InlineData(typeof(string), "String")]
    [InlineData(typeof(object), "Object")]
    public void ConfigurationTypePropertyShouldReflectPassedType(Type expectedType, string expectedName)
    {
        // Arrange & Act
        var serviceTypeBase = new ConcreteEnhancedServiceType(
            1, "ConfigTest", "Testing configuration type property", 
            typeof(ITestService), expectedType, "ConfigTest");

        // Assert
        serviceTypeBase.ConfigurationType.ShouldBe(expectedType);
        serviceTypeBase.ConfigurationType.Name.ShouldBe(expectedName);
        
        _output.WriteLine($"ConfigurationType property test - Type: {serviceTypeBase.ConfigurationType.Name}");
    }

    [Theory]
    [InlineData("Connection")]
    [InlineData("DataProvider")]
    [InlineData("Transformation")]
    [InlineData("Scheduling")]
    [InlineData("")]
    [InlineData(null)]
    public void CategoryPropertyShouldReflectPassedValue(string? category)
    {
        // Arrange & Act
        var serviceTypeBase = new ConcreteEnhancedServiceType(
            1, "CategoryTest", "Testing category property", 
            typeof(ITestService), typeof(TestConfiguration), category!);

        // Assert
        serviceTypeBase.Category.ShouldBe(category ?? string.Empty);
        
        _output.WriteLine($"Category property test - Category: '{serviceTypeBase.Category ?? "null"}'");
    }

    [Fact]
    public void MultipleInstancesShouldHaveDistinctIdentities()
    {
        // Arrange & Act
        var service1 = new ConcreteEnhancedServiceType(
            1, "Service1", "First service", 
            typeof(ITestService), typeof(TestConfiguration), "Category1");
        
        var service2 = new ConcreteEnhancedServiceType(
            2, "Service2", "Second service", 
            typeof(ITestService), typeof(TestConfiguration), "Category2");

        // Assert
        service1.ShouldNotBe(service2);
        service1.Id.ShouldNotBe(service2.Id);
        service1.Name.ShouldNotBe(service2.Name);
        service1.Description.ShouldNotBe(service2.Description);
        service1.Category.ShouldNotBe(service2.Category);
        
        // But ServiceType and ConfigurationType should be the same
        service1.ServiceType.ShouldBe(service2.ServiceType);
        service1.ConfigurationType.ShouldBe(service2.ConfigurationType);
        
        _output.WriteLine($"Service1: ID={service1.Id}, Name={service1.Name}, Category={service1.Category}");
        _output.WriteLine($"Service2: ID={service2.Id}, Name={service2.Name}, Category={service2.Category}");
    }

    // Test with different generic parameters
    public interface IAnotherService : IFdwService
    {
        int AnotherProperty { get; }
    }

    public class AnotherFactory
    {
        public IAnotherService CreateService() => Mock.Of<IAnotherService>();
    }

    public class AnotherConfiguration
    {
        public int Value { get; set; }
    }

    public class AnotherConcreteServiceType : ServiceTypeBase<IAnotherService, AnotherFactory, AnotherConfiguration>
    {
        public AnotherConcreteServiceType() : base(99, "AnotherService", "Another service type", 
            typeof(IAnotherService), typeof(AnotherConfiguration), "AnotherCategory")
        {
        }

        public override IAnotherService CreateService(IServiceProvider serviceProvider)
        {
            return Mock.Of<IAnotherService>();
        }

        public override void RegisterService(IServiceCollection services)
        {
            services.AddSingleton<IAnotherService>(provider => CreateService(provider));
        }

        public override AnotherFactory Factory()
        {
            return new AnotherFactory();
        }
    }

    [Fact]
    public void DifferentGenericParametersShouldWorkCorrectly()
    {
        // Arrange & Act
        var anotherService = new AnotherConcreteServiceType();

        // Assert
        anotherService.ServiceType.ShouldBe(typeof(IAnotherService));
        anotherService.ConfigurationType.ShouldBe(typeof(AnotherConfiguration));
        anotherService.Name.ShouldBe("AnotherService");
        anotherService.Category.ShouldBe("AnotherCategory");
        
        var service = anotherService.CreateService(_serviceProviderMock.Object);
        service.ShouldNotBeNull();
        service.ShouldBeAssignableTo<IAnotherService>();
        
        var factory = anotherService.Factory();
        factory.ShouldNotBeNull();
        factory.ShouldBeOfType<AnotherFactory>();
        
        _output.WriteLine($"Different generic parameters test passed - Service: {anotherService.ServiceType.Name}");
        _output.WriteLine($"Configuration: {anotherService.ConfigurationType.Name}, Factory: {factory.GetType().Name}");
    }
}