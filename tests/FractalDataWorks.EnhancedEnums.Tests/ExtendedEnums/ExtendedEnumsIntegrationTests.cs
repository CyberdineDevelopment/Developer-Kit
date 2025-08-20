using System;
using System.Diagnostics.CodeAnalysis;
using FractalDataWorks.EnhancedEnums.ExtendedEnums;
using FractalDataWorks.EnhancedEnums.ExtendedEnums.Attributes;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests.ExtendedEnums;

/// <summary>
/// Integration tests demonstrating how ExtendedEnums components work together.
/// </summary>
public sealed class ExtendedEnumsIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public ExtendedEnumsIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ExtendedEnumWithAttributesCanBeCreated()
    {
        // Arrange - This demonstrates how a user would set up an extended enum with attributes
        var baseClass = new IntegrationTestStatusBase(TestStatus.Processing);
        var customOption = new CustomProcessingStatus();

        // Act & Assert
        baseClass.EnumValue.ShouldBe(TestStatus.Processing);
        baseClass.Id.ShouldBe(2);
        baseClass.Name.ShouldBe("Processing");

        customOption.EnumValue.ShouldBe(TestStatus.Pending);
        customOption.Id.ShouldBe(1);
        customOption.Name.ShouldBe("Pending");
        customOption.CustomProperty.ShouldBe("Custom Processing Logic");

        _output.WriteLine($"Base class: {baseClass.Name} (Id: {baseClass.Id})");
        _output.WriteLine($"Custom option: {customOption.Name} (Id: {customOption.Id}, Custom: {customOption.CustomProperty})");
    }

    [Fact]
    public void ExtendedEnumImplicitConversionWorks()
    {
        // Arrange
        var extendedOption = new IntegrationTestStatusBase(TestStatus.Completed);

        // Act
        TestStatus convertedValue = extendedOption;
        bool canConvertBack = CanProcessStatus(extendedOption);

        // Assert
        convertedValue.ShouldBe(TestStatus.Completed);
        canConvertBack.ShouldBeTrue();

        _output.WriteLine($"Extended enum implicitly converted to: {convertedValue}");
        _output.WriteLine($"Extended enum can be used where base enum is expected: {canConvertBack}");
    }

    [Fact]
    public void ExtendedEnumEqualityWorksAcrossTypes()
    {
        // Arrange
        var baseOption = new IntegrationTestStatusBase(TestStatus.Processing);
        var customOption = new CustomProcessingStatus();
        var anotherBaseOption = new IntegrationTestStatusBase(TestStatus.Processing);

        // Act & Assert
        baseOption.Equals(customOption).ShouldBeFalse(); // Different enum values
        baseOption.Equals(TestStatus.Processing).ShouldBeTrue(); // Equals underlying enum
        baseOption.Equals(anotherBaseOption).ShouldBeTrue(); // Same enum value

        _output.WriteLine($"Base option equals custom option (different enum values): {baseOption.Equals(customOption)}");
        _output.WriteLine($"Base option equals underlying enum: {baseOption.Equals(TestStatus.Processing)}");
        _output.WriteLine($"Base option equals another base option with same value: {baseOption.Equals(anotherBaseOption)}");
    }

    [Fact]
    public void ExtendedEnumHashCodeConsistency()
    {
        // Arrange
        var option1 = new IntegrationTestStatusBase(TestStatus.Failed);
        var option2 = new IntegrationTestStatusBase(TestStatus.Failed);
        var option3 = new IntegrationTestStatusBase(TestStatus.Completed);

        // Act
        var hash1 = option1.GetHashCode();
        var hash2 = option2.GetHashCode();
        var hash3 = option3.GetHashCode();
        var enumHash = TestStatus.Failed.GetHashCode();

        // Assert
        hash1.ShouldBe(hash2); // Same enum values have same hash
        hash1.ShouldBe(enumHash); // Matches underlying enum hash
        hash1.ShouldNotBe(hash3); // Different enum values have different hash

        _output.WriteLine($"Hash for Failed status: {hash1}");
        _output.WriteLine($"Hash for another Failed status: {hash2}");
        _output.WriteLine($"Hash for Completed status: {hash3}");
        _output.WriteLine($"Hash for underlying enum: {enumHash}");
    }

    [Fact]
    public void ExtendedEnumToStringBehavior()
    {
        // Arrange
        var baseOption = new IntegrationTestStatusBase(TestStatus.Pending);
        var customOption = new CustomProcessingStatus();

        // Act
        var baseString = baseOption.ToString();
        var customString = customOption.ToString();

        // Assert
        baseString.ShouldBe("Pending");
        customString.ShouldBe("Pending"); // Uses underlying enum's ToString
        
        baseString.ShouldBe(baseOption.Name);
        customString.ShouldBe(customOption.Name);

        _output.WriteLine($"Base option ToString(): '{baseString}'");
        _output.WriteLine($"Custom option toString(): '{customString}'");
    }

    [Theory]
    [InlineData(TestStatus.Pending, 1, "Pending")]
    [InlineData(TestStatus.Processing, 2, "Processing")]
    [InlineData(TestStatus.Completed, 3, "Completed")]
    [InlineData(TestStatus.Failed, 4, "Failed")]
    public void ExtendedEnumWorksWithAllEnumValues(TestStatus enumValue, int expectedId, string expectedName)
    {
        // Arrange
        var extendedOption = new IntegrationTestStatusBase(enumValue);

        // Act & Assert
        extendedOption.EnumValue.ShouldBe(enumValue);
        extendedOption.Id.ShouldBe(expectedId);
        extendedOption.Name.ShouldBe(expectedName);

        TestStatus converted = extendedOption;
        converted.ShouldBe(enumValue);

        _output.WriteLine($"Extended enum for {enumValue}: Id={extendedOption.Id}, Name='{extendedOption.Name}'");
    }

    [Fact]
    public void AttributeConfigurationCanBeVerified()
    {
        // Arrange - Check that the attributes are properly configured for use by generators
        var extendEnumAttribute = new ExtendEnumAttribute(typeof(TestStatus))
        {
            CollectionName = "StatusCollection",
            GenerateFactoryMethods = true,
            UseSingletonInstances = true
        };

        var optionAttribute = new ExtendedEnumOptionAttribute(
            name: "CustomStatus",
            order: 10,
            generateFactoryMethod: true);

        var globalAttribute = new GlobalExtendedEnumCollectionAttribute(typeof(TestStatus));

        // Act & Assert
        extendEnumAttribute.EnumType.ShouldBe(typeof(TestStatus));
        extendEnumAttribute.CollectionName.ShouldBe("StatusCollection");
        extendEnumAttribute.GenerateFactoryMethods.ShouldBeTrue();
        extendEnumAttribute.UseSingletonInstances.ShouldBeTrue();

        optionAttribute.Name.ShouldBe("CustomStatus");
        optionAttribute.Order.ShouldBe(10);
        optionAttribute.GenerateFactoryMethod.ShouldBe(true);

        globalAttribute.EnumType.ShouldBe(typeof(TestStatus));
        globalAttribute.CollectionName.ShouldBe("GlobalTestStatusCollection");

        _output.WriteLine("All attributes configured properly for source generator consumption");
    }

    [Fact]
    public void ExtendedEnumCanBeUsedInGenericConstraints()
    {
        // Arrange
        var option = new IntegrationTestStatusBase(TestStatus.Processing);

        // Act
        var result = ProcessExtendedEnum(option);

        // Assert
        result.ShouldBe("Processed: Processing");
        _output.WriteLine($"Generic method result: {result}");
    }

    [Fact]
    public void MultipleCustomExtendedOptionsCanCoexist()
    {
        // Arrange
        var customProcessing = new CustomProcessingStatus();
        var customCompleted = new CustomCompletedStatus();
        var baseOption = new IntegrationTestStatusBase(TestStatus.Failed);

        // Act & Assert
        customProcessing.CustomProperty.ShouldBe("Custom Processing Logic");
        customCompleted.CompletionDate.ShouldNotBe(default);
        baseOption.Name.ShouldBe("Failed");

        // They should all be different based on their enum values
        customProcessing.Equals(customCompleted).ShouldBeFalse();
        customProcessing.Equals(baseOption).ShouldBeFalse();
        customCompleted.Equals(baseOption).ShouldBeFalse();

        _output.WriteLine($"Custom processing: {customProcessing.Name} - {customProcessing.CustomProperty}");
        _output.WriteLine($"Custom completed: {customCompleted.Name} - Completed at {customCompleted.CompletionDate:yyyy-MM-dd}");
        _output.WriteLine($"Base option: {baseOption.Name}");
    }

    // Helper method to test implicit conversion
    private static bool CanProcessStatus(TestStatus status)
    {
        return status is TestStatus.Processing or TestStatus.Pending or TestStatus.Completed or TestStatus.Failed;
    }

    // Helper method to test generic constraints
    private static string ProcessExtendedEnum<T>(T extendedEnum) where T : ExtendedEnumOptionBase<T, TestStatus>
    {
        return $"Processed: {extendedEnum.Name}";
    }
}

/// <summary>
/// Test base class extending TestStatus for integration testing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Test class for integration testing - no business logic to test")]
public class IntegrationTestStatusBase : ExtendedEnumOptionBase<IntegrationTestStatusBase, TestStatus>
{
    public IntegrationTestStatusBase(TestStatus enumValue) : base(enumValue)
    {
    }
}

/// <summary>
/// Custom extended enum option for processing status.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Test class for integration testing - no business logic to test")]
public sealed class CustomProcessingStatus : IntegrationTestStatusBase
{
    public CustomProcessingStatus() : base(TestStatus.Pending)
    {
    }

    public string CustomProperty => "Custom Processing Logic";
}

/// <summary>
/// Custom extended enum option for completed status with additional data.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Test class for integration testing - no business logic to test")]
public sealed class CustomCompletedStatus : IntegrationTestStatusBase
{
    public CustomCompletedStatus() : base(TestStatus.Completed)
    {
        CompletionDate = DateTime.UtcNow;
    }

    public DateTime CompletionDate { get; }
}