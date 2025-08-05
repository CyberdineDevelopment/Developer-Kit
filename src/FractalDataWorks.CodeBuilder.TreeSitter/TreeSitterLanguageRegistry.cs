using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.CodeBuilder.TreeSitter;

/// <summary>
/// Registry for tree-sitter language grammars and parsers.
/// Manages language-specific parsing capabilities.
/// </summary>
public sealed class TreeSitterLanguageRegistry
{
    private readonly ILogger<TreeSitterLanguageRegistry> _logger;
    private readonly Dictionary<string, TreeSitterLanguageConfig> _languages;
    private readonly Dictionary<string, ITreeSitterLanguageParser> _parsers;

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeSitterLanguageRegistry"/> class.
    /// </summary>
    /// <param name="logger">Optional logger instance.</param>
    public TreeSitterLanguageRegistry(ILogger<TreeSitterLanguageRegistry>? logger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TreeSitterLanguageRegistry>.Instance;
        _languages = new Dictionary<string, TreeSitterLanguageConfig>(StringComparer.OrdinalIgnoreCase);
        _parsers = new Dictionary<string, ITreeSitterLanguageParser>(StringComparer.OrdinalIgnoreCase);

        // Register built-in language support
        RegisterBuiltInLanguages();
    }

    /// <summary>
    /// Gets all supported languages.
    /// </summary>
    public IReadOnlyList<string> SupportedLanguages => _languages.Keys.ToArray();

    /// <summary>
    /// Checks if a language is supported.
    /// </summary>
    /// <param name="language">The language to check.</param>
    /// <returns>True if the language is supported; otherwise, false.</returns>
    public bool IsSupported(string language)
    {
        return _languages.ContainsKey(language);
    }

    /// <summary>
    /// Gets the file extensions for a language.
    /// </summary>
    /// <param name="language">The language to get extensions for.</param>
    /// <returns>The file extensions for the language.</returns>
    public IReadOnlyList<string> GetExtensions(string language)
    {
        return _languages.TryGetValue(language, out var config) 
            ? config.Extensions 
            : Array.Empty<string>();
    }

    /// <summary>
    /// Gets or creates a parser for the specified language.
    /// </summary>
    /// <param name="language">The language to get a parser for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parser for the language, or null if not supported.</returns>
    public async Task<ITreeSitterLanguageParser?> GetParserAsync(
        string language, 
        CancellationToken cancellationToken = default)
    {
        if (!_languages.TryGetValue(language, out var config))
        {
            _logger.LogWarning("Language {Language} is not supported", language);
            return null;
        }

        if (_parsers.TryGetValue(language, out var existingParser))
        {
            return existingParser;
        }

        try
        {
            _logger.LogDebug("Creating parser for language {Language}", language);
            var parser = await CreateParserAsync(config, cancellationToken).ConfigureAwait(false);
            
            if (parser != null)
            {
                _parsers[language] = parser;
                _logger.LogDebug("Successfully created parser for language {Language}", language);
            }
            
            return parser;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Failed to create parser for language {Language}", language);
            return null;
        }
    }

    /// <summary>
    /// Registers a new language with the registry.
    /// </summary>
    /// <param name="language">The language name.</param>
    /// <param name="config">The language configuration.</param>
    public void RegisterLanguage(string language, TreeSitterLanguageConfig config)
    {
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(config);

        _languages[language] = config;
        _logger.LogInformation("Registered language {Language} with extensions: {Extensions}", 
            language, string.Join(", ", config.Extensions));
    }

    /// <summary>
    /// Unregisters a language from the registry.
    /// </summary>
    /// <param name="language">The language to unregister.</param>
    public void UnregisterLanguage(string language)
    {
        if (_languages.Remove(language))
        {
            if (_parsers.Remove(language, out var parser))
            {
                parser.Dispose();
            }
            _logger.LogInformation("Unregistered language {Language}", language);
        }
    }

    private void RegisterBuiltInLanguages()
    {
        // C# language support
        RegisterLanguage("csharp", new TreeSitterLanguageConfig
        {
            Name = "C#",
            Extensions = new[] { ".cs" },
            GrammarName = "tree-sitter-c-sharp",
            ParserFactory = () => new CSharpTreeSitterParser()
        });

        // TypeScript language support
        RegisterLanguage("typescript", new TreeSitterLanguageConfig
        {
            Name = "TypeScript",
            Extensions = new[] { ".ts", ".tsx" },
            GrammarName = "tree-sitter-typescript",
            ParserFactory = () => new TypeScriptTreeSitterParser()
        });

        // JavaScript language support
        RegisterLanguage("javascript", new TreeSitterLanguageConfig
        {
            Name = "JavaScript",
            Extensions = new[] { ".js", ".jsx" },
            GrammarName = "tree-sitter-javascript",
            ParserFactory = () => new JavaScriptTreeSitterParser()
        });

        // Python language support
        RegisterLanguage("python", new TreeSitterLanguageConfig
        {
            Name = "Python",
            Extensions = new[] { ".py", ".pyi" },
            GrammarName = "tree-sitter-python",
            ParserFactory = () => new PythonTreeSitterParser()
        });

        // JSON language support
        RegisterLanguage("json", new TreeSitterLanguageConfig
        {
            Name = "JSON",
            Extensions = new[] { ".json" },
            GrammarName = "tree-sitter-json",
            ParserFactory = () => new JsonTreeSitterParser()
        });

        _logger.LogInformation("Registered {Count} built-in languages", _languages.Count);
    }

    private async Task<ITreeSitterLanguageParser?> CreateParserAsync(
        TreeSitterLanguageConfig config, 
        CancellationToken cancellationToken)
    {
        try
        {
            var parser = config.ParserFactory();
            await parser.InitializeAsync(cancellationToken).ConfigureAwait(false);
            return parser;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Failed to create parser for language {Language}", config.Name);
            return null;
        }
    }
}

/// <summary>
/// Configuration for a tree-sitter language.
/// </summary>
public sealed record TreeSitterLanguageConfig
{
    /// <summary>
    /// Gets or sets the human-readable name of the language.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the file extensions for this language.
    /// </summary>
    public IReadOnlyList<string> Extensions { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the tree-sitter grammar name.
    /// </summary>
    public string GrammarName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the factory function for creating parsers.
    /// </summary>
    public Func<ITreeSitterLanguageParser> ParserFactory { get; init; } = null!;
}

/// <summary>
/// Interface for language-specific tree-sitter parsers.
/// </summary>
public interface ITreeSitterLanguageParser : IDisposable
{
    /// <summary>
    /// Gets the language this parser supports.
    /// </summary>
    string Language { get; }

    /// <summary>
    /// Gets whether this parser is initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Initializes the parser with the tree-sitter grammar.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the initialization operation.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses source code and returns the AST root node.
    /// </summary>
    /// <param name="source">The source code to parse.</param>
    /// <param name="filePath">Optional file path for error reporting.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task containing the parse result.</returns>
    Task<FractalDataWorks.Results.IFdwResult<TreeSitterAstNode>> ParseAsync(
        string source, 
        string? filePath = null, 
        CancellationToken cancellationToken = default);
}