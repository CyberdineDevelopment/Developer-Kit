using System;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using FractalDataWorks.CodeBuilder.Parsing;

namespace FractalDataWorks.CodeBuilder.Tests;

public class RoslynCSharpParserTests
{
    [Fact]
    public void LanguageIsCSharp()
    {
        // Arrange
        var parser = new RoslynCSharpParser();

        // Act
        var language = parser.Language;

        // Assert
        language.ShouldBe("csharp");
    }

    [Fact]
    public async Task ParseValidCSharpCodeReturnsSuccess()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = "public class TestClass { }";

        // Act
        var result = await parser.ParseAsync(sourceCode);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.SourceText.ShouldBe(sourceCode);
        result.Value.Language.ShouldBe("csharp");
    }

    [Fact]
    public async Task ParseComplexCSharpCodeReturnsSuccess()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    public class TestClass<T> where T : class, new()
    {
        private readonly List<T> _items = new List<T>();
        
        public IReadOnlyList<T> Items => _items.AsReadOnly();
        
        public async Task<T> GetFirstItemAsync()
        {
            await Task.Delay(100);
            return _items.FirstOrDefault();
        }
    }
}";

        // Act
        var result = await parser.ParseAsync(sourceCode);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.SourceText.ShouldBe(sourceCode);
        result.Value.Language.ShouldBe("csharp");
    }

    [Fact]
    public async Task ParseWithFilePathReturnsSuccessWithFilePath()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = "public class TestClass { }";
        var filePath = "TestClass.cs";

        // Act
        var result = await parser.ParseAsync(sourceCode, filePath);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.FilePath.ShouldBe(filePath);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ParseEmptyOrNullSourceCodeReturnsFailure(string? sourceCode)
    {
        // Arrange
        var parser = new RoslynCSharpParser();

        // Act
        var result = await parser.ParseAsync(sourceCode!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Source code cannot be null or empty");
    }

    [Fact]
    public async Task ParseInvalidCSharpCodeReturnsSuccess()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = "public class TestClass { invalid syntax }";

        // Act
        var result = await parser.ParseAsync(sourceCode);

        // Assert
        // Parser should still succeed even with syntax errors
        // Errors are detected at the syntax tree level
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.HasErrors.ShouldBeTrue();
    }

    [Fact]
    public async Task ParseWithCancellationTokenRespectsCancellation()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = "public class TestClass { }";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var result = await parser.ParseAsync(sourceCode, null, cancellationTokenSource.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Parse operation was cancelled");
    }

    [Fact]
    public async Task ValidateValidCSharpCodeReturnsSuccess()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = "public class TestClass { }";

        // Act
        var result = await parser.ValidateAsync(sourceCode);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateInvalidCSharpCodeReturnsFailure()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = "public class TestClass { invalid syntax }";

        // Act
        var result = await parser.ValidateAsync(sourceCode);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldContain("syntax error");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ValidateEmptyOrNullSourceCodeReturnsFailure(string? sourceCode)
    {
        // Arrange
        var parser = new RoslynCSharpParser();

        // Act
        var result = await parser.ValidateAsync(sourceCode!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Source code cannot be null or empty");
    }

    [Fact]
    public async Task ValidateWithCancellationTokenRespectsCancellation()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = "public class TestClass { }";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var result = await parser.ValidateAsync(sourceCode, cancellationTokenSource.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldBe("Parse operation was cancelled");
    }

    [Fact]
    public async Task ParseCSharpWithMultipleSyntaxErrors()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = @"
public class TestClass 
{
    public void Method1( // Missing closing parenthesis
    
    public void Method2()
    {
        var x = ; // Invalid assignment
        
        // Missing closing brace for method
        
    // Missing closing brace for class
";

        // Act
        var parseResult = await parser.ParseAsync(sourceCode);
        var validateResult = await parser.ValidateAsync(sourceCode);

        // Assert
        parseResult.IsSuccess.ShouldBeTrue();
        parseResult.Value!.HasErrors.ShouldBeTrue();
        
        validateResult.IsFailure.ShouldBeTrue();
        validateResult.Message.ShouldContain("syntax error");
    }

    [Fact]
    public async Task ParseCSharpWithWarnings()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = @"
using System;

public class TestClass
{
    public void Method()
    {
        var unusedVariable = 42; // This may generate a warning but not an error
        Console.WriteLine(""Hello"");
    }
}";

        // Act
        var parseResult = await parser.ParseAsync(sourceCode);
        var validateResult = await parser.ValidateAsync(sourceCode);

        // Assert
        parseResult.IsSuccess.ShouldBeTrue();
        parseResult.Value!.HasErrors.ShouldBeFalse(); // Warnings are not errors
        
        validateResult.IsSuccess.ShouldBeTrue(); // Warnings don't fail validation
    }

    [Fact]
    public async Task ParseMinimalValidCSharpClass()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = "class C{}";

        // Act
        var result = await parser.ParseAsync(sourceCode);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task ParseCSharpInterface()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = @"
public interface ITestInterface
{
    string Name { get; set; }
    Task<int> GetValueAsync();
}";

        // Act
        var result = await parser.ParseAsync(sourceCode);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task ParseCSharpEnum()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = @"
public enum TestEnum
{
    None = 0,
    First = 1,
    Second = 2,
    Combined = First | Second
}";

        // Act
        var result = await parser.ParseAsync(sourceCode);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task ParseCSharpStruct()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = @"
public struct TestStruct
{
    public int X { get; set; }
    public int Y { get; set; }
    
    public TestStruct(int x, int y)
    {
        X = x;
        Y = y;
    }
}";

        // Act
        var result = await parser.ParseAsync(sourceCode);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task ParseCSharpWithLinqQuery()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = @"
using System.Linq;
using System.Collections.Generic;

public class TestClass
{
    public IEnumerable<int> GetEvenNumbers(IEnumerable<int> numbers)
    {
        return from n in numbers
               where n % 2 == 0
               select n;
    }
}";

        // Act
        var result = await parser.ParseAsync(sourceCode);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task ParseCSharpWithAsyncAwait()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = @"
using System;
using System.Threading.Tasks;

public class TestClass
{
    public async Task<string> GetDataAsync()
    {
        await Task.Delay(1000);
        return await FetchDataFromApiAsync();
    }
    
    private async Task<string> FetchDataFromApiAsync()
    {
        return await Task.FromResult(""data"");
    }
}";

        // Act
        var result = await parser.ParseAsync(sourceCode);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task ParseCSharpWithGenerics()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = @"
using System;
using System.Collections.Generic;

public class Repository<T> where T : class, new()
{
    private readonly Dictionary<int, T> _items = new Dictionary<int, T>();
    
    public void Add<TKey>(TKey key, T item) where TKey : IComparable<TKey>
    {
        // Implementation
    }
}";

        // Act
        var result = await parser.ParseAsync(sourceCode);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task ParseCSharpWithAttributes()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = @"
using System;
using System.ComponentModel.DataAnnotations;

[Serializable]
public class TestClass
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [Obsolete(""Use NewMethod instead"")]
    public void OldMethod()
    {
    }
}";

        // Act
        var result = await parser.ParseAsync(sourceCode);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task ParseCSharpWithXmlDocumentation()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = @"
/// <summary>
/// Test class with XML documentation
/// </summary>
public class TestClass
{
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    /// <value>The name value</value>
    public string Name { get; set; }
    
    /// <summary>
    /// Performs a test operation
    /// </summary>
    /// <param name=""value"">The input value</param>
    /// <returns>The result</returns>
    public int TestMethod(string value)
    {
        return value.Length;
    }
}";

        // Act
        var result = await parser.ParseAsync(sourceCode);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task ParseCSharpWithPreprocessorDirectives()
    {
        // Arrange
        var parser = new RoslynCSharpParser();
        var sourceCode = @"
#define DEBUG
#undef TRACE

using System;

public class TestClass
{
#if DEBUG
    public void DebugMethod()
    {
        Console.WriteLine(""Debug mode"");
    }
#endif

#region Helper Methods
    private void HelperMethod()
    {
        // Helper implementation
    }
#endregion
}";

        // Act
        var result = await parser.ParseAsync(sourceCode);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.HasErrors.ShouldBeFalse();
    }
}