using System;
using Shouldly;
using Xunit;
using FractalDataWorks.CodeBuilder.Builders;

namespace FractalDataWorks.CodeBuilder.Tests;

public class FieldBuilderTests
{
    [Fact]
    public void DefaultFieldDeclaration()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe("private object field;");
    }

    [Fact]
    public void FieldWithCustomName()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder.WithName("_customField").Build();

        // Assert
        result.ShouldBe("private object _customField;");
    }

    [Fact]
    public void FieldWithType()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_value")
            .WithType("string")
            .Build();

        // Assert
        result.ShouldBe("private string _value;");
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    public void FieldWithAccessModifier(string accessModifier)
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithAccessModifier(accessModifier)
            .WithName("_testField")
            .Build();

        // Assert
        result.ShouldBe($"{accessModifier} object _testField;");
    }

    [Fact]
    public void StaticField()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_staticField")
            .AsStatic()
            .Build();

        // Assert
        result.ShouldBe("private static object _staticField;");
    }

    [Fact]
    public void ReadOnlyField()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_readOnlyField")
            .AsReadOnly()
            .Build();

        // Assert
        result.ShouldBe("private readonly object _readOnlyField;");
    }

    [Fact]
    public void ConstField()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("MaxValue")
            .WithType("int")
            .AsConst()
            .WithInitializer("100")
            .Build();

        // Assert
        result.ShouldBe("private const int MaxValue = 100;");
    }

    [Fact]
    public void VolatileField()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_volatileField")
            .AsVolatile()
            .Build();

        // Assert
        result.ShouldBe("private volatile object _volatileField;");
    }

    [Fact]
    public void StaticReadOnlyField()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_staticReadOnlyField")
            .WithType("string")
            .AsStatic()
            .AsReadOnly()
            .Build();

        // Assert
        result.ShouldBe("private static readonly string _staticReadOnlyField;");
    }

    [Fact]
    public void ConstFieldImpliesStatic()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("MaxValue")
            .WithType("int")
            .AsStatic()
            .AsConst()
            .WithInitializer("100")
            .Build();

        // Assert
        result.ShouldBe("private const int MaxValue = 100;");
        result.ShouldNotContain("static");
    }

    [Fact]
    public void ConstAndReadOnlyMutuallyExclusive()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("TestField")
            .WithType("int")
            .AsReadOnly()
            .AsConst()
            .WithInitializer("42")
            .Build();

        // Assert
        result.ShouldBe("private const int TestField = 42;");
        result.ShouldNotContain("readonly");
    }

    [Fact]
    public void ReadOnlyAndConstMutuallyExclusive()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("TestField")
            .WithType("int")
            .AsConst()
            .AsReadOnly()
            .WithInitializer("42")
            .Build();

        // Assert
        result.ShouldBe("private readonly int TestField = 42;");
        result.ShouldNotContain("const");
    }

    [Fact]
    public void VolatileAndConstMutuallyExclusive()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("TestField")
            .WithType("int")
            .AsVolatile()
            .AsConst()
            .WithInitializer("42")
            .Build();

        // Assert
        result.ShouldBe("private const int TestField = 42;");
        result.ShouldNotContain("volatile");
    }

    [Fact]
    public void ConstAndVolatileMutuallyExclusive()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("TestField")
            .WithType("int")
            .AsConst()
            .AsVolatile()
            .WithInitializer("42")
            .Build();

        // Assert
        result.ShouldBe("private volatile int TestField = 42;");
        result.ShouldNotContain("const");
    }

    [Fact]
    public void FieldWithInitializer()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_value")
            .WithType("int")
            .WithInitializer("42")
            .Build();

        // Assert
        result.ShouldBe("private int _value = 42;");
    }

    [Fact]
    public void FieldWithComplexInitializer()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_list")
            .WithType("List<string>")
            .WithInitializer("new List<string> { \"item1\", \"item2\" }")
            .Build();

        // Assert
        result.ShouldBe("private List<string> _list = new List<string> { \"item1\", \"item2\" };");
    }

    [Fact]
    public void ConstFieldRequiresInitializer()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => 
            builder
                .WithName("ConstField")
                .WithType("int")
                .AsConst()
                .Build())
            .Message.ShouldBe("Const fields must have an initializer");
    }

    [Fact]
    public void FieldWithAttribute()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_testField")
            .WithAttribute("NonSerialized")
            .Build();

        // Assert
        result.ShouldContain("[NonSerialized]");
        result.ShouldContain("private object _testField;");
    }

    [Fact]
    public void FieldWithMultipleAttributes()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_testField")
            .WithAttribute("NonSerialized")
            .WithAttribute("JsonIgnore")
            .Build();

        // Assert
        result.ShouldContain("[NonSerialized]\r\n[JsonIgnore]");
        result.ShouldContain("private object _testField;");
    }

    [Fact]
    public void FieldWithXmlDocumentation()
    {
        // Arrange
        var builder = new FieldBuilder();
        var summary = "The underlying value storage";

        // Act
        var result = builder
            .WithName("_value")
            .WithXmlDoc(summary)
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain($"/// {summary}");
        result.ShouldContain("/// </summary>");
        result.ShouldContain("private object _value;");
    }

    [Fact]
    public void FieldWithMultilineXmlDocumentation()
    {
        // Arrange
        var builder = new FieldBuilder();
        var summary = "The underlying value storage\nUsed for caching purposes";

        // Act
        var result = builder
            .WithName("_value")
            .WithXmlDoc(summary)
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// The underlying value storage");
        result.ShouldContain("/// Used for caching purposes");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void CompleteFieldExample()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithXmlDoc("Thread-safe counter for tracking operations")
            .WithAttribute("ThreadStatic")
            .WithAccessModifier("protected")
            .AsStatic()
            .AsVolatile()
            .WithType("int")
            .WithName("_operationCounter")
            .WithInitializer("0")
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Thread-safe counter for tracking operations");
        result.ShouldContain("/// </summary>");
        result.ShouldContain("[ThreadStatic]");
        result.ShouldBe("/// <summary>\r\n/// Thread-safe counter for tracking operations\r\n/// </summary>\r\n[ThreadStatic]\r\nprotected static volatile int _operationCounter = 0;");
    }

    [Fact]
    public void PublicConstField()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithAccessModifier("public")
            .WithName("DefaultTimeout")
            .WithType("int")
            .AsConst()
            .WithInitializer("30000")
            .Build();

        // Assert
        result.ShouldBe("public const int DefaultTimeout = 30000;");
    }

    [Fact]
    public void PublicStaticReadOnlyField()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithAccessModifier("public")
            .WithName("Empty")
            .WithType("string")
            .AsStatic()
            .AsReadOnly()
            .WithInitializer("string.Empty")
            .Build();

        // Assert
        result.ShouldBe("public static readonly string Empty = string.Empty;");
    }

    [Fact]
    public void PrivateReadOnlyFieldWithoutInitializer()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_id")
            .WithType("Guid")
            .AsReadOnly()
            .Build();

        // Assert
        result.ShouldBe("private readonly Guid _id;");
    }

    [Fact]
    public void FieldWithGenericType()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_items")
            .WithType("List<T>")
            .AsReadOnly()
            .WithInitializer("new List<T>()")
            .Build();

        // Assert
        result.ShouldBe("private readonly List<T> _items = new List<T>();");
    }

    [Fact]
    public void FieldWithNullableType()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_nullableValue")
            .WithType("int?")
            .WithInitializer("null")
            .Build();

        // Assert
        result.ShouldBe("private int? _nullableValue = null;");
    }

    [Fact]
    public void FieldWithArrayType()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_buffer")
            .WithType("byte[]")
            .WithInitializer("new byte[1024]")
            .Build();

        // Assert
        result.ShouldBe("private byte[] _buffer = new byte[1024];");
    }

    [Fact]
    public void FieldWithLongGenericType()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_dictionary")
            .WithType("Dictionary<string, List<KeyValuePair<int, string>>>")
            .AsReadOnly()
            .WithInitializer("new Dictionary<string, List<KeyValuePair<int, string>>>()")
            .Build();

        // Assert
        result.ShouldBe("private readonly Dictionary<string, List<KeyValuePair<int, string>>> _dictionary = new Dictionary<string, List<KeyValuePair<int, string>>>();");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FieldWithoutInitializerHandlesEmptyValues(string? initializer)
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_field")
            .WithType("string")
            .WithInitializer(initializer!)
            .Build();

        // Assert
        result.ShouldBe("private string _field;");
        result.ShouldNotContain("=");
    }

    [Fact]
    public void FieldWithStringInitializer()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_message")
            .WithType("string")
            .WithInitializer("\"Hello, World!\"")
            .Build();

        // Assert
        result.ShouldBe("private string _message = \"Hello, World!\";");
    }

    [Fact]
    public void FieldWithBooleanInitializer()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_isEnabled")
            .WithType("bool")
            .WithInitializer("true")
            .Build();

        // Assert
        result.ShouldBe("private bool _isEnabled = true;");
    }

    [Fact]
    public void FieldWithMethodCallInitializer()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_timestamp")
            .WithType("DateTime")
            .AsReadOnly()
            .WithInitializer("DateTime.UtcNow")
            .Build();

        // Assert
        result.ShouldBe("private readonly DateTime _timestamp = DateTime.UtcNow;");
    }

    [Fact]
    public void FieldWithLambdaInitializer()
    {
        // Arrange
        var builder = new FieldBuilder();

        // Act
        var result = builder
            .WithName("_factory")
            .WithType("Func<string, int>")
            .AsReadOnly()
            .WithInitializer("s => s.Length")
            .Build();

        // Assert
        result.ShouldBe("private readonly Func<string, int> _factory = s => s.Length;");
    }
}