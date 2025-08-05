using System;

namespace FractalDataWorks.CodeBuilder.Abstractions;

/// <summary>
/// Interface for code generators that convert AST definitions to source code strings.
/// </summary>
public interface ICodeGenerator
{
    /// <summary>
    /// Gets the language this generator supports (e.g., "C#", "Java", "TypeScript").
    /// </summary>
    string Language { get; }

    /// <summary>
    /// Generates source code from an AST node.
    /// </summary>
    /// <param name="node">The AST node to generate code for.</param>
    /// <returns>The generated source code.</returns>
    string Generate(IAstNode node);

    /// <summary>
    /// Generates source code from an AST node with specific options.
    /// </summary>
    /// <param name="node">The AST node to generate code for.</param>
    /// <param name="options">Code generation options.</param>
    /// <returns>The generated source code.</returns>
    string Generate(IAstNode node, CodeGenerationOptions options);
}

/// <summary>
/// Options for controlling code generation behavior.
/// </summary>
public sealed record CodeGenerationOptions
{
    /// <summary>
    /// Gets the indentation string to use (default: 4 spaces).
    /// </summary>
    public string Indentation { get; init; } = "    ";

    /// <summary>
    /// Gets whether to generate XML documentation comments.
    /// </summary>
    public bool GenerateDocumentation { get; init; } = true;

    /// <summary>
    /// Gets whether to generate attributes/annotations.
    /// </summary>
    public bool GenerateAttributes { get; init; } = true;

    /// <summary>
    /// Gets whether to use file-scoped namespaces (C# 10+).
    /// </summary>
    public bool UseFileScopedNamespaces { get; init; } = true;

    /// <summary>
    /// Gets whether to generate nullable reference type annotations.
    /// </summary>
    public bool GenerateNullableAnnotations { get; init; } = true;

    /// <summary>
    /// Gets the line ending style to use.
    /// </summary>
    public LineEndingStyle LineEndings { get; init; } = LineEndingStyle.Environment;

    /// <summary>
    /// Gets whether to sort using statements alphabetically.
    /// </summary>
    public bool SortUsings { get; init; } = true;

    /// <summary>
    /// Gets the maximum line length before wrapping (0 = no limit).
    /// </summary>
    public int MaxLineLength { get; init; } = 120;
}

/// <summary>
/// Represents line ending styles for generated code.
/// </summary>
public enum LineEndingStyle
{
    /// <summary>Use the current environment's line endings.</summary>
    Environment,
    /// <summary>Use Windows-style CRLF line endings.</summary>
    Windows,
    /// <summary>Use Unix-style LF line endings.</summary>
    Unix
}