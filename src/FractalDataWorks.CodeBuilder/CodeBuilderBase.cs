using System;
using System.Text;
using FractalDataWorks.CodeBuilder.Abstractions;

namespace FractalDataWorks.CodeBuilder;

/// <summary>
/// Base class for code builders.
/// </summary>
public abstract class CodeBuilderBase : ICodeBuilder
{
    /// <summary>
    /// The string builder for constructing the code.
    /// </summary>
    protected readonly StringBuilder Builder = new();

    /// <summary>
    /// Gets or sets the indentation string.
    /// </summary>
    public string IndentString { get; set; } = "    ";

    /// <summary>
    /// Gets the current indentation level.
    /// </summary>
    public int IndentLevel { get; private set; }

    /// <summary>
    /// Builds the code and returns it as a string.
    /// </summary>
    /// <returns>The generated code.</returns>
    public abstract string Build();

    /// <summary>
    /// Appends a line with proper indentation.
    /// </summary>
    /// <param name="line">The line to append.</param>
    protected void AppendLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            Builder.AppendLine();
            return;
        }

        for (int i = 0; i < IndentLevel; i++)
        {
            Builder.Append(IndentString);
        }
        Builder.AppendLine(line);
    }

    /// <summary>
    /// Appends text without a newline.
    /// </summary>
    /// <param name="text">The text to append.</param>
    protected void Append(string text)
    {
        Builder.Append(text);
    }

    /// <summary>
    /// Increases the indentation level.
    /// </summary>
    protected void Indent()
    {
        IndentLevel++;
    }

    /// <summary>
    /// Decreases the indentation level.
    /// </summary>
    protected void Outdent()
    {
        if (IndentLevel > 0)
            IndentLevel--;
    }

    /// <summary>
    /// Clears the builder.
    /// </summary>
    protected void Clear()
    {
        Builder.Clear();
        IndentLevel = 0;
    }
}