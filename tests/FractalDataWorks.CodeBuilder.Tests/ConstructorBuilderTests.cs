using System;
using Shouldly;
using Xunit;
using FractalDataWorks.CodeBuilder.Builders;

namespace FractalDataWorks.CodeBuilder.Tests;

public class ConstructorBuilderTests
{
    private static readonly string[] NewLineSeparators = { "\r\n", "\n" };
    [Fact]
    public void DefaultConstructorDeclaration()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe("public MyClass()\r\n{\r\n}");
    }

    // Note: WithClassName is not part of IConstructorBuilder interface
    // Tests using WithClassName would need to be done with concrete ConstructorBuilder type

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    public void ConstructorWithAccessModifier(string accessModifier)
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithAccessModifier(accessModifier)
            .Build();

        // Assert
        result.ShouldContain($"{accessModifier} MyClass()");
    }

    [Fact]
    public void StaticConstructor()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .AsStatic()
            .Build();

        // Assert
        result.ShouldContain("static MyClass()");
        result.ShouldNotContain("public");
        result.ShouldNotContain("private");
    }

    [Fact]
    public void ConstructorWithSingleParameter()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithParameter("string", "name")
            .Build();

        // Assert
        result.ShouldContain("public MyClass(string name)");
    }

    [Fact]
    public void ConstructorWithMultipleParameters()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithParameter("string", "name")
            .WithParameter("int", "age")
            .WithParameter("bool", "isActive")
            .Build();

        // Assert
        result.ShouldContain("public MyClass(string name, int age, bool isActive)");
    }

    [Fact]
    public void ConstructorWithParameterWithDefaultValue()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithParameter("string", "name", "\"default\"")
            .Build();

        // Assert
        result.ShouldContain("public MyClass(string name = \"default\")");
    }

    [Fact]
    public void ConstructorWithMixedParameters()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithParameter("string", "required")
            .WithParameter("int", "optional", "42")
            .WithParameter("bool", "flag", "true")
            .Build();

        // Assert
        result.ShouldContain("public MyClass(string required, int optional = 42, bool flag = true)");
    }

    [Fact]
    public void ConstructorWithBaseCall()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
                .WithParameter("string", "name")
            .WithBaseCall("name")
            .Build();

        // Assert
        result.ShouldContain("public MyClass(string name) : base(name)");
    }

    [Fact]
    public void ConstructorWithBaseCallMultipleArguments()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
                .WithParameter("string", "name")
            .WithParameter("int", "age")
            .WithBaseCall("name", "age")
            .Build();

        // Assert
        result.ShouldContain("public MyClass(string name, int age) : base(name, age)");
    }

    [Fact]
    public void ConstructorWithThisCall()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithParameter("string", "name")
            .WithThisCall("name", "0")
            .Build();

        // Assert
        result.ShouldContain("public MyClass(string name) : this(name, 0)");
    }

    [Fact]
    public void ConstructorWithThisCallMultipleArguments()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithParameter("string", "name")
            .WithThisCall("name", "0", "true")
            .Build();

        // Assert
        result.ShouldContain("public MyClass(string name) : this(name, 0, true)");
    }

    [Fact]
    public void BaseCallAndThisCallMutuallyExclusive()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithParameter("string", "name")
            .WithBaseCall("name")
            .WithThisCall("name", "0")
            .Build();

        // Assert
        result.ShouldContain("public MyClass(string name) : this(name, 0)");
        result.ShouldNotContain("base");
    }

    [Fact]
    public void ThisCallAndBaseCallMutuallyExclusive()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithParameter("string", "name")
            .WithThisCall("name", "0")
            .WithBaseCall("name")
            .Build();

        // Assert
        result.ShouldContain("public MyClass(string name) : base(name)");
        result.ShouldNotContain("this");
    }

    [Fact]
    public void ConstructorWithAttribute()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithAttribute("JsonConstructor")
            .Build();

        // Assert
        result.ShouldContain("[JsonConstructor]");
        result.ShouldContain("public MyClass()");
    }

    [Fact]
    public void ConstructorWithMultipleAttributes()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithAttribute("JsonConstructor")
            .WithAttribute("Obsolete(\"Use other constructor\")")
            .Build();

        // Assert
        result.ShouldContain("[JsonConstructor]");
        result.ShouldContain("[Obsolete(\"Use other constructor\")]");
    }

    [Fact]
    public void ConstructorWithBody()
    {
        // Arrange
        var builder = new ConstructorBuilder();
        var body = "_name = name ?? throw new ArgumentNullException(nameof(name));";

        // Act
        var result = builder
            .WithParameter("string", "name")
            .WithBody(body)
            .Build();

        // Assert
        result.ShouldContain("{\r\n    _name = name ?? throw new ArgumentNullException(nameof(name));\r\n}");
    }

    [Fact]
    public void ConstructorWithMultilineBody()
    {
        // Arrange
        var builder = new ConstructorBuilder();
        var body = "_name = name ?? throw new ArgumentNullException(nameof(name));\n_age = age;";

        // Act
        var result = builder
            .WithParameter("string", "name")
            .WithParameter("int", "age")
            .WithBody(body)
            .Build();

        // Assert
        result.ShouldContain("{\r\n    _name = name ?? throw new ArgumentNullException(nameof(name));\r\n    _age = age;\r\n}");
    }

    [Fact]
    public void AddBodyLineAppendsToExistingBody()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithParameter("string", "name")
            .AddBodyLine("_name = name;")
            .AddBodyLine("_initialized = true;")
            .Build();

        // Assert
        result.ShouldContain("{\r\n    _name = name;\r\n    _initialized = true;\r\n}");
    }

    [Fact]
    public void WithBodyReplacesExistingBodyLines()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .AddBodyLine("old line")
            .WithBody("new body")
            .Build();

        // Assert
        result.ShouldContain("new body");
        result.ShouldNotContain("old line");
    }

    [Fact]
    public void ConstructorWithXmlDocSummary()
    {
        // Arrange
        var builder = new ConstructorBuilder();
        var summary = "Initializes a new instance of the TestClass";

        // Act
        var result = builder
            .WithXmlDoc(summary)
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain($"/// {summary}");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void ConstructorWithMultilineXmlDocSummary()
    {
        // Arrange
        var builder = new ConstructorBuilder();
        var summary = "Initializes a new instance of the TestClass\nwith the specified parameters";

        // Act
        var result = builder
            .WithXmlDoc(summary)
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Initializes a new instance of the TestClass");
        result.ShouldContain("/// with the specified parameters");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void ConstructorWithParameterDocumentation()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithParameter("string", "name")
            .WithParameter("int", "age")
            .WithParamDoc("name", "The person's name")
            .WithParamDoc("age", "The person's age")
            .Build();

        // Assert
        result.ShouldContain("/// <param name=\"name\">The person's name</param>");
        result.ShouldContain("/// <param name=\"age\">The person's age</param>");
    }

    [Fact]
    public void CompleteXmlDocumentationExample()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
                .WithParameter("string", "firstName")
            .WithParameter("string", "lastName")
            .WithParameter("int", "age")
            .WithXmlDoc("Initializes a new instance of the Person class")
            .WithParamDoc("firstName", "The person's first name")
            .WithParamDoc("lastName", "The person's last name")
            .WithParamDoc("age", "The person's age")
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Initializes a new instance of the Person class");
        result.ShouldContain("/// </summary>");
        result.ShouldContain("/// <param name=\"firstName\">The person's first name</param>");
        result.ShouldContain("/// <param name=\"lastName\">The person's last name</param>");
        result.ShouldContain("/// <param name=\"age\">The person's age</param>");
    }

    [Fact]
    public void ConstructorWithOnlyParameterDocumentationWithoutSummary()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithParameter("string", "value")
            .WithParamDoc("value", "The value parameter")
            .Build();

        // Assert
        result.ShouldContain("/// <param name=\"value\">The value parameter</param>");
        result.ShouldNotContain("/// <summary>");
    }

    [Fact]
    public void ParameterDocumentationOnlyForExistingParameters()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithParameter("string", "existing")
            .WithParamDoc("existing", "Existing parameter")
            .WithParamDoc("nonExistent", "Non-existent parameter")
            .Build();

        // Assert
        result.ShouldContain("/// <param name=\"existing\">Existing parameter</param>");
        result.ShouldNotContain("/// <param name=\"nonExistent\">Non-existent parameter</param>");
    }

    [Fact]
    public void CompleteConstructorExample()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithXmlDoc("Initializes a new instance with validation")
            .WithParamDoc("name", "The entity name")
            .WithParamDoc("value", "The initial value")
            .WithAttribute("JsonConstructor")
                .WithAccessModifier("public")
            .WithParameter("string", "name")
            .WithParameter("object", "value", "null")
            .WithBaseCall("name")
            .WithBody("_value = value ?? throw new ArgumentNullException(nameof(value));\n_initialized = true;")
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Initializes a new instance with validation");
        result.ShouldContain("/// </summary>");
        result.ShouldContain("/// <param name=\"name\">The entity name</param>");
        result.ShouldContain("/// <param name=\"value\">The initial value</param>");
        result.ShouldContain("[JsonConstructor]");
        result.ShouldContain("public MyClass(string name, object value = null) : base(name)");
        result.ShouldContain("{\r\n    _value = value ?? throw new ArgumentNullException(nameof(value));\r\n    _initialized = true;\r\n}");
    }

    [Fact]
    public void EmptyConstructorBody()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .Build();

        // Assert
        var lines = result.Split(NewLineSeparators, StringSplitOptions.None);
        lines[0].ShouldBe("public MyClass()");
        lines[1].ShouldBe("{");
        lines[2].ShouldBe("}");
        lines.Length.ShouldBe(3);
    }

    [Fact]
    public void StaticConstructorCannotHaveAccessModifier()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithAccessModifier("public")
            .AsStatic()
            .Build();

        // Assert
        result.ShouldContain("static MyClass()");
        result.ShouldNotContain("public static");
    }

    [Fact]
    public void StaticConstructorWithParameters()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithParameter("string", "param")
            .AsStatic()
            .Build();

        // Assert
        result.ShouldContain("static MyClass(string param)");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyOrNullBodyCreatesEmptyConstructorBlock(string? body)
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithBody(body!)
            .Build();

        // Assert
        result.ShouldBe("public MyClass()\r\n{\r\n}");
    }

    [Fact]
    public void ConstructorWithComplexGenericParameters()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .WithClassName("Repository")
            .WithParameter("ILogger<Repository<T>>", "logger")
            .WithParameter("IOptions<DatabaseConfig>", "options")
            .Build();

        // Assert
        result.ShouldContain("public Repository(ILogger<Repository<T>> logger, IOptions<DatabaseConfig> options)");
    }

    [Fact]
    public void ConstructorBodyLinesAreTrimmedOfTrailingWhitespace()
    {
        // Arrange
        var builder = new ConstructorBuilder();

        // Act
        var result = builder
            .AddBodyLine("line with trailing spaces   ")
            .AddBodyLine("line with tab\t")
            .Build();

        // Assert
        result.ShouldContain("    line with trailing spaces\r\n");
        result.ShouldContain("    line with tab\r\n");
        result.ShouldNotContain("spaces   ");
        result.ShouldNotContain("tab\t");
    }
}