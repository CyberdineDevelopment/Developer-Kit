using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.Results;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.CodeBuilder.TreeSitter;

/// <summary>
/// Tree-sitter based code parser for multi-language support.
/// Provides language-agnostic parsing using tree-sitter grammars.
/// </summary>
public sealed class TreeSitterCodeParser : ICodeParser
{
    private readonly ILogger<TreeSitterCodeParser> _logger;
    private readonly TreeSitterLanguageRegistry _languageRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeSitterCodeParser"/> class.
    /// </summary>
    /// <param name="language">The primary language this parser supports.</param>
    /// <param name="languageRegistry">The language registry for tree-sitter grammars.</param>
    /// <param name="logger">Optional logger instance.</param>
    public TreeSitterCodeParser(
        string language,
        TreeSitterLanguageRegistry languageRegistry,
        ILogger<TreeSitterCodeParser>? logger = null)
    {
        Language = language ?? throw new ArgumentNullException(nameof(language));
        _languageRegistry = languageRegistry ?? throw new ArgumentNullException(nameof(languageRegistry));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TreeSitterCodeParser>.Instance;

        if (!_languageRegistry.IsSupported(language))
        {
            throw new ArgumentException($"Language '{language}' is not supported by the language registry.", nameof(language));
        }
    }

    /// <inheritdoc/>
    public string Language { get; }

    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedExtensions => _languageRegistry.GetExtensions(Language);

    /// <inheritdoc/>
    public async Task<IFdwResult<ISyntaxTree>> ParseAsync(
        string source, 
        string? filePath = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(source))
        {
            return FdwResult<ISyntaxTree>.Failure(new TreeSitterParsingError("Source code cannot be null or empty"));
        }

        try
        {
            _logger.LogDebug("Parsing source code for language {Language}", Language);

            var parser = await _languageRegistry.GetParserAsync(Language, cancellationToken).ConfigureAwait(false);
            if (parser == null)
            {
                return FdwResult<ISyntaxTree>.Failure(new TreeSitterParsingError($"No parser available for language '{Language}'"));
            }

            var parseResult = await parser.ParseAsync(source, filePath, cancellationToken).ConfigureAwait(false);
            if (parseResult.IsFailure)
            {
                return FdwResult<ISyntaxTree>.Failure(parseResult.Message!);
            }

            var syntaxTree = new TreeSitterSyntaxTree(
                parseResult.Value!,
                source,
                filePath,
                Language);

            _logger.LogDebug("Successfully parsed source code for language {Language}", Language);
            return FdwResult<ISyntaxTree>.Success(syntaxTree);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error parsing source code for language {Language}", Language);
            return FdwResult<ISyntaxTree>.Failure(new TreeSitterParsingError($"Parsing failed: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<ICompilationUnitDefinition>> ParseToDefinitionAsync(
        string source, 
        string? filePath = null, 
        CancellationToken cancellationToken = default)
    {
        var parseResult = await ParseAsync(source, filePath, cancellationToken).ConfigureAwait(false);
        if (parseResult.IsFailure)
        {
            return FdwResult<ICompilationUnitDefinition>.Failure(parseResult.Message!);
        }

        try
        {
            var definition = parseResult.Value!.ToDefinition();
            return FdwResult<ICompilationUnitDefinition>.Success(definition);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error converting syntax tree to definition for language {Language}", Language);
            return FdwResult<ICompilationUnitDefinition>.Failure(new TreeSitterParsingError($"Definition conversion failed: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<bool>> ValidateSyntaxAsync(
        string source, 
        string? filePath = null, 
        CancellationToken cancellationToken = default)
    {
        var parseResult = await ParseAsync(source, filePath, cancellationToken).ConfigureAwait(false);
        if (parseResult.IsFailure)
        {
            return FdwResult<bool>.Success(false);
        }

        var hasErrors = parseResult.Value!.HasErrors;
        return FdwResult<bool>.Success(!hasErrors);
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<IReadOnlyList<SyntaxError>>> GetSyntaxErrorsAsync(
        string source, 
        string? filePath = null, 
        CancellationToken cancellationToken = default)
    {
        var parseResult = await ParseAsync(source, filePath, cancellationToken).ConfigureAwait(false);
        if (parseResult.IsFailure)
        {
            return FdwResult<IReadOnlyList<SyntaxError>>.Success(Array.Empty<SyntaxError>());
        }

        return FdwResult<IReadOnlyList<SyntaxError>>.Success(parseResult.Value!.Errors);
    }
}

/// <summary>
/// Error message type for tree-sitter parsing errors.
/// </summary>
public sealed record TreeSitterParsingError : IFdwMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TreeSitterParsingError"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TreeSitterParsingError(string message)
    {
        Message = message;
    }

    /// <inheritdoc/>
    public string Message { get; }

    /// <inheritdoc/>
    public string Format(params object[] args) => string.Format(Message, args);
}