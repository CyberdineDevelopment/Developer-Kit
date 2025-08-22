using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Services.EnhancedEnums;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

public class ServiceTypeCollectionBaseTests
{
    private readonly ITestOutputHelper _output;

    public ServiceTypeCollectionBaseTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // Test Interfaces
    public interface ITestService : IFdwService
    {
        string TestProperty { get; }
    }

    public interface ITestConfiguration : IFdwConfiguration
    {
        string TestValue { get; }
    }

    // Test ServiceType Implementation
    public class TestServiceType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public TestServiceType(int id, string name, string description = "")
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }

    // Concrete Collection Implementation for Testing
    public class TestServiceTypeCollection : ServiceTypeCollectionBase<TestServiceType, ITestService, ITestConfiguration>
    {
        static TestServiceTypeCollection()
        {
            // Register some test service types
            Register(new TestServiceType(1, "Service1", "Description1"));
            Register(new TestServiceType(2, "Service2", "Description2"));
            Register(new TestServiceType(3, "Service3", "Description3"));
        }

        // Public method to test registration functionality
        public static void RegisterTestServiceType(TestServiceType serviceType, string? name = null, int? id = null)
        {
            Register(serviceType, name, id);
        }

        // Public methods to test protected functionality
        public static string? TestExtractServiceTypeName(TestServiceType serviceType)
        {
            return ExtractServiceTypeName(serviceType);
        }

        public static int? TestExtractServiceTypeId(TestServiceType serviceType)
        {
            return ExtractServiceTypeId(serviceType);
        }
    }

    [Fact]
    public void AllPropertyShouldReturnRegisteredServiceTypes()
    {
        // Arrange & Act
        var allServices = TestServiceTypeCollection.All;

        // Assert
        allServices.IsDefaultOrEmpty.ShouldBeFalse();
        allServices.Length.ShouldBeGreaterThanOrEqualTo(3);
        allServices.Any(s => s.Name == "Service1").ShouldBeTrue();
        allServices.Any(s => s.Name == "Service2").ShouldBeTrue();
        allServices.Any(s => s.Name == "Service3").ShouldBeTrue();
        
        _output.WriteLine($"Total registered services: {allServices.Length}");
        foreach (var service in allServices.Take(5)) // Show first 5
        {
            _output.WriteLine($"Service: {service.Name} (ID: {service.Id})");
        }
    }

    [Fact]
    public void CountPropertyShouldReturnCorrectNumber()
    {
        // Arrange & Act
        var count = TestServiceTypeCollection.Count;

        // Assert
        count.ShouldBeGreaterThanOrEqualTo(3);
        count.ShouldBe(TestServiceTypeCollection.All.Length);
        
        _output.WriteLine($"Service types count: {count}");
    }

    [Theory]
    [InlineData("Service1", true)]
    [InlineData("Service2", true)]
    [InlineData("NonExistent", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void GetByNameShouldReturnCorrectResult(string? name, bool shouldExist)
    {
        // Arrange & Act
        var result = TestServiceTypeCollection.GetByName(name!);

        // Assert
        if (shouldExist)
        {
            result.ShouldNotBeNull();
            result.Name.ShouldBe(name);
            _output.WriteLine($"Found service: {result.Name} (ID: {result.Id})");
        }
        else
        {
            result.ShouldBeNull();
            _output.WriteLine($"Service not found for name: {name ?? "null"}");
        }
    }

    [Theory]
    [InlineData("Service1", true)]
    [InlineData("Service2", true)]
    [InlineData("NonExistent", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void TryGetByNameShouldReturnCorrectResult(string? name, bool shouldExist)
    {
        // Arrange & Act
        var found = TestServiceTypeCollection.TryGetByName(name!, out var serviceType);

        // Assert
        found.ShouldBe(shouldExist);
        if (shouldExist)
        {
            serviceType.ShouldNotBeNull();
            serviceType!.Name.ShouldBe(name);
            _output.WriteLine($"TryGet found service: {serviceType.Name} (ID: {serviceType.Id})");
        }
        else
        {
            serviceType.ShouldBeNull();
            _output.WriteLine($"TryGet did not find service for name: {name ?? "null"}");
        }
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(999, false)]
    public void GetByIdShouldReturnCorrectResult(int id, bool shouldExist)
    {
        // Arrange & Act
        var result = TestServiceTypeCollection.GetById(id);

        // Assert
        if (shouldExist)
        {
            result.ShouldNotBeNull();
            result.Id.ShouldBe(id);
            _output.WriteLine($"Found service by ID {id}: {result.Name}");
        }
        else
        {
            result.ShouldBeNull();
            _output.WriteLine($"Service not found for ID: {id}");
        }
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(999, false)]
    public void TryGetByIdShouldReturnCorrectResult(int id, bool shouldExist)
    {
        // Arrange & Act
        var found = TestServiceTypeCollection.TryGetById(id, out var serviceType);

        // Assert
        found.ShouldBe(shouldExist);
        if (shouldExist)
        {
            serviceType.ShouldNotBeNull();
            serviceType!.Id.ShouldBe(id);
            _output.WriteLine($"TryGet found service by ID {id}: {serviceType.Name}");
        }
        else
        {
            serviceType.ShouldBeNull();
            _output.WriteLine($"TryGet did not find service for ID: {id}");
        }
    }

    [Theory]
    [InlineData("Service1", true)]
    [InlineData("Service2", true)]
    [InlineData("NonExistent", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ContainsByNameShouldReturnCorrectResult(string? name, bool shouldContain)
    {
        // Arrange & Act
        var contains = TestServiceTypeCollection.Contains(name!);

        // Assert
        contains.ShouldBe(shouldContain);
        
        _output.WriteLine($"Contains '{name ?? "null"}': {contains}");
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(999, false)]
    public void ContainsByIdShouldReturnCorrectResult(int id, bool shouldContain)
    {
        // Arrange & Act
        var contains = TestServiceTypeCollection.Contains(id);

        // Assert
        contains.ShouldBe(shouldContain);
        
        _output.WriteLine($"Contains ID {id}: {contains}");
    }

    [Fact]
    public void RegisterWithExplicitNameAndIdShouldWork()
    {
        // Arrange
        var testService = new TestServiceType(100, "OriginalName", "Test Description");
        var explicitName = "ExplicitName";
        var explicitId = 200;

        // Act
        TestServiceTypeCollection.RegisterTestServiceType(testService, explicitName, explicitId);

        // Assert
        TestServiceTypeCollection.GetByName(explicitName).ShouldNotBeNull();
        TestServiceTypeCollection.GetById(explicitId).ShouldNotBeNull();
        TestServiceTypeCollection.Contains(explicitName).ShouldBeTrue();
        TestServiceTypeCollection.Contains(explicitId).ShouldBeTrue();
        
        _output.WriteLine($"Registered service with explicit name '{explicitName}' and ID {explicitId}");
    }

    [Fact]
    public void RegisterWithNullServiceTypeShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            TestServiceTypeCollection.RegisterTestServiceType(null!));
        
        exception.ParamName.ShouldBe("serviceType");
        
        _output.WriteLine($"Expected exception thrown: {exception.Message}");
    }

    [Fact]
    public void ExtractServiceTypeNameShouldReturnNameProperty()
    {
        // Arrange
        var serviceType = new TestServiceType(1, "TestName", "Description");

        // Act
        var extractedName = TestServiceTypeCollection.TestExtractServiceTypeName(serviceType);

        // Assert
        extractedName.ShouldBe("TestName");
        
        _output.WriteLine($"Extracted name: {extractedName}");
    }

    [Fact]
    public void ExtractServiceTypeIdShouldReturnIdProperty()
    {
        // Arrange
        var serviceType = new TestServiceType(42, "TestName", "Description");

        // Act
        var extractedId = TestServiceTypeCollection.TestExtractServiceTypeId(serviceType);

        // Assert
        extractedId.ShouldBe(42);
        
        _output.WriteLine($"Extracted ID: {extractedId}");
    }

    // Test ServiceType with no Name property
    public class ServiceTypeWithoutName
    {
        public int Id { get; set; } = 1;
    }

    // Test ServiceType with no Id property
    public class ServiceTypeWithoutId
    {
        public string Name { get; set; } = "TestName";
    }

    // Additional collection for testing edge cases
    public class EdgeCaseServiceTypeCollection : ServiceTypeCollectionBase<ServiceTypeWithoutName, ITestService, ITestConfiguration>
    {
        public static string? TestExtractServiceTypeName(ServiceTypeWithoutName serviceType)
        {
            return ExtractServiceTypeName(serviceType);
        }

        public static int? TestExtractServiceTypeId(ServiceTypeWithoutName serviceType)
        {
            return ExtractServiceTypeId(serviceType);
        }
    }

    public class EdgeCaseServiceTypeCollection2 : ServiceTypeCollectionBase<ServiceTypeWithoutId, ITestService, ITestConfiguration>
    {
        public static string? TestExtractServiceTypeName(ServiceTypeWithoutId serviceType)
        {
            return ExtractServiceTypeName(serviceType);
        }

        public static int? TestExtractServiceTypeId(ServiceTypeWithoutId serviceType)
        {
            return ExtractServiceTypeId(serviceType);
        }
    }

    [Fact]
    public void ExtractServiceTypeNameWithNoNamePropertyShouldReturnNull()
    {
        // Arrange
        var serviceType = new ServiceTypeWithoutName();

        // Act
        var extractedName = EdgeCaseServiceTypeCollection.TestExtractServiceTypeName(serviceType);

        // Assert
        extractedName.ShouldBeNull();
        
        _output.WriteLine($"Extracted name from type without Name property: {extractedName ?? "null"}");
    }

    [Fact]
    public void ExtractServiceTypeIdWithNoIdPropertyShouldReturnNull()
    {
        // Arrange
        var serviceType = new ServiceTypeWithoutId();

        // Act
        var extractedId = EdgeCaseServiceTypeCollection2.TestExtractServiceTypeId(serviceType);

        // Assert
        extractedId.ShouldBeNull();
        
        _output.WriteLine($"Extracted ID from type without Id property: {extractedId?.ToString() ?? "null"}");
    }

    [Fact]
    public void RegisterServiceTypeWithoutNameShouldThrowArgumentException()
    {
        // Arrange
        var serviceTypeWithoutName = new ServiceTypeWithoutName();

        // Create a temporary collection class to test this scenario
        var exception = Should.Throw<ArgumentException>(() =>
        {
            // Use reflection to call the protected Register method
            var method = typeof(ServiceTypeCollectionBase<ServiceTypeWithoutName, ITestService, ITestConfiguration>)
                .GetMethod("Register", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method?.Invoke(null, new object[] { serviceTypeWithoutName, null, null });
        });

        // Assert
        exception.Message.ShouldContain("Unable to determine name for service type");
        
        _output.WriteLine($"Expected exception for missing name: {exception.Message}");
    }

    [Fact]
    public void RegisterServiceTypeWithoutIdShouldThrowArgumentException()
    {
        // Arrange
        var serviceTypeWithoutId = new ServiceTypeWithoutId();

        // Create a temporary collection class to test this scenario
        var exception = Should.Throw<ArgumentException>(() =>
        {
            // Use reflection to call the protected Register method
            var method = typeof(ServiceTypeCollectionBase<ServiceTypeWithoutId, ITestService, ITestConfiguration>)
                .GetMethod("Register", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method?.Invoke(null, new object[] { serviceTypeWithoutId, null, null });
        });

        // Assert
        exception.Message.ShouldContain("Unable to determine ID for service type");
        
        _output.WriteLine($"Expected exception for missing ID: {exception.Message}");
    }

    [Fact]
    public void DuplicateRegistrationShouldNotOverrideExistingEntries()
    {
        // Arrange
        var originalService = TestServiceTypeCollection.GetByName("Service1");
        var duplicateService = new TestServiceType(999, "Service1", "Duplicate Description");

        // Act
        TestServiceTypeCollection.RegisterTestServiceType(duplicateService);

        // Assert - Original should still be there
        var retrievedService = TestServiceTypeCollection.GetByName("Service1");
        retrievedService.ShouldBe(originalService); // Should be the same reference
        retrievedService.Id.ShouldNotBe(999); // Should not have the new ID
        
        _output.WriteLine($"Duplicate registration handled - Original ID: {retrievedService?.Id}");
    }
}