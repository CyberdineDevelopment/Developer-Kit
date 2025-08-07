using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.Parsing;

namespace FractalDataWorks.CodeBuilder;

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

        return _extensionToLanguage.GetValueOrDefault(extension);
    }

    /// <inheritdoc/>
    public Task<ICodeParser?> GetParserAsync(string language, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(language))
        {
            return Task.FromResult<ICodeParser?>(null);
        }

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(_parsers.GetValueOrDefault(language));
    }

    /// <inheritdoc/>
    public void RegisterParser(string language, ICodeParser parser, params string[] extensions)
    {
        if (string.IsNullOrEmpty(language))
        {
            throw new ArgumentException("Language cannot be null or empty", nameof(language));
        }

        _parsers[language] = parser;

        if (extensions != null && extensions.Length > 0)
        {
            if (!_languageToExtensions.ContainsKey(language))
            {
                _languageToExtensions[language] = new List<string>();
            }

            foreach (var extension in extensions)
            {
                if (!string.IsNullOrEmpty(extension))
                {
                    var ext = extension.StartsWith(".", StringComparison.Ordinal) ? extension : "." + extension;
                    _extensionToLanguage[ext] = language;
                    
                    if (!_languageToExtensions[language].Contains(ext))
                    {
                        _languageToExtensions[language].Add(ext);
                    }
                }
            }
        }
    }
}