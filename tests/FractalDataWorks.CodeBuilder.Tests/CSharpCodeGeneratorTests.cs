using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;
using Moq;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.CSharp.Generators;

namespace FractalDataWorks.CodeBuilder.Tests;

public class CSharpCodeGeneratorTests
{
    private static readonly string[] NewLineSeparators = ["\r\n", "\n"];
    [Fact]
    public void TargetLanguageIsCSharp()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();

        // Act
        var language = generator.TargetLanguage;

        // Assert
        language.ShouldBe("csharp");
    }

    [Fact]
    public void GenerateFromSyntaxTreeReturnsSourceText()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        var mockSyntaxTree = new Mock<ISyntaxTree>();
        var sourceText = "public class TestClass { }";
        mockSyntaxTree.Setup(t => t.SourceText).Returns(sourceText);

        // Act
        var result = generator.Generate(mockSyntaxTree.Object);

        // Assert
        result.ShouldBe(sourceText);
    }

    [Fact]
    public void GenerateFromClassBuilderCallsBuild()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        var mockClassBuilder = new Mock<IClassBuilder>();
        var expectedCode = "public class TestClass\r\n{\r\n}";
        mockClassBuilder.Setup(b => b.Build()).Returns(expectedCode);

        // Act
        var result = generator.Generate(mockClassBuilder.Object);

        // Assert
        result.ShouldBe(expectedCode);
        mockClassBuilder.Verify(b => b.Build(), Times.Once);
    }

    [Fact]
    public void GenerateFromInterfaceBuilderCallsBuild()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        var mockInterfaceBuilder = new Mock<IInterfaceBuilder>();
        var expectedCode = "public interface ITestInterface\r\n{\r\n}";
        mockInterfaceBuilder.Setup(b => b.Build()).Returns(expectedCode);

        // Act
        var result = generator.Generate(mockInterfaceBuilder.Object);

        // Assert
        result.ShouldBe(expectedCode);
        mockInterfaceBuilder.Verify(b => b.Build(), Times.Once);
    }

    [Fact]
    public void GenerateFromEnumBuilderCallsBuild()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        var mockEnumBuilder = new Mock<IEnumBuilder>();
        var expectedCode = "public enum TestEnum\r\n{\r\n    Value1,\r\n    Value2\r\n}";
        mockEnumBuilder.Setup(b => b.Build()).Returns(expectedCode);

        // Act
        var result = generator.Generate(mockEnumBuilder.Object);

        // Assert
        result.ShouldBe(expectedCode);
        mockEnumBuilder.Verify(b => b.Build(), Times.Once);
    }

    [Fact]
    public void GenerateCompilationUnitWithSingleBuilder()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        var mockBuilder = new Mock<ICodeBuilder>();
        var expectedCode = "public class TestClass { }";
        mockBuilder.Setup(b => b.Build()).Returns(expectedCode);
        var builders = new[] { mockBuilder.Object };

        // Act
        var result = generator.GenerateCompilationUnit(builders);

        // Assert
        result.ShouldBe(expectedCode);
    }

    [Fact]
    public void GenerateCompilationUnitWithMultipleBuilders()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        
        var mockBuilder1 = new Mock<ICodeBuilder>();
        var code1 = "public class TestClass1 { }";
        mockBuilder1.Setup(b => b.Build()).Returns(code1);
        
        var mockBuilder2 = new Mock<ICodeBuilder>();
        var code2 = "public class TestClass2 { }";
        mockBuilder2.Setup(b => b.Build()).Returns(code2);
        
        var builders = new[] { mockBuilder1.Object, mockBuilder2.Object };

        // Act
        var result = generator.GenerateCompilationUnit(builders);

        // Assert
        var expected = $"{code1}\r\n\r\n{code2}";
        result.ShouldBe(expected);
    }

    [Fact]
    public void GenerateCompilationUnitWithThreeBuilders()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        
        var mockBuilder1 = new Mock<ICodeBuilder>();
        var code1 = "using System;";
        mockBuilder1.Setup(b => b.Build()).Returns(code1);
        
        var mockBuilder2 = new Mock<ICodeBuilder>();
        var code2 = "namespace TestNamespace;";
        mockBuilder2.Setup(b => b.Build()).Returns(code2);
        
        var mockBuilder3 = new Mock<ICodeBuilder>();
        var code3 = "public class TestClass { }";
        mockBuilder3.Setup(b => b.Build()).Returns(code3);
        
        var builders = new[] { mockBuilder1.Object, mockBuilder2.Object, mockBuilder3.Object };

        // Act
        var result = generator.GenerateCompilationUnit(builders);

        // Assert
        var expected = $"{code1}\r\n\r\n{code2}\r\n\r\n{code3}";
        result.ShouldBe(expected);
    }

    [Fact]
    public void GenerateCompilationUnitWithEmptyCollection()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        var builders = Enumerable.Empty<ICodeBuilder>();

        // Act
        var result = generator.GenerateCompilationUnit(builders);

        // Assert
        result.ShouldBe("");
    }

    [Fact]
    public void GenerateCompilationUnitCallsBuildOnAllBuilders()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        
        var mockBuilder1 = new Mock<ICodeBuilder>();
        mockBuilder1.Setup(b => b.Build()).Returns("Code1");
        
        var mockBuilder2 = new Mock<ICodeBuilder>();
        mockBuilder2.Setup(b => b.Build()).Returns("Code2");
        
        var mockBuilder3 = new Mock<ICodeBuilder>();
        mockBuilder3.Setup(b => b.Build()).Returns("Code3");
        
        var builders = new[] { mockBuilder1.Object, mockBuilder2.Object, mockBuilder3.Object };

        // Act
        generator.GenerateCompilationUnit(builders);

        // Assert
        mockBuilder1.Verify(b => b.Build(), Times.Once);
        mockBuilder2.Verify(b => b.Build(), Times.Once);
        mockBuilder3.Verify(b => b.Build(), Times.Once);
    }

    [Fact]
    public void GenerateCompilationUnitHandlesBuilderWithEmptyCode()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        
        var mockBuilder1 = new Mock<ICodeBuilder>();
        mockBuilder1.Setup(b => b.Build()).Returns("public class TestClass1 { }");
        
        var mockBuilder2 = new Mock<ICodeBuilder>();
        mockBuilder2.Setup(b => b.Build()).Returns("");
        
        var mockBuilder3 = new Mock<ICodeBuilder>();
        mockBuilder3.Setup(b => b.Build()).Returns("public class TestClass3 { }");
        
        var builders = new[] { mockBuilder1.Object, mockBuilder2.Object, mockBuilder3.Object };

        // Act
        var result = generator.GenerateCompilationUnit(builders);

        // Assert
        result.ShouldContain("public class TestClass1 { }");
        result.ShouldContain("public class TestClass3 { }");
        var lines = result.Split(NewLineSeparators, StringSplitOptions.None);
        lines.ShouldContain("");
    }

    [Fact]
    public void GenerateCompilationUnitHandlesBuilderWithWhitespaceCode()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        
        var mockBuilder1 = new Mock<ICodeBuilder>();
        mockBuilder1.Setup(b => b.Build()).Returns("public class TestClass1 { }");
        
        var mockBuilder2 = new Mock<ICodeBuilder>();
        mockBuilder2.Setup(b => b.Build()).Returns("   ");
        
        var mockBuilder3 = new Mock<ICodeBuilder>();
        mockBuilder3.Setup(b => b.Build()).Returns("public class TestClass3 { }");
        
        var builders = new[] { mockBuilder1.Object, mockBuilder2.Object, mockBuilder3.Object };

        // Act
        var result = generator.GenerateCompilationUnit(builders);

        // Assert
        result.ShouldContain("public class TestClass1 { }");
        result.ShouldContain("   ");
        result.ShouldContain("public class TestClass3 { }");
    }

    [Fact]
    public void GenerateCompilationUnitPreservesBuilderOrder()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        var builders = new List<ICodeBuilder>();
        
        for (int i = 1; i <= 5; i++)
        {
            var mockBuilder = new Mock<ICodeBuilder>();
            mockBuilder.Setup(b => b.Build()).Returns($"Code{i}");
            builders.Add(mockBuilder.Object);
        }

        // Act
        var result = generator.GenerateCompilationUnit(builders);

        // Assert
        var expected = "Code1\r\n\r\nCode2\r\n\r\nCode3\r\n\r\nCode4\r\n\r\nCode5";
        result.ShouldBe(expected);
    }

    [Fact]
    public void GenerateCompilationUnitWithComplexCode()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        
        var mockBuilder1 = new Mock<ICodeBuilder>();
        var code1 = "using System;\r\nusing System.Collections.Generic;";
        mockBuilder1.Setup(b => b.Build()).Returns(code1);
        
        var mockBuilder2 = new Mock<ICodeBuilder>();
        var code2 = "namespace TestNamespace;";
        mockBuilder2.Setup(b => b.Build()).Returns(code2);
        
        var mockBuilder3 = new Mock<ICodeBuilder>();
        var code3 = "public class TestClass\r\n{\r\n    public string Name { get; set; }\r\n\r\n    public void DoSomething()\r\n    {\r\n        Console.WriteLine(\"Hello\");\r\n    }\r\n}";
        mockBuilder3.Setup(b => b.Build()).Returns(code3);
        
        var builders = new[] { mockBuilder1.Object, mockBuilder2.Object, mockBuilder3.Object };

        // Act
        var result = generator.GenerateCompilationUnit(builders);

        // Assert
        result.ShouldStartWith("using System;\r\nusing System.Collections.Generic;\r\n\r\nnamespace TestNamespace;");
        result.ShouldContain("public class TestClass");
        result.ShouldContain("public string Name { get; set; }");
        result.ShouldContain("public void DoSomething()");
        result.ShouldContain("Console.WriteLine(\"Hello\");");
    }

    [Fact]
    public void GenerateFromSyntaxTreeHandlesNullSourceText()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        var mockSyntaxTree = new Mock<ISyntaxTree>();
        mockSyntaxTree.Setup(t => t.SourceText).Returns((string)null!);

        // Act
        var result = generator.Generate(mockSyntaxTree.Object);

        // Assert
        result.ShouldBe(null);
    }

    [Fact]
    public void GenerateFromSyntaxTreeHandlesEmptySourceText()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        var mockSyntaxTree = new Mock<ISyntaxTree>();
        mockSyntaxTree.Setup(t => t.SourceText).Returns("");

        // Act
        var result = generator.Generate(mockSyntaxTree.Object);

        // Assert
        result.ShouldBe("");
    }

    [Fact]
    public void GenerateFromSyntaxTreeWithComplexSourceText()
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        var mockSyntaxTree = new Mock<ISyntaxTree>();
        var complexSource = @"using System;
using System.Collections.Generic;

namespace TestNamespace
{
    public class ComplexClass<T> where T : class, new()
    {
        private readonly List<T> _items = new List<T>();
        
        public IReadOnlyList<T> Items => _items.AsReadOnly();
        
        public void AddItem(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            
            _items.Add(item);
        }
    }
}";
        mockSyntaxTree.Setup(t => t.SourceText).Returns(complexSource);

        // Act
        var result = generator.Generate(mockSyntaxTree.Object);

        // Assert
        result.ShouldBe(complexSource);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public void GenerateCompilationUnitHandlesVariableNumberOfBuilders(int builderCount)
    {
        // Arrange
        var generator = new CSharpCodeGenerator();
        var builders = new List<ICodeBuilder>();
        
        for (int i = 0; i < builderCount; i++)
        {
            var mockBuilder = new Mock<ICodeBuilder>();
            mockBuilder.Setup(b => b.Build()).Returns($"public class Class{i} {{ }}");
            builders.Add(mockBuilder.Object);
        }

        // Act
        var result = generator.GenerateCompilationUnit(builders);

        // Assert
        var lines = result.Split(NewLineSeparators, StringSplitOptions.RemoveEmptyEntries);
        
        if (builderCount == 1)
        {
            lines.Length.ShouldBe(1);
        }
        else
        {
            // Each builder generates one line, plus (n-1) separator lines
            lines.Length.ShouldBe(builderCount);
        }
        
        for (int i = 0; i < builderCount; i++)
        {
            result.ShouldContain($"public class Class{i} {{ }}");
        }
    }
}