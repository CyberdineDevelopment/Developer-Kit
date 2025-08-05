using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FractalDataWorks.CodeBuilder.CodeGeneration;

/// <summary>
/// Handles code formatting and indentation for generated code.
/// This is a utility class separate from the builder pattern, focused solely on formatting.
/// </summary>
public sealed class CodeFormatter
{
    private readonly string _indentString;
    private int _currentIndentLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeFormatter"/> class.
    /// </summary>
    /// <param name="indentString">The string to use for indentation. Defaults to 4 spaces.</param>
    public CodeFormatter(string indentString = "    ")
    {
        _indentString = indentString ?? throw new ArgumentNullException(nameof(indentString));
        _currentIndentLevel = 0;
    }

    /// <summary>
    /// Gets the current indentation level.
    /// </summary>
    public int IndentLevel => _currentIndentLevel;

    /// <summary>
    /// Gets the current indentation string for the current level.
    /// </summary>
    public string CurrentIndent => string.Concat(Enumerable.Repeat(_indentString, _currentIndentLevel));

    /// <summary>
    /// Increases the indentation level by one.
    /// Returns a disposable that will decrease the indentation when disposed.
    /// </summary>
    /// <returns>A disposable that restores the previous indentation level.</returns>
    public IDisposable IncreaseIndent()
    {
        _currentIndentLevel++;
        return new IndentScope(this);
    }

    /// <summary>
    /// Decreases the indentation level by one.
    /// </summary>
    public void DecreaseIndent()
    {
        if (_currentIndentLevel > 0)
        {
            _currentIndentLevel--;
        }
    }

    /// <summary>
    /// Indents a single line of text with the current indentation level.
    /// </summary>
    /// <param name="text">The text to indent.</param>
    /// <returns>The indented text.</returns>
    public string IndentText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return CurrentIndent + text;
    }

    /// <summary>
    /// Indents multiple lines of text with the current indentation level.
    /// </summary>
    /// <param name="text">The multi-line text to indent.</param>
    /// <returns>The indented text.</returns>
    public string IndentLines(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var sb = new StringBuilder();

        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0)
            {
                sb.AppendLine();
            }

            var line = lines[i];
            if (!string.IsNullOrWhiteSpace(line))
            {
                sb.Append(IndentText(line));
            }
            else
            {
                sb.Append(line); // Preserve empty lines as-is
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a block of code with proper indentation and braces.
    /// </summary>
    /// <param name="header">The header line (e.g., "public class MyClass").</param>
    /// <param name="body">The body content.</param>
    /// <param name="addBlankLineAfter">Whether to add a blank line after the block.</param>
    /// <returns>The formatted code block.</returns>
    public string FormatBlock(string header, string body, bool addBlankLineAfter = false)
    {
        var sb = new StringBuilder();

        sb.AppendLine(IndentText(header));
        sb.AppendLine(IndentText("{"));

        using (IncreaseIndent())
        {
            if (!string.IsNullOrEmpty(body))
            {
                sb.AppendLine(IndentLines(body));
            }
        }

        sb.Append(IndentText("}"));

        if (addBlankLineAfter)
        {
            sb.AppendLine();
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a property with automatic getter/setter.
    /// </summary>
    /// <param name="declaration">The property declaration (e.g., "public string Name").</param>
    /// <param name="hasGetter">Whether the property has a getter.</param>
    /// <param name="hasSetter">Whether the property has a setter.</param>
    /// <param name="getterAccess">Optional getter access modifier.</param>
    /// <param name="setterAccess">Optional setter access modifier.</param>
    /// <returns>The formatted property.</returns>
    public string FormatAutoProperty(
        string declaration, 
        bool hasGetter = true, 
        bool hasSetter = true,
        string? getterAccess = null,
        string? setterAccess = null)
    {
        var sb = new StringBuilder();
        sb.Append(IndentText(declaration));
        sb.Append(" { ");

        if (hasGetter)
        {
            if (!string.IsNullOrEmpty(getterAccess))
            {
                sb.Append(getterAccess);
                sb.Append(' ');
            }
            sb.Append("get; ");
        }

        if (hasSetter)
        {
            if (!string.IsNullOrEmpty(setterAccess))
            {
                sb.Append(setterAccess);
                sb.Append(' ');
            }
            sb.Append("set; ");
        }

        sb.Append('}');

        return sb.ToString();
    }

    /// <summary>
    /// Formats XML documentation comments.
    /// </summary>
    /// <param name="summary">The summary text.</param>
    /// <param name="parameters">Optional parameter documentation.</param>
    /// <param name="returns">Optional return value documentation.</param>
    /// <param name="remarks">Optional remarks.</param>
    /// <returns>The formatted XML documentation.</returns>
    public string FormatXmlDocumentation(
        string summary, 
        Dictionary<string, string>? parameters = null,
        string? returns = null,
        string? remarks = null)
    {
        var sb = new StringBuilder();

        // Summary
        sb.AppendLine(IndentText("/// <summary>"));
        
        var summaryLines = summary.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        foreach (var line in summaryLines)
        {
            sb.AppendLine(IndentText($"/// {line}"));
        }
        
        sb.AppendLine(IndentText("/// </summary>"));

        // Parameters
        if (parameters?.Count > 0)
        {
            foreach (var param in parameters)
            {
                sb.AppendLine(IndentText($"/// <param name=\"{param.Key}\">{param.Value}</param>"));
            }
        }

        // Returns
        if (!string.IsNullOrEmpty(returns))
        {
            sb.AppendLine(IndentText($"/// <returns>{returns}</returns>"));
        }

        // Remarks
        if (!string.IsNullOrEmpty(remarks))
        {
            sb.AppendLine(IndentText("/// <remarks>"));
            var remarksLines = remarks.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (var line in remarksLines)
            {
                sb.AppendLine(IndentText($"/// {line}"));
            }
            sb.AppendLine(IndentText("/// </remarks>"));
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Normalizes line endings to the specified format.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <param name="lineEnding">The line ending to use. Defaults to Environment.NewLine.</param>
    /// <returns>The normalized text.</returns>
    public static string NormalizeLineEndings(string text, string? lineEnding = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        lineEnding ??= Environment.NewLine;

        // Replace all line endings with a consistent format
        return text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Replace("\n", lineEnding);
    }

    /// <summary>
    /// Trims trailing whitespace from all lines in the text.
    /// </summary>
    /// <param name="text">The text to trim.</param>
    /// <returns>The trimmed text.</returns>
    public static string TrimTrailingWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        return string.Join(Environment.NewLine, lines.Select(line => line.TrimEnd()));
    }

    /// <summary>
    /// Ensures the text ends with a single newline.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <returns>The normalized text.</returns>
    public static string EnsureSingleTrailingNewline(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return text.TrimEnd() + Environment.NewLine;
    }

    /// <summary>
    /// Internal class for managing indentation scope.
    /// </summary>
    private sealed class IndentScope : IDisposable
    {
        private readonly CodeFormatter _formatter;
        private bool _disposed;

        public IndentScope(CodeFormatter formatter)
        {
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _formatter.DecreaseIndent();
                _disposed = true;
            }
        }
    }
}