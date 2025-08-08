using System;
using Shouldly;
using Xunit;
using FractalDataWorks.CodeBuilder.Builders;

namespace FractalDataWorks.CodeBuilder.Tests;

public class PropertyBuilderTests
{
    [Fact]
    public void DefaultPropertyDeclaration()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe("public object Property { get; set; }");
    }

    [Fact]
    public void PropertyWithCustomName()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder.WithName("CustomProperty").Build();

        // Assert
        result.ShouldContain("public object CustomProperty { get; set; }");
    }

    [Fact]
    public void PropertyWithType()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("Name")
            .WithType("string")
            .Build();

        // Assert
        result.ShouldContain("public string Name { get; set; }");
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    public void PropertyWithAccessModifier(string accessModifier)
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithAccessModifier(accessModifier)
            .WithName("TestProperty")
            .Build();

        // Assert
        result.ShouldContain($"{accessModifier} object TestProperty {{ get; set; }}");
    }

    [Fact]
    public void StaticProperty()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("StaticProperty")
            .AsStatic()
            .Build();

        // Assert
        result.ShouldContain("public static object StaticProperty { get; set; }");
    }

    [Fact]
    public void VirtualProperty()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("VirtualProperty")
            .AsVirtual()
            .Build();

        // Assert
        result.ShouldContain("public virtual object VirtualProperty { get; set; }");
    }

    [Fact]
    public void OverrideProperty()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("OverrideProperty")
            .AsOverride()
            .Build();

        // Assert
        result.ShouldContain("public override object OverrideProperty { get; set; }");
    }

    [Fact]
    public void AbstractProperty()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("AbstractProperty")
            .AsAbstract()
            .Build();

        // Assert
        result.ShouldContain("public abstract object AbstractProperty { get; set; }");
    }

    [Fact]
    public void VirtualAndOverrideMutuallyExclusive()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .AsVirtual()
            .AsOverride()
            .Build();

        // Assert
        result.ShouldContain("public override object TestProperty { get; set; }");
        result.ShouldNotContain("virtual");
    }

    [Fact]
    public void OverrideAndVirtualMutuallyExclusive()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .AsOverride()
            .AsVirtual()
            .Build();

        // Assert
        result.ShouldContain("public virtual object TestProperty { get; set; }");
        result.ShouldNotContain("override");
    }

    [Fact]
    public void ReadOnlyProperty()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("ReadOnlyProperty")
            .AsReadOnly()
            .Build();

        // Assert
        result.ShouldContain("public object ReadOnlyProperty { get; }");
    }

    [Fact]
    public void WriteOnlyProperty()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("WriteOnlyProperty")
            .AsWriteOnly()
            .Build();

        // Assert
        result.ShouldContain("public object WriteOnlyProperty { set; }");
    }

    [Fact]
    public void ReadOnlyAndWriteOnlyMutuallyExclusive()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .AsReadOnly()
            .AsWriteOnly()
            .Build();

        // Assert
        result.ShouldContain("public object TestProperty { set; }");
        result.ShouldNotContain("get");
    }

    [Fact]
    public void WriteOnlyAndReadOnlyMutuallyExclusive()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .AsWriteOnly()
            .AsReadOnly()
            .Build();

        // Assert
        result.ShouldContain("public object TestProperty { get; }");
        result.ShouldNotContain("set");
    }

    [Fact]
    public void PropertyWithInitSetter()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("InitProperty")
            .WithInitSetter()
            .Build();

        // Assert
        result.ShouldContain("public object InitProperty { get; init; }");
    }

    [Fact]
    public void InitSetterClearsReadOnly()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .AsReadOnly()
            .WithInitSetter()
            .Build();

        // Assert
        result.ShouldContain("public object TestProperty { get; init; }");
    }

    [Fact]
    public void InitSetterClearsSetter()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithSetter("_value = value;")
            .WithInitSetter()
            .Build();

        // Assert
        result.ShouldContain("public object TestProperty { get; init; }");
        result.ShouldNotContain("_value = value;");
    }

    [Fact]
    public void PropertyWithCustomGetter()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithType("string")
            .WithGetter("return _value?.ToUpper();")
            .Build();

        // Assert
        result.ShouldContain("public string TestProperty");
        result.ShouldContain("{\r\n    get\r\n    {\r\n        return _value?.ToUpper();\r\n    }\r\n    set;\r\n}");
    }

    [Fact]
    public void PropertyWithCustomSetter()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithType("string")
            .WithSetter("_value = value?.Trim();")
            .Build();

        // Assert
        result.ShouldContain("public string TestProperty");
        result.ShouldContain("{\r\n    get;\r\n    set\r\n    {\r\n        _value = value?.Trim();\r\n    }\r\n}");
    }

    [Fact]
    public void PropertyWithCustomGetterAndSetter()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithType("string")
            .WithGetter("return _value?.ToUpper();")
            .WithSetter("_value = value?.Trim();")
            .Build();

        // Assert
        result.ShouldContain("public string TestProperty");
        result.ShouldContain("{\r\n    get\r\n    {\r\n        return _value?.ToUpper();\r\n    }\r\n    set\r\n    {\r\n        _value = value?.Trim();\r\n    }\r\n}");
    }

    [Fact]
    public void PropertyWithGetterAccessModifier()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithGetterAccessModifier("private")
            .Build();

        // Assert
        result.ShouldContain("public object TestProperty { private get; set; }");
    }

    [Fact]
    public void PropertyWithSetterAccessModifier()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithSetterAccessModifier("private")
            .Build();

        // Assert
        result.ShouldContain("public object TestProperty { get; private set; }");
    }

    [Fact]
    public void PropertyWithBothAccessModifiers()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithGetterAccessModifier("protected")
            .WithSetterAccessModifier("private")
            .Build();

        // Assert
        result.ShouldContain("public object TestProperty { protected get; private set; }");
    }

    [Fact]
    public void PropertyWithAccessModifiersAndCustomAccessors()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithType("string")
            .WithGetterAccessModifier("protected")
            .WithSetterAccessModifier("private")
            .WithGetter("return _value;")
            .WithSetter("_value = value;")
            .Build();

        // Assert
        result.ShouldContain("public string TestProperty");
        result.ShouldContain("{\r\n    protected get\r\n    {\r\n        return _value;\r\n    }\r\n    private set\r\n    {\r\n        _value = value;\r\n    }\r\n}");
    }

    [Fact]
    public void PropertyWithInitializer()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithType("int")
            .WithInitializer("42")
            .Build();

        // Assert
        result.ShouldContain("public int TestProperty { get; set; } = 42;");
    }

    [Fact]
    public void PropertyWithInitializerAndCustomAccessors()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithType("string")
            .WithGetter("return _value;")
            .WithSetter("_value = value;")
            .WithInitializer("\"default\"")
            .Build();

        // Assert
        result.ShouldContain("public string TestProperty");
        result.ShouldContain("} = \"default\";");
    }

    [Fact]
    public void PropertyWithExpressionBody()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("FullName")
            .WithType("string")
            .WithExpressionBody("$\"{FirstName} {LastName}\"")
            .Build();

        // Assert
        result.ShouldBe("public string FullName => $\"{FirstName} {LastName}\";");
    }

    [Fact]
    public void PropertyWithAttribute()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithAttribute("JsonPropertyName(\"test_property\")")
            .Build();

        // Assert
        result.ShouldContain("[JsonPropertyName(\"test_property\")]");
        result.ShouldContain("public object TestProperty { get; set; }");
    }

    [Fact]
    public void PropertyWithMultipleAttributes()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithAttribute("Required")
            .WithAttribute("MaxLength(50)")
            .Build();

        // Assert
        result.ShouldContain("[Required]");
        result.ShouldContain("[MaxLength(50)]");
    }

    [Fact]
    public void PropertyWithXmlDocumentation()
    {
        // Arrange
        var builder = new PropertyBuilder();
        var summary = "Gets or sets the test value";

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithXmlDoc(summary)
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain($"/// {summary}");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void PropertyWithMultilineXmlDocumentation()
    {
        // Arrange
        var builder = new PropertyBuilder();
        var summary = "Gets or sets the test value\nThis property has multiple lines";

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithXmlDoc(summary)
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Gets or sets the test value");
        result.ShouldContain("/// This property has multiple lines");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void ReadOnlyPropertyWithCustomGetterClearsWriteOnly()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .AsWriteOnly()
            .WithGetter("return _value;")
            .Build();

        // Assert
        result.ShouldContain("get\r\n    {\r\n        return _value;\r\n    }");
        result.ShouldContain("set;");
    }

    [Fact]
    public void WriteOnlyPropertyWithCustomSetterClearsReadOnly()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .AsReadOnly()
            .WithSetter("_value = value;")
            .Build();

        // Assert
        result.ShouldContain("get;");
        result.ShouldContain("set\r\n    {\r\n        _value = value;\r\n    }");
    }

    [Fact]
    public void AbstractPropertyDoesNotHaveInitializer()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("AbstractProperty")
            .AsAbstract()
            .WithInitializer("should be ignored")
            .Build();

        // Assert
        result.ShouldBe("public abstract object AbstractProperty { get; set; }");
        result.ShouldNotContain("=");
        result.ShouldNotContain("should be ignored");
    }

    [Fact]
    public void CompletePropertyExample()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithXmlDoc("Gets or sets the user's full name")
            .WithAttribute("Required")
            .WithAttribute("MaxLength(100)")
            .WithAccessModifier("public")
            .AsVirtual()
            .WithType("string")
            .WithName("FullName")
            .WithGetterAccessModifier("protected")
            .WithSetterAccessModifier("private")
            .WithGetter("return $\"{FirstName} {LastName}\";")
            .WithSetter("var parts = value.Split(' ');\nFirstName = parts[0];\nLastName = parts.Length > 1 ? parts[1] : string.Empty;")
            .WithInitializer("string.Empty")
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Gets or sets the user's full name");
        result.ShouldContain("/// </summary>");
        result.ShouldContain("[Required]");
        result.ShouldContain("[MaxLength(100)]");
        result.ShouldContain("public virtual string FullName");
        result.ShouldContain("{\r\n    protected get\r\n    {\r\n        return $\"{FirstName} {LastName}\";\r\n    }");
        result.ShouldContain("private set\r\n    {\r\n        var parts = value.Split(' ');\r\n        FirstName = parts[0];\r\n        LastName = parts.Length > 1 ? parts[1] : string.Empty;\r\n    }");
        result.ShouldContain("} = string.Empty;");
    }

    [Fact]
    public void PropertyWithGetterAccessModifierInAutoProperty()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithGetterAccessModifier("private")
            .WithInitializer("42")
            .Build();

        // Assert
        result.ShouldContain("public object TestProperty { private get; set; } = 42;");
    }

    [Fact]
    public void PropertyWithSetterAccessModifierInAutoProperty()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithSetterAccessModifier("private")
            .WithInitializer("42")
            .Build();

        // Assert
        result.ShouldContain("public object TestProperty { get; private set; } = 42;");
    }

    [Fact]
    public void ReadOnlyPropertyWithInitializer()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithType("int")
            .AsReadOnly()
            .WithInitializer("42")
            .Build();

        // Assert
        result.ShouldContain("public int TestProperty { get; } = 42;");
    }

    [Fact]
    public void InitOnlyPropertyWithInitializer()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithType("string")
            .WithInitSetter()
            .WithInitializer("\"default\"")
            .Build();

        // Assert
        result.ShouldContain("public string TestProperty { get; init; } = \"default\";");
    }

    [Fact]
    public void PropertyWithGetterAccessModifierAndInitSetter()
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithGetterAccessModifier("protected")
            .WithInitSetter()
            .Build();

        // Assert
        result.ShouldContain("public object TestProperty { protected get; init; }");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyOrNullGetterBodyCreatesAutoProperty(string? getterBody)
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithGetter(getterBody!)
            .Build();

        // Assert
        result.ShouldContain("public object TestProperty { get; set; }");
        result.ShouldNotContain("{\r\n    get");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyOrNullSetterBodyCreatesAutoProperty(string? setterBody)
    {
        // Arrange
        var builder = new PropertyBuilder();

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithSetter(setterBody!)
            .Build();

        // Assert
        result.ShouldContain("public object TestProperty { get; set; }");
        result.ShouldNotContain("{\r\n    set");
    }

    [Fact]
    public void PropertyWithMultilineGetterBody()
    {
        // Arrange
        var builder = new PropertyBuilder();
        var getterBody = "if (_value == null)\n    return \"default\";\nreturn _value;";

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithType("string")
            .WithGetter(getterBody)
            .Build();

        // Assert
        result.ShouldContain("get\r\n    {\r\n        if (_value == null)\r\n            return \"default\";\r\n        return _value;\r\n    }");
    }

    [Fact]
    public void PropertyWithMultilineSetterBody()
    {
        // Arrange
        var builder = new PropertyBuilder();
        var setterBody = "if (value == null)\n    throw new ArgumentNullException();\n_value = value;";

        // Act
        var result = builder
            .WithName("TestProperty")
            .WithType("string")
            .WithSetter(setterBody)
            .Build();

        // Assert
        result.ShouldContain("set\r\n    {\r\n        if (value == null)\r\n            throw new ArgumentNullException();\r\n        _value = value;\r\n    }");
    }
}