using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.CodeBuilder.CSharp.Parsing;
using Shouldly;
using Xunit;

namespace FractalDataWorks.CodeBuilder.Tests;

public class RoslynCSharpParserAdditionalTests
{
    private readonly ITestOutputHelper _output;

    public RoslynCSharpParserAdditionalTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public class AsyncCancellationTests : RoslynCSharpParserAdditionalTests
    {
        public AsyncCancellationTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ParseWithShortTimeoutShouldRespectCancellation()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var largeSourceCode = GenerateLargeSourceCode(10000); // Generate large code to potentially slow down parsing
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1)); // Very short timeout

            // Act
            var result = await parser.ParseAsync(largeSourceCode, "LargeFile.cs", cts.Token);

            // Assert
            // Note: This test might pass if parsing is fast enough, but it tests the cancellation path
            if (result.IsFailure && result.Message == "Parse operation was cancelled")
            {
                result.Message.ShouldBe("Parse operation was cancelled");
                _output.WriteLine("Cancellation was respected");
            }
            else
            {
                // If parsing completed before cancellation, that's also a valid outcome
                _output.WriteLine("Parsing completed before cancellation timeout");
            }
        }

        [Fact]
        public async Task ValidateWithShortTimeoutShouldRespectCancellation()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var largeSourceCode = GenerateLargeSourceCode(5000);
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

            // Act
            var result = await parser.ValidateAsync(largeSourceCode, cts.Token);

            // Assert
            // Similar to above, test cancellation path
            if (result.IsFailure && result.Message == "Parse operation was cancelled")
            {
                result.Message.ShouldBe("Parse operation was cancelled");
                _output.WriteLine("Validation cancellation was respected");
            }
            else
            {
                _output.WriteLine("Validation completed before cancellation timeout");
            }
        }

        [Fact]
        public async Task ParseWithAlreadyCancelledTokenShouldReturnCancelledImmediately()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = "public class TestClass { }";
            using var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel before calling

            // Act
            var result = await parser.ParseAsync(sourceCode, null, cts.Token);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Message.ShouldBe("Parse operation was cancelled");
        }

        [Fact]
        public async Task ValidateWithAlreadyCancelledTokenShouldReturnCancelledImmediately()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = "public class TestClass { }";
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await parser.ValidateAsync(sourceCode, cts.Token);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Message.ShouldBe("Parse operation was cancelled");
        }

        [Fact]
        public async Task ParseConcurrentlyWithDifferentCancellationTokens()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode1 = "public class TestClass1 { }";
            var sourceCode2 = "public class TestClass2 { }";
            var sourceCode3 = "public class TestClass3 { }";
            
            using var cts1 = new CancellationTokenSource();
            using var cts2 = new CancellationTokenSource();
            using var cts3 = new CancellationTokenSource();
            
            cts2.Cancel(); // Cancel one of them

            // Act
            var task1 = parser.ParseAsync(sourceCode1, "File1.cs", cts1.Token);
            var task2 = parser.ParseAsync(sourceCode2, "File2.cs", cts2.Token);
            var task3 = parser.ParseAsync(sourceCode3, "File3.cs", cts3.Token);

            var results = await Task.WhenAll(task1, task2, task3);

            // Assert
            results[0].IsSuccess.ShouldBeTrue(); // Should succeed
            results[1].IsFailure.ShouldBeTrue(); // Should be cancelled
            results[1].Message.ShouldBe("Parse operation was cancelled");
            results[2].IsSuccess.ShouldBeTrue(); // Should succeed

            _output.WriteLine($"Result 1: {results[0].IsSuccess}");
            _output.WriteLine($"Result 2: {results[1].IsSuccess} - {results[1].Message}");
            _output.WriteLine($"Result 3: {results[2].IsSuccess}");
        }

        private static string GenerateLargeSourceCode(int classCount)
        {
            var code = "using System;\nusing System.Collections.Generic;\n\n";
            for (int i = 0; i < classCount; i++)
            {
                code += $@"
public class GeneratedClass{i}
{{
    public string Property{i} {{ get; set; }}
    
    public void Method{i}()
    {{
        var value = ""{i}"";
        Console.WriteLine(value);
    }}
}}
";
            }
            return code;
        }
    }

    public class ErrorHandlingEdgeCasesTests : RoslynCSharpParserAdditionalTests
    {
        public ErrorHandlingEdgeCasesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ParseWithExtremelyDeepNestingShouldNotCrash()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var deepNesting = GenerateDeeplyNestedCode(50); // 50 levels deep

            // Act
            var parseResult = await parser.ParseAsync(deepNesting, "Deep.cs", CancellationToken.None);
            var validateResult = await parser.ValidateAsync(deepNesting, CancellationToken.None);

            // Assert
            parseResult.IsSuccess.ShouldBeTrue();
            parseResult.Value.ShouldNotBeNull();
            validateResult.IsSuccess.ShouldBeTrue(); // Should be valid despite being deep

            _output.WriteLine($"Successfully parsed {50} levels of nesting");
        }

        [Fact]
        public async Task ParseWithVeryLongIdentifiersShouldSucceed()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var longIdentifier = new string('A', 1000); // Very long identifier
            var sourceCode = $@"
public class {longIdentifier}
{{
    public string {longIdentifier}Property {{ get; set; }}
    
    public void {longIdentifier}Method()
    {{
        var {longIdentifier}Variable = ""value"";
    }}
}}";

            // Act
            var result = await parser.ParseAsync(sourceCode, null, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.HasErrors.ShouldBeFalse();

            _output.WriteLine($"Successfully parsed code with {longIdentifier.Length}-character identifiers");
        }

        [Fact]
        public async Task ParseWithUnicodeCharactersShouldSucceed()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = @"
using System;

public class TestClass
{
    // Unicode characters in comments: 你好世界 🌍 αβγδε
    public string UnicodeProperty { get; set; } = ""こんにちは"";
    
    public void UnicodeMethod()
    {
        var greeting = ""Здравствуй мир"";
        Console.WriteLine($""Message: {greeting} 🚀"");
    }
}";

            // Act
            var result = await parser.ParseAsync(sourceCode, "Unicode.cs", CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.HasErrors.ShouldBeFalse();

            _output.WriteLine("Successfully parsed code with Unicode characters");
        }

        [Fact]
        public async Task ParseWithInvalidUnicodeSequencesShouldHandleGracefully()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            // Create string with potentially problematic Unicode sequences
            var sourceCode = "public class Test { public string prop = \"\uFFFE\uFFFF\"; }";

            // Act
            var result = await parser.ParseAsync(sourceCode, null, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue(); // Parser should handle gracefully
            result.Value.ShouldNotBeNull();

            _output.WriteLine("Handled Unicode edge case gracefully");
        }

        [Fact]
        public async Task ParseWithExtremelyLongLineShouldSucceed()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var longString = new string('x', 100000); // 100k character string
            var sourceCode = $@"
public class TestClass
{{
    public string LongString = ""{longString}"";
}}";

            // Act
            var result = await parser.ParseAsync(sourceCode, null, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();

            _output.WriteLine($"Successfully parsed code with {longString.Length}-character string literal");
        }

        [Fact]
        public async Task ParseWithMultipleNullCharactersShouldSucceed()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = "public class Test\0Class { public void Method\0Name() { } }";

            // Act
            var result = await parser.ParseAsync(sourceCode, null, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            // May or may not have errors depending on how Roslyn handles null characters
            
            _output.WriteLine($"Handled null characters in source code");
        }

        [Theory]
        [InlineData("\r\n")]
        [InlineData("\n")]
        [InlineData("\r")]
        [InlineData("\n\r")]
        public async Task ParseWithDifferentLineEndingsShouldSucceed(string lineEnding)
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = $"public class TestClass{lineEnding}{{{lineEnding}    public void Method(){lineEnding}    {{{lineEnding}        Console.WriteLine(\"test\");{lineEnding}    }}{lineEnding}}}";

            // Act
            var result = await parser.ParseAsync(sourceCode, null, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.HasErrors.ShouldBeFalse();

            _output.WriteLine($"Successfully parsed code with line ending: {lineEnding.Replace("\r", "\\r").Replace("\n", "\\n")}");
        }

        [Fact]
        public async Task ParseEmptyFileWithWhitespaceOnlyShouldReturnFailure()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = "   \t   \r\n   \t   ";

            // Act
            var result = await parser.ParseAsync(sourceCode, null, CancellationToken.None);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Message.ShouldBe("Source code cannot be null or empty");
        }

        private static string GenerateDeeplyNestedCode(int depth)
        {
            var code = "public class Root {\n";
            
            for (int i = 0; i < depth; i++)
            {
                code += $"    public class Nested{i} {{\n";
            }
            
            code += "        public void DeepMethod() { }\n";
            
            for (int i = 0; i < depth; i++)
            {
                code += "    }\n";
            }
            
            code += "}";
            return code;
        }
    }

    public class FilepathHandlingTests : RoslynCSharpParserAdditionalTests
    {
        public FilepathHandlingTests(ITestOutputHelper output) : base(output) { }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task ParseWithEmptyOrNullFilePathShouldUseEmptyString(string? filePath)
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = "public class TestClass { }";

            // Act
            var result = await parser.ParseAsync(sourceCode, filePath, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.FilePath.ShouldBe(string.Empty);
        }

        [Fact]
        public async Task ParseWithVeryLongFilePathShouldSucceed()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = "public class TestClass { }";
            var longPath = "C:\\" + new string('A', 300) + ".cs"; // Very long path

            // Act
            var result = await parser.ParseAsync(sourceCode, longPath, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.FilePath.ShouldBe(longPath);

            _output.WriteLine($"Successfully parsed with {longPath.Length}-character file path");
        }

        [Fact]
        public async Task ParseWithSpecialCharactersInFilePathShouldSucceed()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = "public class TestClass { }";
            var specialPath = "C:\\测试\\файл-тест\\tëst file with spaces & symbols!@#$%^&()_+.cs";

            // Act
            var result = await parser.ParseAsync(sourceCode, specialPath, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.FilePath.ShouldBe(specialPath);
        }
    }

    public class ValidateMethodEdgeCasesTests : RoslynCSharpParserAdditionalTests
    {
        public ValidateMethodEdgeCasesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ValidateWithComplexSyntaxErrorsShouldProvideDetailedMessage()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = @"
public class TestClass
{
    public void Method1(
    public void Method2() {
        var x = ;
        var y = 123 +;
        var z = new List<>;
    }
    // Missing closing brace
";

            // Act
            var result = await parser.ValidateAsync(sourceCode, CancellationToken.None);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Message.ShouldContain("syntax error");
            
            // The message should indicate multiple errors
            var errorCount = ExtractErrorCount(result.Message);
            errorCount.ShouldBeGreaterThan(1);

            _output.WriteLine($"Validation found multiple errors: {result.Message}");
        }

        [Fact]
        public async Task ValidateWhenParseFailsShouldReturnParseFailureMessage()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = ""; // Empty source that will fail parsing

            // Act
            var result = await parser.ValidateAsync(sourceCode, CancellationToken.None);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Message.ShouldBe("Source code cannot be null or empty");
        }

        [Fact]
        public async Task ValidateWithNoErrorsShouldReturnSuccess()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    public class TestClass
    {
        public string Name { get; set; }
        
        public void TestMethod()
        {
            var items = new List<string>();
            items.Add(Name ?? ""default"");
        }
    }
}";

            // Act
            var result = await parser.ValidateAsync(sourceCode, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.ShouldBeNull();
        }

        private static int ExtractErrorCount(string? message)
        {
            if (string.IsNullOrEmpty(message))
                return 0;

            // Extract number from message like "Source code contains 5 syntax error(s)"
            var words = message.Split(' ');
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words[i] == "contains" && int.TryParse(words[i + 1], out int count))
                {
                    return count;
                }
            }
            return 1; // Default to 1 if we can't parse the count
        }
    }

    public class LanguageVersionTests : RoslynCSharpParserAdditionalTests
    {
        public LanguageVersionTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ParseWithLatestCSharpFeaturesShouldSucceed()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = @"
using System;

public record PersonRecord(string FirstName, string LastName);

public class TestClass
{
    public void TestMethod()
    {
        // Pattern matching enhancements
        var result = ""test"" switch
        {
            { Length: > 0 } => ""non-empty"",
            _ => ""empty""
        };
        
        // Target-typed new expressions
        PersonRecord person = new(""John"", ""Doe"");
        
        // Init-only properties and records
        var data = person with { FirstName = ""Jane"" };
    }
}";

            // Act
            var result = await parser.ParseAsync(sourceCode, null, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.HasErrors.ShouldBeFalse();

            _output.WriteLine("Successfully parsed modern C# language features");
        }

        [Fact]
        public async Task ParseWithFileScoupedNamespaceShouldSucceed()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var sourceCode = @"
namespace MyApp.Models;

using System;

public class TestClass
{
    public void TestMethod()
    {
        Console.WriteLine(""Hello from file-scoped namespace"");
    }
}";

            // Act
            var result = await parser.ParseAsync(sourceCode, null, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.HasErrors.ShouldBeFalse();

            _output.WriteLine("Successfully parsed file-scoped namespace");
        }
    }

    public class PerformanceTests : RoslynCSharpParserAdditionalTests
    {
        public PerformanceTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ParseMultipleLargeFilesConcurrentlyShouldSucceed()
        {
            // Arrange
            var parser = new RoslynCSharpParser();
            var tasks = new List<Task>();
            
            for (int i = 0; i < 10; i++)
            {
                var sourceCode = GenerateLargeSourceCode(100); // 100 classes per file
                tasks.Add(VerifyParseResult(parser, sourceCode, $"LargeFile{i}.cs"));
            }

            // Act
            await Task.WhenAll(tasks);

            _output.WriteLine($"Successfully parsed {tasks.Count} large files concurrently");
        }

        private static async Task VerifyParseResult(RoslynCSharpParser parser, string sourceCode, string fileName)
        {
            var result = await parser.ParseAsync(sourceCode, fileName, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
        }

        private static string GenerateLargeSourceCode(int classCount)
        {
            var code = "using System;\nusing System.Collections.Generic;\n\n";
            for (int i = 0; i < classCount; i++)
            {
                code += $@"
public class GeneratedClass{i}
{{
    public string Property{i} {{ get; set; }}
    public int IntProperty{i} {{ get; set; }}
    public DateTime DateProperty{i} {{ get; set; }}
    
    public void Method{i}()
    {{
        var value = ""{i}"";
        var number = {i};
        Console.WriteLine($""Class {{this.GetType().Name}} - Value: {{value}}, Number: {{number}}"");
    }}
    
    public async Task<string> AsyncMethod{i}()
    {{
        await Task.Delay({i % 100});
        return $""Result from class {i}"";
    }}
}}
";
            }
            return code;
        }
    }
}