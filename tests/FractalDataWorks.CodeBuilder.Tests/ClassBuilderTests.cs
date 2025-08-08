using System;
using System.Linq;
using Shouldly;
using Xunit;
using FractalDataWorks.CodeBuilder.Builders;
using FractalDataWorks.CodeBuilder.Abstractions;
using Moq;

namespace FractalDataWorks.CodeBuilder.Tests;

public class ClassBuilderTests
{
    private static readonly string[] NewLineSeparators = ["\r\n", "\n"];
    [Fact]
    public void DefaultClassDeclaration()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe("public class MyClass\r\n{\r\n}");
    }

    [Fact]
    public void ClassWithCustomName()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder.WithName("CustomClass").Build();

        // Assert
        result.ShouldContain("public class CustomClass");
    }

    [Fact]
    public void ClassWithNamespace()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithNamespace("MyNamespace")
            .WithName("TestClass")
            .Build();

        // Assert
        result.ShouldStartWith("namespace MyNamespace;\r\n\r\npublic class TestClass");
    }

    [Fact]
    public void ClassWithUsings()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithUsings("System", "System.Text")
            .WithName("TestClass")
            .Build();

        // Assert
        result.ShouldStartWith("using System;\r\nusing System.Text;\r\n\r\npublic class TestClass");
    }

    [Fact]
    public void ClassWithUsingsOrdersAlphabetically()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithUsings("System.Text", "System", "System.Linq")
            .WithName("TestClass")
            .Build();

        // Assert
        result.ShouldStartWith("using System;\r\nusing System.Linq;\r\nusing System.Text;\r\n\r\npublic class TestClass");
    }

    [Fact]
    public void ClassWithDuplicateUsingsRemovesDuplicates()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithUsings("System", "System.Text", "System")
            .WithName("TestClass")
            .Build();

        // Assert
        var usingCount = result.Split('\n').Where(line => line.StartsWith("using ", StringComparison.Ordinal)).Count();
        usingCount.ShouldBe(2);
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    public void ClassWithAccessModifier(string accessModifier)
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithAccessModifier(accessModifier)
            .WithName("TestClass")
            .Build();

        // Assert
        result.ShouldContain($"{accessModifier} class TestClass");
    }

    [Fact]
    public void StaticClass()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("StaticClass")
            .AsStatic()
            .Build();

        // Assert
        result.ShouldContain("public static class StaticClass");
    }

    [Fact]
    public void AbstractClass()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("AbstractClass")
            .AsAbstract()
            .Build();

        // Assert
        result.ShouldContain("public abstract class AbstractClass");
    }

    [Fact]
    public void SealedClass()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("SealedClass")
            .AsSealed()
            .Build();

        // Assert
        result.ShouldContain("public sealed class SealedClass");
    }

    [Fact]
    public void PartialClass()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("PartialClass")
            .AsPartial()
            .Build();

        // Assert
        result.ShouldContain("public partial class PartialClass");
    }

    [Fact]
    public void AbstractAndSealedMutuallyExclusive()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("TestClass")
            .AsAbstract()
            .AsSealed()
            .Build();

        // Assert
        result.ShouldContain("public sealed class TestClass");
        result.ShouldNotContain("abstract");
    }

    [Fact]
    public void SealedAndAbstractMutuallyExclusive()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("TestClass")
            .AsSealed()
            .AsAbstract()
            .Build();

        // Assert
        result.ShouldContain("public abstract class TestClass");
        result.ShouldNotContain("sealed");
    }

    [Fact]
    public void ClassWithBaseClass()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("ChildClass")
            .WithBaseClass("BaseClass")
            .Build();

        // Assert
        result.ShouldContain("public class ChildClass : BaseClass");
    }

    [Fact]
    public void ClassWithInterfaces()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("TestClass")
            .WithInterfaces("IInterface1", "IInterface2")
            .Build();

        // Assert
        result.ShouldContain("public class TestClass : IInterface1, IInterface2");
    }

    [Fact]
    public void ClassWithBaseClassAndInterfaces()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("TestClass")
            .WithBaseClass("BaseClass")
            .WithInterfaces("IInterface1", "IInterface2")
            .Build();

        // Assert
        result.ShouldContain("public class TestClass : BaseClass, IInterface1, IInterface2");
    }

    [Fact]
    public void GenericClass()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("GenericClass")
            .WithGenericParameters("T")
            .Build();

        // Assert
        result.ShouldContain("public class GenericClass<T>");
    }

    [Fact]
    public void GenericClassWithMultipleParameters()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("GenericClass")
            .WithGenericParameters("T", "U", "V")
            .Build();

        // Assert
        result.ShouldContain("public class GenericClass<T, U, V>");
    }

    [Fact]
    public void GenericClassWithConstraints()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("GenericClass")
            .WithGenericParameters("T")
            .WithGenericConstraint("T", "class")
            .Build();

        // Assert
        result.ShouldContain("public class GenericClass<T>");
        result.ShouldContain("    where T : class");
    }

    [Fact]
    public void GenericClassWithMultipleConstraints()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("GenericClass")
            .WithGenericParameters("T", "U")
            .WithGenericConstraint("T", "class", "new()")
            .WithGenericConstraint("U", "struct")
            .Build();

        // Assert
        result.ShouldContain("public class GenericClass<T, U>");
        result.ShouldContain("    where T : class, new()");
        result.ShouldContain("    where U : struct");
    }

    [Fact]
    public void ClassWithAttribute()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("TestClass")
            .WithAttribute("Serializable")
            .Build();

        // Assert
        result.ShouldContain("[Serializable]");
        result.ShouldContain("public class TestClass");
    }

    [Fact]
    public void ClassWithMultipleAttributes()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("TestClass")
            .WithAttribute("Serializable")
            .WithAttribute("DataContract")
            .Build();

        // Assert
        result.ShouldContain("[Serializable]");
        result.ShouldContain("[DataContract]");
    }

    [Fact]
    public void ClassWithXmlDocumentation()
    {
        // Arrange
        var builder = new ClassBuilder();
        var summary = "This is a test class";

        // Act
        var result = builder
            .WithName("TestClass")
            .WithXmlDoc(summary)
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain($"/// {summary}");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void ClassWithMultilineXmlDocumentation()
    {
        // Arrange
        var builder = new ClassBuilder();
        var summary = "This is a test class\nwith multiple lines";

        // Act
        var result = builder
            .WithName("TestClass")
            .WithXmlDoc(summary)
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// This is a test class");
        result.ShouldContain("/// with multiple lines");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void ClassWithField()
    {
        // Arrange
        var builder = new ClassBuilder();
        var mockField = new Mock<IFieldBuilder>();
        mockField.Setup(f => f.Build()).Returns("private string _field;");

        // Act
        var result = builder
            .WithName("TestClass")
            .WithField(mockField.Object)
            .Build();

        // Assert
        result.ShouldContain("private string _field;");
    }

    [Fact]
    public void ClassWithProperty()
    {
        // Arrange
        var builder = new ClassBuilder();
        var mockProperty = new Mock<IPropertyBuilder>();
        mockProperty.Setup(p => p.Build()).Returns("public string Property { get; set; }");

        // Act
        var result = builder
            .WithName("TestClass")
            .WithProperty(mockProperty.Object)
            .Build();

        // Assert
        result.ShouldContain("public string Property { get; set; }");
    }

    [Fact]
    public void ClassWithMethod()
    {
        // Arrange
        var builder = new ClassBuilder();
        var mockMethod = new Mock<IMethodBuilder>();
        mockMethod.Setup(m => m.Build()).Returns("public void Method() { }");

        // Act
        var result = builder
            .WithName("TestClass")
            .WithMethod(mockMethod.Object)
            .Build();

        // Assert
        result.ShouldContain("public void Method() { }");
    }

    [Fact]
    public void ClassWithConstructor()
    {
        // Arrange
        var builder = new ClassBuilder();
        var mockConstructor = new Mock<IConstructorBuilder>();
        mockConstructor.Setup(c => c.Build()).Returns("public TestClass() { }");

        // Act
        var result = builder
            .WithName("TestClass")
            .WithConstructor(mockConstructor.Object)
            .Build();

        // Assert
        result.ShouldContain("public TestClass() { }");
    }

    [Fact]
    public void ClassWithNestedClass()
    {
        // Arrange
        var builder = new ClassBuilder();
        var mockNestedClass = new Mock<IClassBuilder>();
        mockNestedClass.Setup(c => c.Build()).Returns("public class NestedClass { }");

        // Act
        var result = builder
            .WithName("OuterClass")
            .WithNestedClass(mockNestedClass.Object)
            .Build();

        // Assert
        result.ShouldContain("public class NestedClass { }");
    }

    [Fact]
    public void CompleteClassExample()
    {
        // Arrange
        var builder = new ClassBuilder();
        var mockField = new Mock<IFieldBuilder>();
        var mockProperty = new Mock<IPropertyBuilder>();
        var mockMethod = new Mock<IMethodBuilder>();
        
        mockField.Setup(f => f.Build()).Returns("private readonly string _value");
        mockProperty.Setup(p => p.Build()).Returns("public string Value { get; private set; }");
        mockMethod.Setup(m => m.Build()).Returns("public void DoSomething()\n{\n    // Implementation\n}");

        // Act
        var result = builder
            .WithUsings("System", "System.Text")
            .WithNamespace("MyApp.Models")
            .WithXmlDoc("A complete example class")
            .WithAttribute("Serializable")
            .WithName("ExampleClass")
            .WithAccessModifier("public")
            .AsPartial()
            .WithBaseClass("BaseClass")
            .WithInterfaces("IExample", "IDisposable")
            .WithGenericParameters("T")
            .WithGenericConstraint("T", "class", "new()")
            .WithField(mockField.Object)
            .WithProperty(mockProperty.Object)
            .WithMethod(mockMethod.Object)
            .Build();

        // Assert
        result.ShouldStartWith("using System;\r\nusing System.Text;\r\n\r\nnamespace MyApp.Models;");
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// A complete example class");
        result.ShouldContain("/// </summary>");
        result.ShouldContain("[Serializable]");
        result.ShouldContain("public partial class ExampleClass<T> : BaseClass, IExample, IDisposable");
        result.ShouldContain("    where T : class, new()");
        result.ShouldContain("private readonly string _value");
        result.ShouldContain("public string Value { get; private set; }");
        result.ShouldContain("public void DoSomething()");
    }

    [Fact]
    public void EmptyClassStructure()
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithName("EmptyClass")
            .Build();

        // Assert
        var lines = result.Split(NewLineSeparators, StringSplitOptions.None);
        lines[0].ShouldBe("public class EmptyClass");
        lines[1].ShouldBe("{");
        lines[2].ShouldBe("}");
        lines.Length.ShouldBe(3);
    }

    [Fact]
    public void ClassWithOnlyFields()
    {
        // Arrange
        var builder = new ClassBuilder();
        var mockField1 = new Mock<IFieldBuilder>();
        var mockField2 = new Mock<IFieldBuilder>();
        
        mockField1.Setup(f => f.Build()).Returns("private string _field1");
        mockField2.Setup(f => f.Build()).Returns("private int _field2");

        // Act
        var result = builder
            .WithName("TestClass")
            .WithField(mockField1.Object)
            .WithField(mockField2.Object)
            .Build();

        // Assert
        result.ShouldContain("private string _field1");
        result.ShouldContain("private int _field2");
        // Should not have extra spacing after fields when no other members exist
        result.ShouldNotContain("private int _field2\r\n\r\n}");
    }

    [Fact]
    public void FieldsAndOtherMembersHaveProperSpacing()
    {
        // Arrange
        var builder = new ClassBuilder();
        var mockField = new Mock<IFieldBuilder>();
        var mockProperty = new Mock<IPropertyBuilder>();
        
        mockField.Setup(f => f.Build()).Returns("private string _field");
        mockProperty.Setup(p => p.Build()).Returns("public string Property { get; set; }");

        // Act
        var result = builder
            .WithName("TestClass")
            .WithField(mockField.Object)
            .WithProperty(mockProperty.Object)
            .Build();

        // Assert
        result.ShouldContain("private string _field\r\n\r\n    public string Property { get; set; }");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WithNamespaceHandlesEmptyValues(string? namespaceValue)
    {
        // Arrange
        var builder = new ClassBuilder();

        // Act
        var result = builder
            .WithNamespace(namespaceValue!)
            .WithName("TestClass")
            .Build();

        // Assert
        result.ShouldNotContain("namespace");
        result.ShouldStartWith("public class TestClass");
    }
}