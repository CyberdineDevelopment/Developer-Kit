using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.CodeBuilder.Abstractions;

/// <summary>
/// Interface for parsing source code into AST representations.
/// Provides language-agnostic parsing capabilities.
/// </summary>
public interface ICodeParser
{
    /// <summary>
    /// Gets the language this parser supports.
    /// </summary>
    string Language { get; }

    /// <summary>
    /// Gets the file extensions this parser can handle.
    /// </summary>
    IReadOnlyList<string> SupportedExtensions { get; }

    /// <summary>
    /// Parses source code into a syntax tree.
    /// </summary>
    /// <param name="source">The source code to parse.</param>
    /// <param name="filePath">Optional file path for error reporting.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the parsed syntax tree or parsing errors.</returns>
    Task<IFdwResult<ISyntaxTree>> ParseAsync(
        string source, 
        string? filePath = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses source code into a compilation unit definition.
    /// </summary>
    /// <param name="source">The source code to parse.</param>
    /// <param name="filePath">Optional file path for error reporting.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the parsed compilation unit or parsing errors.</returns>
    Task<IFdwResult<ICompilationUnitDefinition>> ParseToDefinitionAsync(
        string source, 
        string? filePath = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates source code syntax without full parsing.
    /// </summary>
    /// <param name="source">The source code to validate.</param>
    /// <param name="filePath">Optional file path for error reporting.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating whether the syntax is valid.</returns>
    Task<IFdwResult<bool>> ValidateSyntaxAsync(
        string source, 
        string? filePath = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets syntax errors from source code.
    /// </summary>
    /// <param name="source">The source code to check.</param>
    /// <param name="filePath">Optional file path for error reporting.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing any syntax errors found.</returns>
    Task<IFdwResult<IReadOnlyList<SyntaxError>>> GetSyntaxErrorsAsync(
        string source, 
        string? filePath = null, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an abstract syntax tree for parsed code.
/// </summary>
public interface ISyntaxTree
{
    /// <summary>
    /// Gets the root node of the syntax tree.
    /// </summary>
    IAstNode Root { get; }

    /// <summary>
    /// Gets the original source text.
    /// </summary>
    string SourceText { get; }

    /// <summary>
    /// Gets the file path, if available.
    /// </summary>
    string? FilePath { get; }

    /// <summary>
    /// Gets the language of this syntax tree.
    /// </summary>
    string Language { get; }

    /// <summary>
    /// Gets any syntax errors in the tree.
    /// </summary>
    IReadOnlyList<SyntaxError> Errors { get; }

    /// <summary>
    /// Gets whether the syntax tree has any errors.
    /// </summary>
    bool HasErrors { get; }

    /// <summary>
    /// Gets all nodes of a specific type in the tree.
    /// </summary>
    /// <typeparam name="T">The type of nodes to retrieve.</typeparam>
    /// <returns>An enumerable of nodes of the specified type.</returns>
    IEnumerable<T> GetNodes<T>() where T : class, IAstNode;

    /// <summary>
    /// Gets the node at a specific position in the source text.
    /// </summary>
    /// <param name="position">The position in the source text.</param>
    /// <returns>The node at the specified position, or null if not found.</returns>
    IAstNode? GetNodeAtPosition(int position);

    /// <summary>
    /// Gets the node at a specific line and column.
    /// </summary>
    /// <param name="line">The line number (1-based).</param>
    /// <param name="column">The column number (1-based).</param>
    /// <returns>The node at the specified location, or null if not found.</returns>
    IAstNode? GetNodeAtLocation(int line, int column);

    /// <summary>
    /// Converts this syntax tree to a code definition.
    /// </summary>
    /// <returns>The code definition representation of this syntax tree.</returns>
    ICompilationUnitDefinition ToDefinition();
}

/// <summary>
/// Represents a syntax error in parsed code.
/// </summary>
public sealed record SyntaxError
{
    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the error code, if available.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// Gets the severity of the error.
    /// </summary>
    public ErrorSeverity Severity { get; init; } = ErrorSeverity.Error;

    /// <summary>
    /// Gets the location of the error.
    /// </summary>
    public SourceLocation? Location { get; init; }

    /// <summary>
    /// Gets the file path where the error occurred.
    /// </summary>
    public string? FilePath { get; init; }
}

/// <summary>
/// Represents the severity of a syntax error.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>Hidden/suppressed error.</summary>
    Hidden,
    /// <summary>Informational message.</summary>
    Info,
    /// <summary>Warning message.</summary>
    Warning,
    /// <summary>Error message.</summary>
    Error
}