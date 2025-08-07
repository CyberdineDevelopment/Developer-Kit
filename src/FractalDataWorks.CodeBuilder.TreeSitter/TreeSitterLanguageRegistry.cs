using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.CodeBuilder.Abstractions;

namespace FractalDataWorks.CodeBuilder.TreeSitter;

/// <summary>
/// Registry for managing TreeSitter language parsers.
/// </summary>
public sealed class TreeSitterLanguageRegistry : ILanguageRegistry
{
    private readonly Dictionary<string, ICodeParser> _parsers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<string>> _languageExtensions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _extensionToLanguage = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeSitterLanguageRegistry"/> class.
    /// </summary>
    public TreeSitterLanguageRegistry()
    {
        // Register default languages and extensions
        RegisterDefaultLanguages();
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedLanguages => _parsers.Keys.ToList();

    /// <inheritdoc/>
    public bool IsSupported(string language)
    {
        return _parsers.ContainsKey(language);
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetExtensions(string language)
    {
        return _languageExtensions.TryGetValue(language, out var extensions) 
            ? extensions 
            : Array.Empty<string>();
    }

    /// <inheritdoc/>
    public string? GetLanguageByExtension(string extension)
    {
        var ext = extension.StartsWith(".") ? extension : $".{extension}";
        return _extensionToLanguage.TryGetValue(ext, out var language) ? language : null;
    }

    /// <inheritdoc/>
    public Task<ICodeParser?> GetParserAsync(string language, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_parsers.TryGetValue(language, out var parser) ? parser : null);
    }

    /// <inheritdoc/>
    public void RegisterParser(string language, ICodeParser parser, params string[] extensions)
    {
        _parsers[language] = parser;
        
        if (!_languageExtensions.ContainsKey(language))
            _languageExtensions[language] = new List<string>();

        foreach (var extension in extensions)
        {
            var ext = extension.StartsWith(".") ? extension : $".{extension}";
            _languageExtensions[language].Add(ext);
            _extensionToLanguage[ext] = language;
        }
    }

    private void RegisterDefaultLanguages()
    {
        // Register C# parser
        var csharpParser = new TreeSitterCSharpParser();
        RegisterParser("csharp", csharpParser, ".cs", ".csx");
        RegisterParser("c#", csharpParser, ".cs", ".csx");

        // Additional languages can be registered here
        // RegisterParser("javascript", new TreeSitterJavaScriptParser(), ".js", ".mjs");
        // RegisterParser("typescript", new TreeSitterTypeScriptParser(), ".ts", ".tsx");
        // RegisterParser("python", new TreeSitterPythonParser(), ".py");
    }
}