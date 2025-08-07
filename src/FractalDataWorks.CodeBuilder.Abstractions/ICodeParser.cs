using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;

namespace FractalDataWorks.CodeBuilder.Abstractions;

/// <summary>
/// Defines a parser that can convert source code into a syntax tree.
/// </summary>
public interface ICodeParser
{
    /// <summary>
    /// Gets the language this parser supports.
    /// </summary>
    string Language { get; }

    /// <summary>
    /// Parses the source code into a syntax tree.
    /// </summary>
    /// <param name="sourceCode">The source code to parse.</param>
    /// <param name="filePath">Optional file path for context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the syntax tree or error information.</returns>
    Task<IFdwResult<ISyntaxTree>> ParseAsync(
        string sourceCode,
        string? filePath = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the source code without building a full syntax tree.
    /// </summary>
    /// <param name="sourceCode">The source code to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating whether the code is valid.</returns>
    Task<IFdwResult> ValidateAsync(
        string sourceCode,
        CancellationToken cancellationToken = default);
}