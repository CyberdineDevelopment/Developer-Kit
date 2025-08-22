using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.CSharp.Parsing;

namespace FractalDataWorks.CodeBuilder.CSharp;

/// <summary>
/// Default implementation of ILanguageRegistry.
/// </summary>
public sealed class LanguageRegistry : ILanguageRegistry
{
    private readonly Dictionary<string, ICodeParser> _parsers = new Dictionary<string, ICodeParser>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _extensionToLanguage = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<string>> _languageToExtensions = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageRegistry"/> class.
    /// </summary>
    public LanguageRegistry()
    {
        // Register default C# parser
        RegisterParser("csharp", new RoslynCSharpParser(), ".cs", ".csx");
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedLanguages
    {
        get
        {
            var languages = new List<string>(_parsers.Keys);
            languages.Sort(StringComparer.OrdinalIgnoreCase);
            return languages;
        }
    }

    /// <inheritdoc/>
    public bool IsSupported(string language)
    {
        if (string.IsNullOrEmpty(language))
        {
            return false;
        }

        return _parsers.ContainsKey(language);
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetExtensions(string language)
    {
        if (string.IsNullOrEmpty(language))
        {
            return Array.Empty<string>();
        }

        if (_languageToExtensions.TryGetValue(language, out var extensions))
        {
            return extensions.AsReadOnly();
        }

        return Array.Empty<string>();
    }

    /// <inheritdoc/>
    public string? GetLanguageByExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return null;
        }

        // Ensure extension starts with a dot
        if (!extension.StartsWith(".", StringComparison.Ordinal))
        {
            extension = "." + extension;
        }

        return _extensionToLanguage.TryGetValue(extension, out var language) ? language : null;
    }

    /// <inheritdoc/>
    public Task<ICodeParser?> GetParserAsync(string language, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(language))
        {
            return Task.FromResult<ICodeParser?>(null);
        }

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(_parsers.TryGetValue(language, out var parser) ? parser : null);
    }

    /// <inheritdoc/>
    public void RegisterParser(string language, ICodeParser parser, params string[] extensions)
    {
        if (string.IsNullOrEmpty(language))
        {
            throw new ArgumentException("Language cannot be null or empty", nameof(language));
        }

        var normalizedLanguage = language.ToLowerInvariant();
        _parsers[normalizedLanguage] = parser;

        if (extensions != null && extensions.Length > 0)
        {
            if (!_languageToExtensions.ContainsKey(normalizedLanguage))
            {
                _languageToExtensions[normalizedLanguage] = new List<string>();
            }

            foreach (var extension in extensions)
            {
                if (!string.IsNullOrEmpty(extension))
                {
                    var ext = extension.StartsWith(".", StringComparison.Ordinal) ? extension : "." + extension;
                    _extensionToLanguage[ext] = normalizedLanguage;
                    
                    if (!_languageToExtensions[normalizedLanguage].Contains(ext))
                    {
                        _languageToExtensions[normalizedLanguage].Add(ext);
                    }
                }
            }
        }
    }
}