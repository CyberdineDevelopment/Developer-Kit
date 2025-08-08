using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using FractalDataWorks.CodeBuilder.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FractalDataWorks.CodeBuilder.Tests;

public class RoslynSyntaxTreeAndNodeTests
{
    private async Task<RoslynSyntaxTree> CreateSyntaxTreeAsync(string sourceCode, string? filePath = null)
    {
        var parser = new RoslynCSharpParser();
        var result = await parser.ParseAsync(sourceCode, filePath);
        result.IsSuccess.ShouldBeTrue();
        return (RoslynSyntaxTree)result.Value!;
    }

    [Fact]
    public async Task SyntaxTreeHasCorrectProperties()
    {
        // Arrange
        var sourceCode = "public class TestClass { }";
        var filePath = "TestClass.cs";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode, filePath);

        // Assert
        syntaxTree.SourceText.ShouldBe(sourceCode);
        syntaxTree.Language.ShouldBe("csharp");
        syntaxTree.FilePath.ShouldBe(filePath);
    }

    [Fact]
    public async Task SyntaxTreeWithoutFilePathUsesDefault()
    {
        // Arrange
        var sourceCode = "public class TestClass { }";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);

        // Assert
        syntaxTree.FilePath.ShouldBe("");
    }

    [Fact]
    public async Task ValidSyntaxTreeHasNoErrors()
    {
        // Arrange
        var sourceCode = "public class TestClass { public void Method() { } }";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);

        // Assert
        syntaxTree.HasErrors.ShouldBeFalse();
        syntaxTree.GetErrors().Count().ShouldBe(0);
    }

    [Fact]
    public async Task InvalidSyntaxTreeHasErrors()
    {
        // Arrange
        var sourceCode = "public class TestClass { invalid syntax }";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);

        // Assert
        syntaxTree.HasErrors.ShouldBeTrue();
        syntaxTree.GetErrors().Count().ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task SyntaxTreeRootIsNotNull()
    {
        // Arrange
        var sourceCode = "public class TestClass { }";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);

        // Assert
        syntaxTree.Root.ShouldNotBeNull();
        syntaxTree.Root.NodeType.ShouldBe("CompilationUnit");
    }

    [Fact]
    public async Task FindNodesReturnsMatchingNodes()
    {
        // Arrange
        var sourceCode = @"
public class TestClass 
{ 
    public void Method1() { }
    public void Method2() { }
}";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var methods = syntaxTree.FindNodes("MethodDeclaration");

        // Assert
        methods.Count().ShouldBe(2);
        methods.ShouldAllBe(n => n.NodeType == "MethodDeclaration");
    }

    [Fact]
    public async Task FindNodesWithNonExistentTypeReturnsEmpty()
    {
        // Arrange
        var sourceCode = "public class TestClass { }";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var nodes = syntaxTree.FindNodes("NonExistentType");

        // Assert
        nodes.Count().ShouldBe(0);
    }

    [Fact]
    public async Task GetNodeAtPositionReturnsCorrectNode()
    {
        // Arrange
        var sourceCode = "public class TestClass { }";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var node = syntaxTree.GetNodeAtPosition(13); // Position within "TestClass"

        // Assert
        node.ShouldNotBeNull();
        node!.Text.ShouldContain("TestClass");
    }

    [Fact]
    public async Task GetNodeAtInvalidPositionReturnsNull()
    {
        // Arrange
        var sourceCode = "public class TestClass { }";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var node = syntaxTree.GetNodeAtPosition(1000); // Beyond end of source

        // Assert
        node.ShouldBeNull();
    }

    [Fact]
    public async Task GetNodeAtLocationReturnsCorrectNode()
    {
        // Arrange
        var sourceCode = @"public class TestClass 
{ 
    public void Method() { }
}";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var node = syntaxTree.GetNodeAtLocation(2, 16); // Position within "Method"

        // Assert
        node.ShouldNotBeNull();
        node!.Text.ShouldContain("Method");
    }

    [Fact]
    public async Task RootNodeHasCorrectProperties()
    {
        // Arrange
        var sourceCode = "public class TestClass { }";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var root = syntaxTree.Root;

        // Assert
        root.NodeType.ShouldBe("CompilationUnit");
        root.Parent.ShouldBeNull();
        root.IsTerminal.ShouldBeFalse();
        root.IsError.ShouldBeFalse();
        root.StartPosition.ShouldBe(0);
        root.EndPosition.ShouldBe(sourceCode.Length);
    }

    [Fact]
    public async Task NodeChildrenAreCorrectlyPopulated()
    {
        // Arrange
        var sourceCode = @"
namespace TestNamespace
{
    public class TestClass { }
}";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var root = syntaxTree.Root;

        // Assert
        root.Children.Count.ShouldBeGreaterThan(0);
        var namespaceNode = root.Children.FirstOrDefault(c => c.NodeType == "FileScopedNamespaceDeclaration" || c.NodeType == "NamespaceDeclaration");
        namespaceNode.ShouldNotBeNull();
    }

    [Fact]
    public async Task FindChildReturnsCorrectChild()
    {
        // Arrange
        var sourceCode = @"
public class TestClass 
{
    public void Method() { }
}";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var classNode = syntaxTree.FindNodes("ClassDeclaration").First();
        var methodNode = classNode.FindChild("MethodDeclaration");

        // Assert
        methodNode.ShouldNotBeNull();
        methodNode!.NodeType.ShouldBe("MethodDeclaration");
    }

    [Fact]
    public async Task FindChildrenReturnsAllMatchingChildren()
    {
        // Arrange
        var sourceCode = @"
public class TestClass 
{
    public void Method1() { }
    public void Method2() { }
    public int Property { get; set; }
}";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var classNode = syntaxTree.FindNodes("ClassDeclaration").First();
        var methods = classNode.FindChildren("MethodDeclaration");

        // Assert
        methods.Count().ShouldBe(2);
        methods.ShouldAllBe(n => n.NodeType == "MethodDeclaration");
    }

    [Fact]
    public async Task DescendantNodesReturnsAllDescendants()
    {
        // Arrange
        var sourceCode = @"
public class TestClass 
{
    public void Method() 
    { 
        var x = 42;
    }
}";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var classNode = syntaxTree.FindNodes("ClassDeclaration").First();
        var descendants = classNode.DescendantNodes();

        // Assert
        descendants.Count().ShouldBeGreaterThan(5); // Should include method, block, variable declaration, etc.
    }

    [Fact]
    public async Task NodePositionPropertiesAreCorrect()
    {
        // Arrange
        var sourceCode = @"public class TestClass { }";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var classNode = syntaxTree.FindNodes("ClassDeclaration").First();

        // Assert
        classNode.StartPosition.ShouldBeGreaterThan(0);
        classNode.EndPosition.ShouldBeGreaterThan(classNode.StartPosition);
        classNode.StartLine.ShouldBe(0);
        classNode.StartColumn.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task NodeTextContainsExpectedContent()
    {
        // Arrange
        var sourceCode = @"public class TestClass { }";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var classNode = syntaxTree.FindNodes("ClassDeclaration").First();

        // Assert
        classNode.Text.ShouldContain("TestClass");
        classNode.Text.ShouldContain("public");
        classNode.Text.ShouldContain("class");
    }

    [Fact]
    public async Task TerminalNodeHasNoChildren()
    {
        // Arrange
        var sourceCode = @"public class TestClass { }";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var identifierNodes = syntaxTree.FindNodes("IdentifierToken");

        // Assert
        if (identifierNodes.Any())
        {
            var identifierNode = identifierNodes.First();
            identifierNode.IsTerminal.ShouldBeTrue();
            identifierNode.Children.Count.ShouldBe(0);
        }
    }

    [Fact]
    public async Task ParentChildRelationshipsAreCorrect()
    {
        // Arrange
        var sourceCode = @"
public class TestClass 
{
    public void Method() { }
}";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var classNode = syntaxTree.FindNodes("ClassDeclaration").First();
        var methodNode = classNode.FindChild("MethodDeclaration");

        // Assert
        methodNode.ShouldNotBeNull();
        methodNode!.Parent.ShouldBe(classNode);
    }

    [Fact]
    public async Task ErrorNodeIsDetected()
    {
        // Arrange
        var sourceCode = @"public class TestClass { invalid }";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);
        var errorNodes = syntaxTree.GetErrors();

        // Assert
        errorNodes.Count().ShouldBeGreaterThan(0);
        errorNodes.ShouldAllBe(n => n.IsError);
    }

    [Fact]
    public async Task ComplexSyntaxTreeNavigation()
    {
        // Arrange
        var sourceCode = @"
using System;

namespace TestNamespace
{
    public class TestClass<T> where T : class
    {
        private readonly T _value;
        
        public T Value => _value;
        
        public TestClass(T value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }
        
        public void Method<U>(U parameter) where U : IComparable<U>
        {
            Console.WriteLine(parameter);
        }
    }
}";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);

        // Assert
        // Find using directives
        var usingDirectives = syntaxTree.FindNodes("UsingDirective");
        usingDirectives.Count().ShouldBeGreaterThan(0);

        // Find namespace
        var namespaceNodes = syntaxTree.FindNodes("NamespaceDeclaration");
        namespaceNodes.Count().ShouldBe(1);

        // Find class
        var classNodes = syntaxTree.FindNodes("ClassDeclaration");
        classNodes.Count().ShouldBe(1);

        // Find fields, properties, constructors, methods
        var fieldNodes = syntaxTree.FindNodes("FieldDeclaration");
        fieldNodes.Count().ShouldBe(1);

        var propertyNodes = syntaxTree.FindNodes("PropertyDeclaration");
        propertyNodes.Count().ShouldBe(1);

        var constructorNodes = syntaxTree.FindNodes("ConstructorDeclaration");
        constructorNodes.Count().ShouldBe(1);

        var methodNodes = syntaxTree.FindNodes("MethodDeclaration");
        methodNodes.Count().ShouldBe(1);
    }

    [Fact]
    public async Task SyntaxTreeHandlesWhitespaceAndComments()
    {
        // Arrange
        var sourceCode = @"
// This is a comment
public class TestClass // Another comment
{
    /* Multi-line
       comment */
    public void Method()
    {
        // Method comment
    }
}";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);

        // Assert
        syntaxTree.HasErrors.ShouldBeFalse();
        var classNode = syntaxTree.FindNodes("ClassDeclaration").First();
        classNode.ShouldNotBeNull();
        classNode.Text.ShouldContain("TestClass");
    }

    [Fact]
    public async Task SyntaxTreeHandlesStringLiterals()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public string GetMessage()
    {
        return ""Hello, World!"";
    }
    
    public string GetMultilineString()
    {
        return @""This is a
multi-line string"";
    }
}";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);

        // Assert
        syntaxTree.HasErrors.ShouldBeFalse();
        var stringLiterals = syntaxTree.FindNodes("StringLiteralExpression");
        stringLiterals.Count().ShouldBe(2);
    }

    [Fact]
    public async Task SyntaxTreeHandlesGenericConstraints()
    {
        // Arrange
        var sourceCode = @"
public class TestClass<T, U> 
    where T : class, IDisposable, new()
    where U : struct
{
    public void Method<V>(V value) where V : IComparable<V>
    {
    }
}";

        // Act
        var syntaxTree = await CreateSyntaxTreeAsync(sourceCode);

        // Assert
        syntaxTree.HasErrors.ShouldBeFalse();
        var constraintClauses = syntaxTree.FindNodes("TypeParameterConstraintClause");
        constraintClauses.Count().ShouldBe(3); // Two on class, one on method
    }
}