using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Moq;
using FractalDataWorks.CodeBuilder;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.CSharp;

namespace FractalDataWorks.CodeBuilder.Tests;

public class LanguageRegistryTests
{
    [Fact]
    public void ConstructorRegistersDefaultCSharpParser()
    {
        // Arrange & Act
        var registry = new LanguageRegistry();

        // Assert
        registry.SupportedLanguages.ShouldContain("csharp");
        registry.IsSupported("csharp").ShouldBeTrue();
        registry.GetExtensions("csharp").ShouldContain(".cs");
        registry.GetExtensions("csharp").ShouldContain(".csx");
        registry.GetLanguageByExtension(".cs").ShouldBe("csharp");
        registry.GetLanguageByExtension(".csx").ShouldBe("csharp");
    }

    [Fact]
    public void SupportedLanguagesReturnsAlphabeticalList()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser1 = new Mock<ICodeParser>();
        var mockParser2 = new Mock<ICodeParser>();
        
        // Act
        registry.RegisterParser("zebra", mockParser1.Object);
        registry.RegisterParser("alpha", mockParser2.Object);
        var languages = registry.SupportedLanguages;

        // Assert
        languages.Count.ShouldBe(3); // alpha, csharp, zebra
        languages[0].ShouldBe("alpha");
        languages[1].ShouldBe("csharp");
        languages[2].ShouldBe("zebra");
    }

    [Theory]
    [InlineData("csharp", true)]
    [InlineData("CSHARP", true)]
    [InlineData("CSharp", true)]
    [InlineData("javascript", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsSupportedHandlesCaseInsensitiveComparison(string? language, bool expected)
    {
        // Arrange
        var registry = new LanguageRegistry();

        // Act
        var result = registry.IsSupported(language!);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void GetExtensionsReturnsCorrectExtensions()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser("javascript", mockParser.Object, ".js", ".mjs", ".ts");
        var extensions = registry.GetExtensions("javascript");

        // Assert
        extensions.Count.ShouldBe(3);
        extensions.ShouldContain(".js");
        extensions.ShouldContain(".mjs");
        extensions.ShouldContain(".ts");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("unknown")]
    public void GetExtensionsForUnknownLanguageReturnsEmpty(string? language)
    {
        // Arrange
        var registry = new LanguageRegistry();

        // Act
        var extensions = registry.GetExtensions(language!);

        // Assert
        extensions.Count.ShouldBe(0);
    }

    [Fact]
    public void GetExtensionsIsCaseInsensitive()
    {
        // Arrange
        var registry = new LanguageRegistry();

        // Act
        var extensions1 = registry.GetExtensions("csharp");
        var extensions2 = registry.GetExtensions("CSHARP");
        var extensions3 = registry.GetExtensions("CSharp");

        // Assert
        extensions1.ShouldBe(extensions2);
        extensions2.ShouldBe(extensions3);
    }

    [Theory]
    [InlineData(".cs", "csharp")]
    [InlineData("CS", "csharp")]
    [InlineData("cs", "csharp")]
    [InlineData(".csx", "csharp")]
    [InlineData("csx", "csharp")]
    [InlineData(".unknown", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void GetLanguageByExtensionReturnsCorrectLanguage(string? extension, string? expected)
    {
        // Arrange
        var registry = new LanguageRegistry();

        // Act
        var result = registry.GetLanguageByExtension(extension!);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void GetLanguageByExtensionHandlesExtensionsWithoutDot()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser("javascript", mockParser.Object, "js", "ts");
        var result1 = registry.GetLanguageByExtension("js");
        var result2 = registry.GetLanguageByExtension(".js");

        // Assert
        result1.ShouldBe("javascript");
        result2.ShouldBe("javascript");
    }

    [Fact]
    public async Task GetParserAsyncReturnsCorrectParser()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser("javascript", mockParser.Object);
        var result = await registry.GetParserAsync("javascript", TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(mockParser.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("unknown")]
    public async Task GetParserAsyncForUnknownLanguageReturnsNull(string? language)
    {
        // Arrange
        var registry = new LanguageRegistry();

        // Act
        var result = await registry.GetParserAsync(language!, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetParserAsyncIsCaseInsensitive()
    {
        // Arrange
        var registry = new LanguageRegistry();

        // Act
        var result1 = await registry.GetParserAsync("csharp", TestContext.Current.CancellationToken);
        var result2 = await registry.GetParserAsync("CSHARP", TestContext.Current.CancellationToken);
        var result3 = await registry.GetParserAsync("CSharp", TestContext.Current.CancellationToken);

        // Assert
        result1.ShouldNotBeNull();
        result2.ShouldNotBeNull();
        result3.ShouldNotBeNull();
        result1.ShouldBeSameAs(result2);
        result2.ShouldBeSameAs(result3);
    }

    [Fact]
    public async Task GetParserAsyncRespectsCancellation()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await registry.GetParserAsync("csharp", cancellationTokenSource.Token));
    }

    [Fact]
    public void RegisterParserThrowsForNullOrEmptyLanguage()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser = new Mock<ICodeParser>();

        // Act & Assert
        Should.Throw<ArgumentException>(() => registry.RegisterParser("", mockParser.Object))
            .ParamName.ShouldBe("language");
        
        Should.Throw<ArgumentException>(() => registry.RegisterParser(null!, mockParser.Object))
            .ParamName.ShouldBe("language");
    }

    [Fact]
    public void RegisterParserAcceptsNullParser()
    {
        // Arrange
        var registry = new LanguageRegistry();

        // Act & Assert
        Should.NotThrow(() => registry.RegisterParser("test", null!));
    }

    [Fact]
    public void RegisterParserWithoutExtensions()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser("test", mockParser.Object);

        // Assert
        registry.IsSupported("test").ShouldBeTrue();
        registry.GetExtensions("test").Count.ShouldBe(0);
    }

    [Fact]
    public void RegisterParserWithNullExtensions()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser("test", mockParser.Object, null!);

        // Assert
        registry.IsSupported("test").ShouldBeTrue();
        registry.GetExtensions("test").Count.ShouldBe(0);
    }

    [Fact]
    public void RegisterParserWithEmptyExtensions()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser("test", mockParser.Object, Array.Empty<string>());

        // Assert
        registry.IsSupported("test").ShouldBeTrue();
        registry.GetExtensions("test").Count.ShouldBe(0);
    }

    [Fact]
    public void RegisterParserIgnoresEmptyExtensions()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser("test", mockParser.Object, ".valid", "", "   ", null!, ".another");

        // Assert
        var extensions = registry.GetExtensions("test");
        extensions.Count.ShouldBe(2);
        extensions.ShouldContain(".valid");
        extensions.ShouldContain(".another");
    }

    [Fact]
    public async Task RegisterParserOverwritesExistingLanguage()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser1 = new Mock<ICodeParser>();
        var mockParser2 = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser("test", mockParser1.Object, ".ext1");
        registry.RegisterParser("test", mockParser2.Object, ".ext2");

        // Assert
        var parser = await registry.GetParserAsync("test", TestContext.Current.CancellationToken);
        parser.ShouldBe(mockParser2.Object);
        
        var extensions = registry.GetExtensions("test");
        extensions.ShouldContain(".ext2");
        extensions.ShouldContain(".ext1"); // Old extensions should still be there
    }

    [Fact]
    public void RegisterParserAddsExtensionsToExistingLanguage()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser("test", mockParser.Object, ".ext1");
        registry.RegisterParser("test", mockParser.Object, ".ext2", ".ext3");

        // Assert
        var extensions = registry.GetExtensions("test");
        extensions.Count.ShouldBe(3);
        extensions.ShouldContain(".ext1");
        extensions.ShouldContain(".ext2");
        extensions.ShouldContain(".ext3");
    }

    [Fact]
    public void RegisterParserHandlesDuplicateExtensions()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser("test", mockParser.Object, ".ext1", ".ext2");
        registry.RegisterParser("test", mockParser.Object, ".ext1", ".ext3");

        // Assert
        var extensions = registry.GetExtensions("test");
        extensions.Count.ShouldBe(3);
        extensions.Count(e => e == ".ext1").ShouldBe(1); // No duplicates
        extensions.ShouldContain(".ext2");
        extensions.ShouldContain(".ext3");
    }

    [Fact]
    public void ExtensionMappingIsCaseInsensitive()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser("test", mockParser.Object, ".EXT");

        // Assert
        registry.GetLanguageByExtension(".ext").ShouldBe("test");
        registry.GetLanguageByExtension(".EXT").ShouldBe("test");
        registry.GetLanguageByExtension("ext").ShouldBe("test");
        registry.GetLanguageByExtension("EXT").ShouldBe("test");
    }

    [Fact]
    public void ExtensionCanOnlyMapToOneLanguage()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser1 = new Mock<ICodeParser>();
        var mockParser2 = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser("lang1", mockParser1.Object, ".ext");
        registry.RegisterParser("lang2", mockParser2.Object, ".ext");

        // Assert
        // The last registration should win
        registry.GetLanguageByExtension(".ext").ShouldBe("lang2");
    }

    [Fact]
    public async Task ComplexRegistrationScenario()
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockCSharpParser = new Mock<ICodeParser>();
        var mockJavaScriptParser = new Mock<ICodeParser>();
        var mockTypeScriptParser = new Mock<ICodeParser>();
        var mockPythonParser = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser("javascript", mockJavaScriptParser.Object, ".js", ".mjs");
        registry.RegisterParser("typescript", mockTypeScriptParser.Object, ".ts", ".tsx");
        registry.RegisterParser("python", mockPythonParser.Object, ".py", ".pyw");

        // Assert
        var languages = registry.SupportedLanguages;
        languages.Count.ShouldBe(4); // csharp + 3 new ones
        languages.ShouldContain("csharp");
        languages.ShouldContain("javascript");
        languages.ShouldContain("typescript");
        languages.ShouldContain("python");

        // Test extensions
        registry.GetLanguageByExtension(".js").ShouldBe("javascript");
        registry.GetLanguageByExtension(".ts").ShouldBe("typescript");
        registry.GetLanguageByExtension(".py").ShouldBe("python");
        registry.GetLanguageByExtension(".cs").ShouldBe("csharp");

        // Test parser retrieval
        (await registry.GetParserAsync("javascript", TestContext.Current.CancellationToken)).ShouldBe(mockJavaScriptParser.Object);
        (await registry.GetParserAsync("typescript", TestContext.Current.CancellationToken)).ShouldBe(mockTypeScriptParser.Object);
        (await registry.GetParserAsync("python", TestContext.Current.CancellationToken)).ShouldBe(mockPythonParser.Object);
    }

    [Fact]
    public void SupportedLanguagesReturnsImmutableCollection()
    {
        // Arrange
        var registry = new LanguageRegistry();

        // Act
        var languages = registry.SupportedLanguages;

        // Assert
        languages.ShouldNotBeNull();
        languages.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void GetExtensionsReturnsImmutableCollection()
    {
        // Arrange
        var registry = new LanguageRegistry();

        // Act
        var extensions = registry.GetExtensions("csharp");

        // Assert
        extensions.ShouldNotBeNull();
        extensions.Count.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("test")]
    [InlineData("TEST")]
    [InlineData("Test")]
    public void LanguageRegistrationIsCaseInsensitive(string language)
    {
        // Arrange
        var registry = new LanguageRegistry();
        var mockParser = new Mock<ICodeParser>();

        // Act
        registry.RegisterParser(language, mockParser.Object, ".ext");

        // Assert
        registry.IsSupported("test").ShouldBeTrue();
        registry.IsSupported("TEST").ShouldBeTrue();
        registry.IsSupported("Test").ShouldBeTrue();
        
        registry.GetLanguageByExtension(".ext").ShouldBe(language.ToLowerInvariant());
    }
}