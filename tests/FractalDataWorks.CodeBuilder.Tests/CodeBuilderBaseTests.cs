using System;
using Shouldly;
using Xunit;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.CSharp;

namespace FractalDataWorks.CodeBuilder.Tests;

public class CodeBuilderBaseTests
{
    private sealed class TestCodeBuilder : CodeBuilderBase
    {
        private readonly string _testContent;

        public TestCodeBuilder(string testContent = "")
        {
            _testContent = testContent;
        }

        public override string Build()
        {
            Clear();
            if (!string.IsNullOrEmpty(_testContent))
                AppendLine(_testContent);
            return Builder.ToString();
        }

        // Expose protected members for testing
        public new void AppendLine(string line) => base.AppendLine(line);
        public new void Append(string text) => base.Append(text);
        public new void Indent() => base.Indent();
        public new void Outdent() => base.Outdent();
        public new void Clear() => base.Clear();
        public string GetCurrentContent() => Builder.ToString();
    }

    [Fact]
    public void DefaultIndentStringIsFourSpaces()
    {
        // Arrange & Act
        var builder = new TestCodeBuilder();

        // Assert
        builder.IndentString.ShouldBe("    ");
    }

    [Fact]
    public void IndentStringCanBeChanged()
    {
        // Arrange
        var builder = new TestCodeBuilder();
        var customIndent = "\t";

        // Act
        builder.IndentString = customIndent;

        // Assert
        builder.IndentString.ShouldBe(customIndent);
    }

    [Fact]
    public void InitialIndentLevelIsZero()
    {
        // Arrange & Act
        var builder = new TestCodeBuilder();

        // Assert
        builder.IndentLevel.ShouldBe(0);
    }

    [Fact]
    public void IndentIncreasesIndentLevel()
    {
        // Arrange
        var builder = new TestCodeBuilder();

        // Act
        builder.Indent();

        // Assert
        builder.IndentLevel.ShouldBe(1);
    }

    [Fact]
    public void MultipleIndentsIncreaseIndentLevel()
    {
        // Arrange
        var builder = new TestCodeBuilder();

        // Act
        builder.Indent();
        builder.Indent();
        builder.Indent();

        // Assert
        builder.IndentLevel.ShouldBe(3);
    }

    [Fact]
    public void OutdentDecreasesIndentLevel()
    {
        // Arrange
        var builder = new TestCodeBuilder();
        builder.Indent();
        builder.Indent();

        // Act
        builder.Outdent();

        // Assert
        builder.IndentLevel.ShouldBe(1);
    }

    [Fact]
    public void OutdentDoesNotGoBelowZero()
    {
        // Arrange
        var builder = new TestCodeBuilder();

        // Act
        builder.Outdent();
        builder.Outdent();

        // Assert
        builder.IndentLevel.ShouldBe(0);
    }

    [Fact]
    public void AppendLineWithoutIndentationAddsLineWithoutPadding()
    {
        // Arrange
        var builder = new TestCodeBuilder();
        var testLine = "test line";

        // Act
        builder.AppendLine(testLine);

        // Assert
        builder.GetCurrentContent().ShouldBe(testLine + Environment.NewLine);
    }

    [Fact]
    public void AppendLineWithIndentationAddsProperPadding()
    {
        // Arrange
        var builder = new TestCodeBuilder();
        var testLine = "test line";
        builder.Indent();

        // Act
        builder.AppendLine(testLine);

        // Assert
        var expected = "    " + testLine + Environment.NewLine;
        builder.GetCurrentContent().ShouldBe(expected);
    }

    [Fact]
    public void AppendLineWithMultipleIndentationLevels()
    {
        // Arrange
        var builder = new TestCodeBuilder();
        var testLine = "test line";
        builder.Indent();
        builder.Indent();

        // Act
        builder.AppendLine(testLine);

        // Assert
        var expected = "        " + testLine + Environment.NewLine;
        builder.GetCurrentContent().ShouldBe(expected);
    }

    [Fact]
    public void AppendLineWithCustomIndentString()
    {
        // Arrange
        var builder = new TestCodeBuilder();
        builder.IndentString = "\t";
        var testLine = "test line";
        builder.Indent();

        // Act
        builder.AppendLine(testLine);

        // Assert
        var expected = "\t" + testLine + Environment.NewLine;
        builder.GetCurrentContent().ShouldBe(expected);
    }

    [Fact]
    public void AppendLineWithEmptyStringAddsEmptyLine()
    {
        // Arrange
        var builder = new TestCodeBuilder();

        // Act
        builder.AppendLine("");

        // Assert
        builder.GetCurrentContent().ShouldBe(Environment.NewLine);
    }

    [Fact]
    public void AppendLineWithWhitespaceStringAddsEmptyLine()
    {
        // Arrange
        var builder = new TestCodeBuilder();

        // Act
        builder.AppendLine("   ");

        // Assert
        builder.GetCurrentContent().ShouldBe(Environment.NewLine);
    }

    [Fact]
    public void AppendLineWithNullStringAddsEmptyLine()
    {
        // Arrange
        var builder = new TestCodeBuilder();

        // Act
        builder.AppendLine(null!);

        // Assert
        builder.GetCurrentContent().ShouldBe(Environment.NewLine);
    }

    [Fact]
    public void AppendTextWithoutNewline()
    {
        // Arrange
        var builder = new TestCodeBuilder();
        var testText = "test text";

        // Act
        builder.Append(testText);

        // Assert
        builder.GetCurrentContent().ShouldBe(testText);
    }

    [Fact]
    public void AppendMultipleTextsWithoutNewlines()
    {
        // Arrange
        var builder = new TestCodeBuilder();

        // Act
        builder.Append("first");
        builder.Append("second");

        // Assert
        builder.GetCurrentContent().ShouldBe("firstsecond");
    }

    [Fact]
    public void ClearResetsBuilderContent()
    {
        // Arrange
        var builder = new TestCodeBuilder();
        builder.AppendLine("test");
        builder.Indent();

        // Act
        builder.Clear();

        // Assert
        builder.GetCurrentContent().ShouldBe("");
        builder.IndentLevel.ShouldBe(0);
    }

    [Fact]
    public void BuildMethodCallsClear()
    {
        // Arrange
        var builder = new TestCodeBuilder("test content");
        builder.AppendLine("existing content");

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe("test content" + Environment.NewLine);
    }

    [Theory]
    [InlineData("")]
    [InlineData("single line")]
    [InlineData("line one\nline two")]
    [InlineData("  leading spaces")]
    [InlineData("trailing spaces  ")]
    public void BuildHandlesVariousContentTypes(string content)
    {
        // Arrange
        var builder = new TestCodeBuilder(content);

        // Act
        var result = builder.Build();

        // Assert
        if (string.IsNullOrEmpty(content))
        {
            result.ShouldBe("");
        }
        else
        {
            result.ShouldBe(content + Environment.NewLine);
        }
    }

    [Fact]
    public void ComplexIndentationScenario()
    {
        // Arrange
        var builder = new TestCodeBuilder();

        // Act
        builder.AppendLine("root level");
        builder.Indent();
        builder.AppendLine("level 1");
        builder.Indent();
        builder.AppendLine("level 2");
        builder.Outdent();
        builder.AppendLine("back to level 1");
        builder.Outdent();
        builder.AppendLine("back to root");

        // Assert
        var expected = "root level" + Environment.NewLine +
                      "    level 1" + Environment.NewLine +
                      "        level 2" + Environment.NewLine +
                      "    back to level 1" + Environment.NewLine +
                      "back to root" + Environment.NewLine;
        
        builder.GetCurrentContent().ShouldBe(expected);
    }
}