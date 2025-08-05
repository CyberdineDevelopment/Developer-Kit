using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.Roslyn;
using FractalDataWorks.CodeBuilder.TreeSitter;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.CodeBuilder;

/// <summary>
/// Default implementation of the code parser factory.
/// Provides language-specific parsers and builders with built-in support for common languages.
/// </summary>
public sealed class CodeParserFactory : ICodeParserFactory
{
    private readonly ILogger<CodeParserFactory> _logger;
    private readonly ConcurrentDictionary<string, LanguageRegistration> _languages;
    private readonly ConcurrentDictionary<string, string> _extensionToLanguage;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeParserFactory"/> class.
    /// </summary>
    /// <param name="logger">Optional logger instance.</param>
    public CodeParserFactory(ILogger<CodeParserFactory>? logger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<CodeParserFactory>.Instance;
        _languages = new ConcurrentDictionary<string, LanguageRegistration>(StringComparer.OrdinalIgnoreCase);
        _extensionToLanguage = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        RegisterBuiltInLanguages();
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedLanguages => _languages.Keys.ToArray();

    /// <inheritdoc/>
    public ICodeParser? CreateParser(string language)
    {
        if (string.IsNullOrEmpty(language))
        {
            return null;
        }

        if (_languages.TryGetValue(language, out var registration))
        {
            try
            {
                _logger.LogDebug("Creating parser for language {Language}", language);
                var parser = registration.ParserFactory();
                _logger.LogDebug("Successfully created parser for language {Language}", language);
                return parser;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                _logger.LogError(ex, "Error creating parser for language {Language}", language);
                return null;
            }
        }

        _logger.LogWarning("No parser factory registered for language {Language}", language);
        return null;
    }

    /// <inheritdoc/>
    public ICodeBuilder? CreateCodeBuilder(string language)
    {
        if (string.IsNullOrEmpty(language))
        {
            return null;
        }

        if (_languages.TryGetValue(language, out var registration))
        {
            try
            {
                _logger.LogDebug("Creating code builder for language {Language}", language);
                var builder = registration.BuilderFactory();
                _logger.LogDebug("Successfully created code builder for language {Language}", language);
                return builder;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                _logger.LogError(ex, "Error creating code builder for language {Language}", language);
                return null;
            }
        }

        _logger.LogWarning("No code builder factory registered for language {Language}", language);
        return null;
    }

    /// <inheritdoc/>
    public bool IsLanguageSupported(string language)
    {
        return !string.IsNullOrEmpty(language) && _languages.ContainsKey(language);
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetLanguageExtensions(string language)
    {
        if (_languages.TryGetValue(language, out var registration))
        {
            return registration.Extensions;
        }

        return Array.Empty<string>();
    }

    /// <inheritdoc/>
    public string? DetectLanguageFromExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return null;
        }

        // Normalize extension (ensure it starts with a dot)
        var normalizedExtension = extension.StartsWith('.') ? extension : $".{extension}";

        return _extensionToLanguage.TryGetValue(normalizedExtension, out var language) ? language : null;
    }

    /// <inheritdoc/>
    public void RegisterLanguage(
        string language,
        Func<ICodeParser> parserFactory,
        Func<ICodeBuilder> builderFactory,
        IReadOnlyList<string> extensions)
    {
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(parserFactory);
        ArgumentNullException.ThrowIfNull(builderFactory);
        ArgumentNullException.ThrowIfNull(extensions);

        var registration = new LanguageRegistration
        {
            Language = language,
            DisplayName = language,
            Extensions = extensions.Select(NormalizeExtension).ToArray(),
            ParserFactory = parserFactory,
            BuilderFactory = builderFactory,
            SupportsSemanticAnalysis = false,
            SupportsCodeCompletion = false,
            SupportsTransformations = false,
            ParserType = "Generic"
        };

        RegisterLanguageInternal(registration);
    }

    /// <inheritdoc/>
    public bool UnregisterLanguage(string language)
    {
        if (string.IsNullOrEmpty(language))
        {
            return false;
        }

        if (_languages.TryRemove(language, out var registration))
        {
            // Remove extension mappings
            foreach (var extension in registration.Extensions)
            {
                _extensionToLanguage.TryRemove(extension, out _);
            }

            _logger.LogInformation("Unregistered language {Language}", language);
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public LanguageInfo? GetLanguageInfo(string language)
    {
        if (_languages.TryGetValue(language, out var registration))
        {
            return new LanguageInfo
            {
                Name = registration.Language,
                DisplayName = registration.DisplayName,
                Extensions = registration.Extensions,
                SupportsSemanticAnalysis = registration.SupportsSemanticAnalysis,
                SupportsCodeCompletion = registration.SupportsCodeCompletion,
                SupportsTransformations = registration.SupportsTransformations,
                ParserType = registration.ParserType,
                Metadata = registration.Metadata
            };
        }

        return null;
    }

    /// <inheritdoc/>
    public IReadOnlyList<LanguageInfo> GetAllLanguageInfo()
    {
        return _languages.Values.Select(reg => new LanguageInfo
        {
            Name = reg.Language,
            DisplayName = reg.DisplayName,
            Extensions = reg.Extensions,
            SupportsSemanticAnalysis = reg.SupportsSemanticAnalysis,
            SupportsCodeCompletion = reg.SupportsCodeCompletion,
            SupportsTransformations = reg.SupportsTransformations,
            ParserType = reg.ParserType,
            Metadata = reg.Metadata
        }).ToArray();
    }

    private void RegisterBuiltInLanguages()
    {
        // Register C# with Roslyn
        RegisterLanguageInternal(new LanguageRegistration
        {
            Language = "csharp",
            DisplayName = "C#",
            Extensions = new[] { ".cs" },
            ParserFactory = () => new RoslynCodeParser(),
            BuilderFactory = () => new CSharpCodeBuilder(),
            SupportsSemanticAnalysis = true,
            SupportsCodeCompletion = true,
            SupportsTransformations = true,
            ParserType = "Roslyn",
            Metadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["SupportsGenerics"] = true,
                ["SupportsNullableReferenceTypes"] = true,
                ["SupportsRecords"] = true
            }
        });

        // Register TypeScript with Tree-sitter
        RegisterLanguageInternal(new LanguageRegistration
        {
            Language = "typescript",
            DisplayName = "TypeScript",
            Extensions = new[] { ".ts", ".tsx" },
            ParserFactory = () => CreateTreeSitterParser("typescript"),
            BuilderFactory = () => new TypeScriptCodeBuilder(),
            SupportsSemanticAnalysis = false,
            SupportsCodeCompletion = false,
            SupportsTransformations = false,
            ParserType = "TreeSitter"
        });

        // Register JavaScript with Tree-sitter
        RegisterLanguageInternal(new LanguageRegistration
        {
            Language = "javascript",
            DisplayName = "JavaScript",
            Extensions = new[] { ".js", ".jsx" },
            ParserFactory = () => CreateTreeSitterParser("javascript"),
            BuilderFactory = () => new JavaScriptCodeBuilder(),
            SupportsSemanticAnalysis = false,
            SupportsCodeCompletion = false,
            SupportsTransformations = false,
            ParserType = "TreeSitter"
        });

        // Register Python with Tree-sitter
        RegisterLanguageInternal(new LanguageRegistration
        {
            Language = "python",
            DisplayName = "Python",
            Extensions = new[] { ".py", ".pyi" },
            ParserFactory = () => CreateTreeSitterParser("python"),
            BuilderFactory = () => new PythonCodeBuilder(),
            SupportsSemanticAnalysis = false,
            SupportsCodeCompletion = false,
            SupportsTransformations = false,
            ParserType = "TreeSitter"
        });

        // Register JSON with Tree-sitter
        RegisterLanguageInternal(new LanguageRegistration
        {
            Language = "json",
            DisplayName = "JSON",
            Extensions = new[] { ".json" },
            ParserFactory = () => CreateTreeSitterParser("json"),
            BuilderFactory = () => new JsonCodeBuilder(),
            SupportsSemanticAnalysis = false,
            SupportsCodeCompletion = false,
            SupportsTransformations = false,
            ParserType = "TreeSitter"
        });

        _logger.LogInformation("Registered {LanguageCount} built-in languages", _languages.Count);
    }

    private void RegisterLanguageInternal(LanguageRegistration registration)
    {
        _languages[registration.Language] = registration;

        // Register extension mappings
        foreach (var extension in registration.Extensions)
        {
            _extensionToLanguage[extension] = registration.Language;
        }

        _logger.LogDebug("Registered language {Language} with {ExtensionCount} extensions", 
            registration.Language, registration.Extensions.Count);
    }

    private static string NormalizeExtension(string extension)
    {
        return extension.StartsWith('.') ? extension : $".{extension}";
    }

    private static ICodeParser CreateTreeSitterParser(string language)
    {
        var registry = new TreeSitterLanguageRegistry();
        return new TreeSitterCodeParser(language, registry);
    }
}

// Basic code builder implementations for non-C# languages
internal sealed class CSharpCodeBuilder : CodeBuilderBase
{
    public CSharpCodeBuilder() : base("csharp") { }
}

internal sealed class TypeScriptCodeBuilder : CodeBuilderBase
{
    public TypeScriptCodeBuilder() : base("typescript") { }
}

internal sealed class JavaScriptCodeBuilder : CodeBuilderBase
{
    public JavaScriptCodeBuilder() : base("javascript") { }
}

internal sealed class PythonCodeBuilder : CodeBuilderBase
{
    public PythonCodeBuilder() : base("python") { }
}

internal sealed class JsonCodeBuilder : CodeBuilderBase
{
    public JsonCodeBuilder() : base("json") { }
}

/// <summary>
/// Base implementation of ICodeBuilder for common functionality.
/// </summary>
internal abstract class CodeBuilderBase : ICodeBuilder
{
    protected CodeBuilderBase(string language)
    {
        Language = language;
    }

    public string Language { get; }

    public virtual IClassBuilder CreateClass(string name) => new GenericClassBuilder(name);
    public virtual IInterfaceBuilder CreateInterface(string name) => new GenericInterfaceBuilder(name);
    public virtual IMethodBuilder CreateMethod(string name) => new GenericMethodBuilder(name);
    public virtual IPropertyBuilder CreateProperty(string name, string type) => new GenericPropertyBuilder(name, type);
    public virtual IFieldBuilder CreateField(string name, string type) => new GenericFieldBuilder(name, type);
    public virtual IParameterBuilder CreateParameter(string name, string type) => new GenericParameterBuilder(name, type);
    public virtual INamespaceBuilder CreateNamespace(string name) => new GenericNamespaceBuilder(name);
    public virtual ICompilationUnitBuilder CreateCompilationUnit() => new GenericCompilationUnitBuilder();
}

// Generic builder implementations (simplified for brevity)
internal sealed class GenericClassBuilder : IClassBuilder
{
    private readonly string _name;
    private AccessModifier _access = AccessModifier.None;
    private readonly List<IMethodDefinition> _methods = new();
    private readonly List<IPropertyDefinition> _properties = new();
    private readonly List<IFieldDefinition> _fields = new();

    public GenericClassBuilder(string name) => _name = name;

    public IClassBuilder WithAccess(AccessModifier access) { _access = access; return this; }
    public IClassBuilder WithBaseClass(string baseClass) => this;
    public IClassBuilder ImplementsInterface(string interfaceName) => this;
    public IClassBuilder AsAbstract() => this;
    public IClassBuilder AsSealed() => this;
    public IClassBuilder AsStatic() => this;
    public IClassBuilder AsPartial() => this;
    public IClassBuilder AddGenericParameter(string name, params string[] constraints) => this;

    public IClassBuilder AddMethod(Action<IMethodBuilder> configure)
    {
        var builder = new GenericMethodBuilder("Method");
        configure(builder);
        _methods.Add(builder.Build());
        return this;
    }

    public IClassBuilder AddProperty(Action<IPropertyBuilder> configure)
    {
        var builder = new GenericPropertyBuilder("Property", "object");
        configure(builder);
        _properties.Add(builder.Build());
        return this;
    }

    public IClassBuilder AddField(Action<IFieldBuilder> configure)
    {
        var builder = new GenericFieldBuilder("Field", "object");
        configure(builder);
        _fields.Add(builder.Build());
        return this;
    }

    public IClassBuilder AddAttribute(string name, params object[] arguments) => this;
    public IClassBuilder WithDocumentation(string documentation) => this;

    public IClassDefinition Build() => new ClassDefinition
    {
        Name = _name,
        Access = _access,
        Methods = _methods.Cast<MethodDefinition>().ToArray(),
        Properties = _properties.Cast<PropertyDefinition>().ToArray(),
        Fields = _fields.Cast<FieldDefinition>().ToArray()
    };
}

// Similar generic implementations for other builders...
internal sealed class GenericInterfaceBuilder : IInterfaceBuilder
{
    private readonly string _name;
    private AccessModifier _access = AccessModifier.None;

    public GenericInterfaceBuilder(string name) => _name = name;

    public IInterfaceBuilder WithAccess(AccessModifier access) { _access = access; return this; }
    public IInterfaceBuilder ExtendsInterface(string interfaceName) => this;
    public IInterfaceBuilder AddGenericParameter(string name, params string[] constraints) => this;
    public IInterfaceBuilder AddMethod(Action<IMethodBuilder> configure) => this;
    public IInterfaceBuilder AddProperty(Action<IPropertyBuilder> configure) => this;
    public IInterfaceBuilder AddField(Action<IFieldBuilder> configure) => this;
    public IInterfaceBuilder AddAttribute(string name, params object[] arguments) => this;
    public IInterfaceBuilder WithDocumentation(string documentation) => this;

    public IInterfaceDefinition Build() => new InterfaceDefinition { Name = _name, Access = _access };
}

internal sealed class GenericMethodBuilder : IMethodBuilder
{
    private readonly string _name;
    private AccessModifier _access = AccessModifier.None;
    private string _returnType = "void";

    public GenericMethodBuilder(string name) => _name = name;

    public IMethodBuilder WithAccess(AccessModifier access) { _access = access; return this; }
    public IMethodBuilder WithReturnType(string returnType) { _returnType = returnType; return this; }
    public IMethodBuilder AddParameter(Action<IParameterBuilder> configure) => this;
    public IMethodBuilder WithBody(string body) => this;
    public IMethodBuilder AsAbstract() => this;
    public IMethodBuilder AsVirtual() => this;
    public IMethodBuilder AsOverride() => this;
    public IMethodBuilder AsStatic() => this;
    public IMethodBuilder AddAttribute(string name, params object[] arguments) => this;
    public IMethodBuilder WithDocumentation(string documentation) => this;

    public IMethodDefinition Build() => new MethodDefinition { Name = _name, Access = _access, ReturnType = _returnType };
}

internal sealed class GenericPropertyBuilder : IPropertyBuilder
{
    private readonly string _name;
    private readonly string _type;
    private AccessModifier _access = AccessModifier.None;

    public GenericPropertyBuilder(string name, string type) { _name = name; _type = type; }

    public IPropertyBuilder WithAccess(AccessModifier access) { _access = access; return this; }
    public IPropertyBuilder WithType(string type) => this;
    public IPropertyBuilder WithGetter(string? body = null, AccessModifier? access = null) => this;
    public IPropertyBuilder WithSetter(string? body = null, AccessModifier? access = null) => this;
    public IPropertyBuilder AsStatic() => this;
    public IPropertyBuilder AsVirtual() => this;
    public IPropertyBuilder AsOverride() => this;
    public IPropertyBuilder AddAttribute(string name, params object[] arguments) => this;
    public IPropertyBuilder WithDocumentation(string documentation) => this;

    public IPropertyDefinition Build() => new PropertyDefinition { Name = _name, Access = _access, Type = _type };
}

internal sealed class GenericFieldBuilder : IFieldBuilder
{
    private readonly string _name;
    private readonly string _type;
    private AccessModifier _access = AccessModifier.None;

    public GenericFieldBuilder(string name, string type) { _name = name; _type = type; }

    public IFieldBuilder WithAccess(AccessModifier access) { _access = access; return this; }
    public IFieldBuilder WithType(string type) => this;
    public IFieldBuilder WithInitialValue(string value) => this;
    public IFieldBuilder AsStatic() => this;
    public IFieldBuilder AsReadOnly() => this;
    public IFieldBuilder AsConst() => this;
    public IFieldBuilder AddAttribute(string name, params object[] arguments) => this;
    public IFieldBuilder WithDocumentation(string documentation) => this;

    public IFieldDefinition Build() => new FieldDefinition { Name = _name, Access = _access, Type = _type };
}

internal sealed class GenericParameterBuilder : IParameterBuilder
{
    private readonly string _name;
    private readonly string _type;

    public GenericParameterBuilder(string name, string type) { _name = name; _type = type; }

    public IParameterBuilder WithType(string type) => this;
    public IParameterBuilder WithDefaultValue(string defaultValue) => this;
    public IParameterBuilder AsOptional() => this;
    public IParameterBuilder AsRef() => this;
    public IParameterBuilder AsOut() => this;
    public IParameterBuilder AsParams() => this;
    public IParameterBuilder AddAttribute(string name, params object[] arguments) => this;

    public IParameterDefinition Build() => new ParameterDefinition { Name = _name, Type = _type };
}

internal sealed class GenericNamespaceBuilder : INamespaceBuilder
{
    private readonly string _name;

    public GenericNamespaceBuilder(string name) => _name = name;

    public INamespaceBuilder AddClass(Action<IClassBuilder> configure) => this;
    public INamespaceBuilder AddInterface(Action<IInterfaceBuilder> configure) => this;
    public INamespaceBuilder AddNamespace(Action<INamespaceBuilder> configure) => this;

    public INamespaceDefinition Build() => new NamespaceDefinition { Name = _name };
}

internal sealed class GenericCompilationUnitBuilder : ICompilationUnitBuilder
{
    public ICompilationUnitBuilder AddUsing(string @namespace) => this;
    public ICompilationUnitBuilder AddNamespace(Action<INamespaceBuilder> configure) => this;
    public ICompilationUnitBuilder AddClass(Action<IClassBuilder> configure) => this;
    public ICompilationUnitBuilder AddInterface(Action<IInterfaceBuilder> configure) => this;

    public ICompilationUnitDefinition Build() => new CompilationUnitDefinition();
}