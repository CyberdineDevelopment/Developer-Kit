using System;
using System.Linq;
using System.Text;
using FractalDataWorks.CodeBuilder.Abstractions;

namespace FractalDataWorks.CodeBuilder.Generators;

/// <summary>
/// Utility class for formatting generated code with proper indentation and line endings.
/// </summary>
public sealed class CodeFormatter
{
    private readonly CodeGenerationOptions _options;

    /// <summary>
    /// Initializes a new instance of CodeFormatter.
    /// </summary>
    /// <param name="options">The code generation options.</param>
    public CodeFormatter(CodeGenerationOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Indents a single line of code.
    /// </summary>
    /// <param name="line">The line to indent.</param>
    /// <param name="level">The indentation level (default: 1).</param>
    /// <returns>The indented line.</returns>
    public string Indent(string line, int level = 1)
    {
        if (string.IsNullOrWhiteSpace(line))
            return line;

        var indentation = string.Concat(Enumerable.Repeat(_options.Indentation, level));
        return indentation + line.TrimStart();
    }

    /// <summary>
    /// Indents multiple lines of code.
    /// </summary>
    /// <param name="code">The code to indent.</param>
    /// <param name="level">The indentation level (default: 1).</param>
    /// <returns>The indented code.</returns>
    public string Indent(string code, int level = 1)
    {
        if (string.IsNullOrWhiteSpace(code))
            return code;

        var lines = code.Split('\n');
        var indentedLines = lines.Select(line => 
            string.IsNullOrWhiteSpace(line) ? line : Indent(line, level));

        return string.Join(GetLineEnding(), indentedLines);
    }

    /// <summary>
    /// Formats a block of code with proper bracing and indentation.
    /// </summary>
    /// <param name="content">The content to wrap in braces.</param>
    /// <param name="indentContent">Whether to indent the content (default: true).</param>
    /// <returns>The formatted block.</returns>
    public string FormatBlock(string content, bool indentContent = true)
    {
        var code = new StringBuilder();
        code.AppendLine("{");
        
        if (!string.IsNullOrWhiteSpace(content))
        {
            var formattedContent = indentContent ? Indent(content) : content;
            code.AppendLine(formattedContent);
        }
        
        code.Append("}");
        return code.ToString();
    }

    /// <summary>
    /// Wraps text to fit within the maximum line length.
    /// </summary>
    /// <param name="text">The text to wrap.</param>
    /// <param name="maxLength">The maximum line length (uses options if not specified).</param>
    /// <returns>The wrapped text.</returns>
    public string WrapText(string text, int? maxLength = null)
    {
        var limit = maxLength ?? _options.MaxLineLength;
        if (limit <= 0 || string.IsNullOrWhiteSpace(text) || text.Length <= limit)
            return text;

        var lines = text.Split('\n');
        var wrappedLines = lines.SelectMany(line => WrapLine(line, limit));
        return string.Join(GetLineEnding(), wrappedLines);
    }

    /// <summary>
    /// Formats a parameter list with proper line wrapping.
    /// </summary>
    /// <param name="parameters">The parameters to format.</param>
    /// <param name="maxLength">The maximum line length (uses options if not specified).</param>
    /// <returns>The formatted parameter list.</returns>
    public string FormatParameterList(string[] parameters, int? maxLength = null)
    {
        if (parameters == null || parameters.Length == 0)
            return "";

        var limit = maxLength ?? _options.MaxLineLength;
        var parameterList = string.Join(", ", parameters);

        if (limit <= 0 || parameterList.Length <= limit)
            return parameterList;

        // Multi-line parameter formatting
        var result = new StringBuilder();
        result.AppendLine();
        
        for (int i = 0; i < parameters.Length; i++)
        {
            result.Append(Indent(parameters[i]));
            if (i < parameters.Length - 1)
                result.Append(',');
            result.AppendLine();
        }

        return result.ToString().TrimEnd();
    }

    /// <summary>
    /// Formats using statements with proper sorting if enabled.
    /// </summary>
    /// <param name="usings">The using statements to format.</param>
    /// <returns>The formatted using statements.</returns>
    public string FormatUsings(string[] usings)
    {
        if (usings == null || usings.Length == 0)
            return "";

        var usingList = _options.SortUsings ? usings.OrderBy(u => u).ToArray() : usings;
        var result = new StringBuilder();

        foreach (var usingStatement in usingList)
        {
            if (!usingStatement.StartsWith("using "))
                result.Append("using ");
            
            result.Append(usingStatement);
            
            if (!usingStatement.EndsWith(";"))
                result.Append(';');
            
            result.AppendLine();
        }

        return result.ToString();
    }

    /// <summary>
    /// Normalizes line endings according to the options.
    /// </summary>
    /// <param name="code">The code to normalize.</param>
    /// <returns>The code with normalized line endings.</returns>
    public string NormalizeLineEndings(string code)
    {
        if (string.IsNullOrEmpty(code))
            return code;

        var lineEnding = GetLineEnding();
        return code.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", lineEnding);
    }

    /// <summary>
    /// Gets the line ending string based on options.
    /// </summary>
    /// <returns>The line ending string.</returns>
    private string GetLineEnding()
    {
        return _options.LineEndings switch
        {
            LineEndingStyle.Windows => "\r\n",
            LineEndingStyle.Unix => "\n",
            LineEndingStyle.Environment => Environment.NewLine,
            _ => Environment.NewLine
        };
    }

    /// <summary>
    /// Wraps a single line to fit within the specified length.
    /// </summary>
    /// <param name="line">The line to wrap.</param>
    /// <param name="maxLength">The maximum line length.</param>
    /// <returns>The wrapped lines.</returns>
    private static string[] WrapLine(string line, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(line) || line.Length <= maxLength)
            return new[] { line };

        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var lines = new List<string>();
        var currentLine = new StringBuilder();

        foreach (var word in words)
        {
            if (currentLine.Length == 0)
            {
                currentLine.Append(word);
            }
            else if (currentLine.Length + 1 + word.Length <= maxLength)
            {
                currentLine.Append(' ');
                currentLine.Append(word);
            }
            else
            {
                lines.Add(currentLine.ToString());
                currentLine.Clear();
                currentLine.Append(word);
            }
        }

        if (currentLine.Length > 0)
        {
            lines.Add(currentLine.ToString());
        }

        return lines.ToArray();
    }
}