using System;
using System.Linq;
using FractalDataWorks.CodeBuilder.CSharp.Builders;
using Shouldly;
using Xunit;

namespace FractalDataWorks.CodeBuilder.Tests;

public class MethodBuilderTests
{
    [Fact]
    public void DefaultMethodDeclaration()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe("public void Method()\r\n{\r\n}");
    }

    [Fact]
    public void MethodWithCustomName()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder.WithName("CustomMethod").Build();

        // Assert
        result.ShouldContain("public void CustomMethod()");
    }

    [Fact]
    public void MethodWithReturnType()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("GetValue")
            .WithReturnType("string")
            .Build();

        // Assert
        result.ShouldContain("public string GetValue()");
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    public void MethodWithAccessModifier(string accessModifier)
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithAccessModifier(accessModifier)
            .WithName("TestMethod")
            .Build();

        // Assert
        result.ShouldContain($"{accessModifier} void TestMethod()");
    }

    [Fact]
    public void StaticMethod()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("StaticMethod")
            .AsStatic()
            .Build();

        // Assert
        result.ShouldContain("public static void StaticMethod()");
    }

    [Fact]
    public void VirtualMethod()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("VirtualMethod")
            .AsVirtual()
            .Build();

        // Assert
        result.ShouldContain("public virtual void VirtualMethod()");
    }

    [Fact]
    public void OverrideMethod()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("OverrideMethod")
            .AsOverride()
            .Build();

        // Assert
        result.ShouldContain("public override void OverrideMethod()");
    }

    [Fact]
    public void AbstractMethod()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("AbstractMethod")
            .AsAbstract()
            .Build();

        // Assert
        result.ShouldBe("public abstract void AbstractMethod();");
    }

    [Fact]
    public void AsyncMethod()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("AsyncMethod")
            .AsAsync()
            .WithReturnType("Task")
            .Build();

        // Assert
        result.ShouldContain("public async Task AsyncMethod()");
    }

    [Fact]
    public void VirtualAndOverrideMutuallyExclusive()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .AsVirtual()
            .AsOverride()
            .Build();

        // Assert
        result.ShouldContain("public override void TestMethod()");
        result.ShouldNotContain("virtual");
    }

    [Fact]
    public void OverrideAndVirtualMutuallyExclusive()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .AsOverride()
            .AsVirtual()
            .Build();

        // Assert
        result.ShouldContain("public virtual void TestMethod()");
        result.ShouldNotContain("override");
    }

    [Fact]
    public void MethodWithSingleParameter()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .WithParameter("string", "value")
            .Build();

        // Assert
        result.ShouldContain("public void TestMethod(string value)");
    }

    [Fact]
    public void MethodWithMultipleParameters()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .WithParameter("string", "name")
            .WithParameter("int", "age")
            .WithParameter("bool", "isActive")
            .Build();

        // Assert
        result.ShouldContain("public void TestMethod(string name, int age, bool isActive)");
    }

    [Fact]
    public void MethodWithParameterWithDefaultValue()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .WithParameter("string", "value", "\"default\"")
            .Build();

        // Assert
        result.ShouldContain("public void TestMethod(string value = \"default\")");
    }

    [Fact]
    public void MethodWithMixedParameters()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .WithParameter("string", "required")
            .WithParameter("int", "optional", "42")
            .WithParameter("bool", "flag", "true")
            .Build();

        // Assert
        result.ShouldContain("public void TestMethod(string required, int optional = 42, bool flag = true)");
    }

    [Fact]
    public void GenericMethod()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("GenericMethod")
            .WithGenericParameters("T")
            .WithReturnType("T")
            .WithParameter("T", "value")
            .Build();

        // Assert
        result.ShouldContain("public T GenericMethod<T>(T value)");
    }

    [Fact]
    public void GenericMethodWithMultipleParameters()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("GenericMethod")
            .WithGenericParameters("T", "U", "V")
            .WithReturnType("V")
            .Build();

        // Assert
        result.ShouldContain("public V GenericMethod<T, U, V>()");
    }

    [Fact]
    public void GenericMethodWithConstraints()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("GenericMethod")
            .WithGenericParameters("T")
            .WithGenericConstraint("T", "class")
            .WithReturnType("T")
            .Build();

        // Assert
        result.ShouldContain("public T GenericMethod<T>()");
        result.ShouldContain("    where T : class");
    }

    [Fact]
    public void GenericMethodWithMultipleConstraints()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("GenericMethod")
            .WithGenericParameters("T", "U")
            .WithGenericConstraint("T", "class", "new()")
            .WithGenericConstraint("U", "struct")
            .WithReturnType("void")
            .Build();

        // Assert
        result.ShouldContain("public void GenericMethod<T, U>()");
        result.ShouldContain("    where T : class, new()");
        result.ShouldContain("    where U : struct");
    }

    [Fact]
    public void MethodWithAttribute()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .WithAttribute("HttpGet")
            .Build();

        // Assert
        result.ShouldContain("[HttpGet]");
        result.ShouldContain("public void TestMethod()");
    }

    [Fact]
    public void MethodWithMultipleAttributes()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .WithAttribute("HttpGet")
            .WithAttribute("Authorize")
            .Build();

        // Assert
        result.ShouldContain("[HttpGet]");
        result.ShouldContain("[Authorize]");
    }

    [Fact]
    public void MethodWithBody()
    {
        // Arrange
        var builder = new MethodBuilder();
        var body = "return 42;";

        // Act
        var result = builder
            .WithName("GetNumber")
            .WithReturnType("int")
            .WithBody(body)
            .Build();

        // Assert
        result.ShouldContain("{\r\n    return 42;\r\n}");
    }

    [Fact]
    public void MethodWithMultilineBody()
    {
        // Arrange
        var builder = new MethodBuilder();
        var body = "var value = 42;\nreturn value;";

        // Act
        var result = builder
            .WithName("GetNumber")
            .WithReturnType("int")
            .WithBody(body)
            .Build();

        // Assert
        result.ShouldContain("{\r\n    var value = 42;\r\n    return value;\r\n}");
    }

    [Fact]
    public void AddBodyLineAppendsToExistingBody()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .AddBodyLine("var x = 1;")
            .AddBodyLine("var y = 2;")
            .AddBodyLine("return x + y;")
            .WithReturnType("int")
            .Build();

        // Assert
        result.ShouldContain("{\r\n    var x = 1;\r\n    var y = 2;\r\n    return x + y;\r\n}");
    }

    [Fact]
    public void WithBodyReplacesExistingBodyLines()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .AddBodyLine("old line")
            .WithBody("new body")
            .Build();

        // Assert
        result.ShouldContain("new body");
        result.ShouldNotContain("old line");
    }

    [Fact]
    public void MethodWithExpressionBody()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("GetNumber")
            .WithReturnType("int")
            .WithExpressionBody("42")
            .Build();

        // Assert
        result.ShouldBe("public int GetNumber() => 42;");
    }

    [Fact]
    public void WithExpressionBodyClearsPreviousBodyLines()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .AddBodyLine("old body")
            .WithExpressionBody("new expression")
            .Build();

        // Assert
        result.ShouldContain("=> new expression;");
        result.ShouldNotContain("old body");
        result.ShouldNotContain("{");
        result.ShouldNotContain("}");
    }

    [Fact]
    public void AddBodyLineClearsExpressionBody()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .WithExpressionBody("old expression")
            .AddBodyLine("new body")
            .Build();

        // Assert
        result.ShouldContain("{\r\n    new body\r\n}");
        result.ShouldNotContain("=>");
        result.ShouldNotContain("old expression");
    }

    [Fact]
    public void MethodWithXmlDocSummary()
    {
        // Arrange
        var builder = new MethodBuilder();
        var summary = "This method does something";

        // Act
        var result = builder
            .WithName("TestMethod")
            .WithXmlDoc(summary)
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain($"/// {summary}");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void MethodWithMultilineXmlDocSummary()
    {
        // Arrange
        var builder = new MethodBuilder();
        var summary = "This method does something\nwith multiple lines";

        // Act
        var result = builder
            .WithName("TestMethod")
            .WithXmlDoc(summary)
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// This method does something");
        result.ShouldContain("/// with multiple lines");
        result.ShouldContain("/// </summary>");
    }

    [Fact]
    public void MethodWithParameterDocumentation()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
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
    public void MethodWithReturnDocumentation()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("GetValue")
            .WithReturnType("string")
            .WithReturnDoc("The calculated value")
            .Build();

        // Assert
        result.ShouldContain("/// <returns>The calculated value</returns>");
    }

    [Fact]
    public void CompleteXmlDocumentationExample()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("CalculateSum")
            .WithReturnType("int")
            .WithParameter("int", "a")
            .WithParameter("int", "b")
            .WithXmlDoc("Calculates the sum of two integers")
            .WithParamDoc("a", "First number")
            .WithParamDoc("b", "Second number")
            .WithReturnDoc("The sum of a and b")
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Calculates the sum of two integers");
        result.ShouldContain("/// </summary>");
        result.ShouldContain("/// <param name=\"a\">First number</param>");
        result.ShouldContain("/// <param name=\"b\">Second number</param>");
        result.ShouldContain("/// <returns>The sum of a and b</returns>");
    }

    [Fact]
    public void MethodWithOnlyParameterDocumentationWithoutSummary()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
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
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .WithParameter("string", "existing")
            .WithParamDoc("existing", "Existing parameter")
            .WithParamDoc("nonExistent", "Non-existent parameter")
            .Build();

        // Assert
        result.ShouldContain("/// <param name=\"existing\">Existing parameter</param>");
        result.ShouldNotContain("/// <param name=\"nonExistent\">Non-existent parameter</param>");
    }

    [Fact]
    public void CompleteMethodExample()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithXmlDoc("Processes a user asynchronously")
            .WithParamDoc("userId", "The user identifier")
            .WithParamDoc("options", "Processing options")
            .WithReturnDoc("A task representing the processing result")
            .WithAttribute("HttpPost")
            .WithAttribute("Authorize(Roles = \"Admin\")")
            .WithAccessModifier("public")
            .AsAsync()
            .WithReturnType("Task<ProcessResult>")
            .WithName("ProcessUser")
            .WithGenericParameters("T")
            .WithGenericConstraint("T", "class", "IProcessable")
            .WithParameter("int", "userId")
            .WithParameter("ProcessOptions<T>", "options", "null")
            .WithBody("var user = await GetUser(userId);\nif (user == null) throw new UserNotFoundException();\nreturn await ProcessUserInternal(user, options);")
            .Build();

        // Assert
        result.ShouldContain("/// <summary>");
        result.ShouldContain("/// Processes a user asynchronously");
        result.ShouldContain("/// </summary>");
        result.ShouldContain("/// <param name=\"userId\">The user identifier</param>");
        result.ShouldContain("/// <param name=\"options\">Processing options</param>");
        result.ShouldContain("/// <returns>A task representing the processing result</returns>");
        result.ShouldContain("[HttpPost]");
        result.ShouldContain("[Authorize(Roles = \"Admin\")]");
        result.ShouldContain("public async Task<ProcessResult> ProcessUser<T>(int userId, ProcessOptions<T> options = null)");
        result.ShouldContain("    where T : class, IProcessable");
        result.ShouldContain("{\r\n    var user = await GetUser(userId);\r\n    if (user == null) throw new UserNotFoundException();\r\n    return await ProcessUserInternal(user, options);\r\n}");
    }

    [Fact]
    public void AbstractMethodDoesNotHaveBody()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("AbstractMethod")
            .AsAbstract()
            .AddBodyLine("this should be ignored")
            .Build();

        // Assert
        result.ShouldBe("public abstract void AbstractMethod();");
        result.ShouldNotContain("{");
        result.ShouldNotContain("}");
        result.ShouldNotContain("this should be ignored");
    }

    [Fact]
    public void ExpressionBodyIgnoredForAbstractMethods()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("AbstractMethod")
            .AsAbstract()
            .WithExpressionBody("ignored")
            .Build();

        // Assert
        result.ShouldBe("public abstract void AbstractMethod();");
        result.ShouldNotContain("=>");
        result.ShouldNotContain("ignored");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyOrNullBodyCreatesEmptyMethodBlock(string? body)
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
            .WithBody(body!)
            .Build();

        // Assert
        result.ShouldBe("public void TestMethod()\r\n{\r\n}");
    }

    [Fact]
    public void BodyLinesAreTrimmedOfTrailingWhitespace()
    {
        // Arrange
        var builder = new MethodBuilder();

        // Act
        var result = builder
            .WithName("TestMethod")
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