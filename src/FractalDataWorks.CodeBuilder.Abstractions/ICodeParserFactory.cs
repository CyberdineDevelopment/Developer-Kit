using System;
using System.Collections.Generic;

namespace FractalDataWorks.CodeBuilder.Abstractions;

/// <summary>
/// Factory interface for creating code parsers and builders for different languages.
/// Provides a centralized way to access language-specific implementations.
/// </summary>
public interface ICodeParserFactory
{
    /// <summary>
    /// Gets all supported languages.
    /// </summary>
    IReadOnlyList<string> SupportedLanguages { get; }

    /// <summary>
    /// Creates a code parser for the specified language.
    /// </summary>
    /// <param name="language">The programming language.</param>
    /// <returns>A code parser for the language, or null if not supported.</returns>
    ICodeParser? CreateParser(string language);

    /// <summary>
    /// Creates a code builder for the specified language.
    /// </summary>
    /// <param name="language">The programming language.</param>
    /// <returns>A code builder for the language, or null if not supported.</returns>
    ICodeBuilder? CreateCodeBuilder(string language);

    /// <summary>
    /// Checks if a language is supported.
    /// </summary>
    /// <param name="language">The programming language to check.</param>
    /// <returns>True if the language is supported; otherwise, false.</returns>
    bool IsLanguageSupported(string language);

    /// <summary>
    /// Gets the file extensions associated with a language.
    /// </summary>
    /// <param name="language">The programming language.</param>
    /// <returns>The file extensions for the language.</returns>
    IReadOnlyList<string> GetLanguageExtensions(string language);

    /// <summary>
    /// Detects the language from a file extension.
    /// </summary>
    /// <param name="extension">The file extension (with or without leading dot).</param>
    /// <returns>The detected language, or null if not recognized.</returns>
    string? DetectLanguageFromExtension(string extension);

    /// <summary>
    /// Registers a new language parser.
    /// </summary>
    /// <param name="language">The programming language.</param>
    /// <param name="parserFactory">Factory function to create the parser.</param>
    /// <param name="builderFactory">Factory function to create the code builder.</param>
    /// <param name="extensions">File extensions for the language.</param>
    void RegisterLanguage(
        string language,
        Func<ICodeParser> parserFactory,
        Func<ICodeBuilder> builderFactory,
        IReadOnlyList<string> extensions);

    /// <summary>
    /// Unregisters a language parser.
    /// </summary>
    /// <param name="language">The programming language to unregister.</param>
    /// <returns>True if the language was unregistered; otherwise, false.</returns>
    bool UnregisterLanguage(string language);

    /// <summary>
    /// Gets information about a supported language.
    /// </summary>
    /// <param name="language">The programming language.</param>
    /// <returns>Language information if supported; otherwise, null.</returns>
    LanguageInfo? GetLanguageInfo(string language);

    /// <summary>
    /// Gets all registered language information.
    /// </summary>
    /// <returns>A collection of language information.</returns>
    IReadOnlyList<LanguageInfo> GetAllLanguageInfo();
}

/// <summary>
/// Represents information about a supported programming language.
/// </summary>
public sealed record LanguageInfo
{
    /// <summary>Gets the language name.</summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>Gets the display name for the language.</summary>
    public string DisplayName { get; init; } = string.Empty;
    
    /// <summary>Gets the file extensions for this language.</summary>
    public IReadOnlyList<string> Extensions { get; init; } = Array.Empty<string>();
    
    /// <summary>Gets whether this language supports semantic analysis.</summary>
    public bool SupportsSemanticAnalysis { get; init; }
    
    /// <summary>Gets whether this language supports code completion.</summary>
    public bool SupportsCodeCompletion { get; init; }
    
    /// <summary>Gets whether this language supports transformations.</summary>
    public bool SupportsTransformations { get; init; }
    
    /// <summary>Gets the parser implementation type.</summary>
    public string ParserType { get; init; } = string.Empty;
    
    /// <summary>Gets additional metadata about the language.</summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = 
        new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Configuration for registering a language with the parser factory.
/// </summary>
public sealed record LanguageRegistration
{
    /// <summary>Gets or sets the language name.</summary>
    public string Language { get; init; } = string.Empty;
    
    /// <summary>Gets or sets the display name.</summary>
    public string DisplayName { get; init; } = string.Empty;
    
    /// <summary>Gets or sets the file extensions.</summary>
    public IReadOnlyList<string> Extensions { get; init; } = Array.Empty<string>();
    
    /// <summary>Gets or sets the parser factory.</summary>
    public Func<ICodeParser> ParserFactory { get; init; } = null!;
    
    /// <summary>Gets or sets the code builder factory.</summary>
    public Func<ICodeBuilder> BuilderFactory { get; init; } = null!;
    
    /// <summary>Gets or sets whether semantic analysis is supported.</summary>
    public bool SupportsSemanticAnalysis { get; init; }
    
    /// <summary>Gets or sets whether code completion is supported.</summary>
    public bool SupportsCodeCompletion { get; init; }
    
    /// <summary>Gets or sets whether transformations are supported.</summary>
    public bool SupportsTransformations { get; init; }
    
    /// <summary>Gets or sets the parser implementation type name.</summary>
    public string ParserType { get; init; } = string.Empty;
    
    /// <summary>Gets or sets additional metadata.</summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = 
        new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Exception thrown when a language is not supported by the parser factory.
/// </summary>
public sealed class LanguageNotSupportedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageNotSupportedException"/> class.
    /// </summary>
    /// <param name="language">The unsupported language.</param>
    public LanguageNotSupportedException(string language)
        : base($"Language '{language}' is not supported")
    {
        Language = language;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageNotSupportedException"/> class.
    /// </summary>
    /// <param name="language">The unsupported language.</param>
    /// <param name="message">The exception message.</param>
    public LanguageNotSupportedException(string language, string message)
        : base(message)
    {
        Language = language;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageNotSupportedException"/> class.
    /// </summary>
    /// <param name="language">The unsupported language.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LanguageNotSupportedException(string language, string message, Exception innerException)
        : base(message, innerException)
    {
        Language = language;
    }

    /// <summary>
    /// Gets the unsupported language.
    /// </summary>
    public string Language { get; }
}