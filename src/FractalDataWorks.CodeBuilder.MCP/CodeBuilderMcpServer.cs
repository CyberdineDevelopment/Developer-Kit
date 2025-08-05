using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.CodeBuilder.MCP;

/// <summary>
/// MCP server for code building operations.
/// Provides AI agents with comprehensive code analysis and building capabilities.
/// </summary>
public sealed class CodeBuilderMcpServer : McpServerBase
{
    private readonly ICodeSessionManager _sessionManager;
    private readonly ICodeParserFactory _parserFactory;
    private readonly ICodeTransformerFactory _transformerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeBuilderMcpServer"/> class.
    /// </summary>
    /// <param name="sessionManager">The session manager for code sessions.</param>
    /// <param name="parserFactory">The parser factory for different languages.</param>
    /// <param name="transformerFactory">The transformer factory for code transformations.</param>
    /// <param name="logger">Optional logger instance.</param>
    public CodeBuilderMcpServer(
        ICodeSessionManager sessionManager,
        ICodeParserFactory parserFactory,
        ICodeTransformerFactory transformerFactory,
        ILogger<CodeBuilderMcpServer>? logger = null)
        : base("code-builder", "1.0.0", logger)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _parserFactory = parserFactory ?? throw new ArgumentNullException(nameof(parserFactory));
        _transformerFactory = transformerFactory ?? throw new ArgumentNullException(nameof(transformerFactory));

        RegisterTools();
    }

    /// <summary>
    /// Gets the supported MCP tools for code building operations.
    /// </summary>
    public override IReadOnlyList<McpTool> Tools => new[]
    {
        CreateParseCodeTool(),
        CreateBuildCodeTool(),
        CreateTransformCodeTool(),
        CreateAnalyzeCodeTool(),
        CreateGetCompletionsTool(),
        CreateGetSessionInfoTool(),
        CreateCreateSessionTool(),
        CreateUpdateSessionTool(),
        CreateCompileSessionTool()
    };

    /// <summary>
    /// Executes an MCP tool with the specified parameters.
    /// </summary>
    /// <param name="toolName">The name of the tool to execute.</param>
    /// <param name="parameters">The tool parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tool execution result.</returns>
    public override async Task<McpToolResult> ExecuteToolAsync(
        string toolName,
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Executing CodeBuilder MCP tool: {ToolName}", toolName);

            var result = toolName switch
            {
                "parse-code" => await HandleParseCode(parameters, cancellationToken).ConfigureAwait(false),
                "build-code" => await HandleBuildCode(parameters, cancellationToken).ConfigureAwait(false),
                "transform-code" => await HandleTransformCode(parameters, cancellationToken).ConfigureAwait(false),
                "analyze-code" => await HandleAnalyzeCode(parameters, cancellationToken).ConfigureAwait(false),
                "get-completions" => await HandleGetCompletions(parameters, cancellationToken).ConfigureAwait(false),
                "get-session-info" => await HandleGetSessionInfo(parameters, cancellationToken).ConfigureAwait(false),
                "create-session" => await HandleCreateSession(parameters, cancellationToken).ConfigureAwait(false),
                "update-session" => await HandleUpdateSession(parameters, cancellationToken).ConfigureAwait(false),
                "compile-session" => await HandleCompileSession(parameters, cancellationToken).ConfigureAwait(false),
                _ => McpToolResult.Error($"Unknown tool: {toolName}")
            };

            Logger.LogDebug("Successfully executed CodeBuilder MCP tool: {ToolName}", toolName);
            return result;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            Logger.LogError(ex, "Error executing CodeBuilder MCP tool: {ToolName}", toolName);
            return McpToolResult.Error($"Tool execution failed: {ex.Message}");
        }
    }

    private void RegisterTools()
    {
        // Tools are automatically registered through the Tools property
        Logger.LogDebug("Registered {ToolCount} CodeBuilder MCP tools", Tools.Count);
    }

    private async Task<McpToolResult> HandleParseCode(
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var language = GetRequiredParameter<string>(parameters, "language");
        var source = GetRequiredParameter<string>(parameters, "source");
        var filePath = GetOptionalParameter<string>(parameters, "filePath");

        var parser = _parserFactory.CreateParser(language);
        if (parser == null)
        {
            return McpToolResult.Error($"No parser available for language: {language}");
        }

        var parseResult = await parser.ParseToDefinitionAsync(source, filePath, cancellationToken).ConfigureAwait(false);
        if (parseResult.IsFailure)
        {
            return McpToolResult.Error($"Parsing failed: {parseResult.Message?.Message}");
        }

        return McpToolResult.Success(new
        {
            language,
            filePath,
            definition = parseResult.Value,
            success = true
        });
    }

    private async Task<McpToolResult> HandleBuildCode(
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var language = GetRequiredParameter<string>(parameters, "language");
        var builderType = GetRequiredParameter<string>(parameters, "builderType");
        var builderParameters = GetOptionalParameter<Dictionary<string, object>>(parameters, "builderParameters") ?? 
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        var codeBuilder = _parserFactory.CreateCodeBuilder(language);
        if (codeBuilder == null)
        {
            return McpToolResult.Error($"No code builder available for language: {language}");
        }

        // This is a simplified implementation - a full implementation would 
        // dynamically create builders based on the builderType and parameters
        try
        {
            var result = builderType.ToLowerInvariant() switch
            {
                "class" => await BuildClass(codeBuilder, builderParameters, cancellationToken).ConfigureAwait(false),
                "method" => await BuildMethod(codeBuilder, builderParameters, cancellationToken).ConfigureAwait(false),
                "property" => await BuildProperty(codeBuilder, builderParameters, cancellationToken).ConfigureAwait(false),
                "interface" => await BuildInterface(codeBuilder, builderParameters, cancellationToken).ConfigureAwait(false),
                _ => throw new ArgumentException($"Unsupported builder type: {builderType}")
            };

            return McpToolResult.Success(new
            {
                language,
                builderType,
                definition = result,
                success = true
            });
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return McpToolResult.Error($"Code building failed: {ex.Message}");
        }
    }

    private async Task<McpToolResult> HandleTransformCode(
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var sessionId = GetRequiredParameter<string>(parameters, "sessionId");
        var transformationName = GetRequiredParameter<string>(parameters, "transformationName");
        var transformationParameters = GetOptionalParameter<Dictionary<string, object>>(parameters, "transformationParameters") ?? 
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (!Guid.TryParse(sessionId, out var guid))
        {
            return McpToolResult.Error("Invalid session ID format");
        }

        var session = await _sessionManager.GetSessionAsync(guid, cancellationToken).ConfigureAwait(false);
        if (session == null)
        {
            return McpToolResult.Error($"Session not found: {sessionId}");
        }

        var transformer = _transformerFactory.CreateTransformer(transformationName);
        if (transformer == null)
        {
            return McpToolResult.Error($"Transformer not found: {transformationName}");
        }

        // Create a transformation using the built-in factory methods
        var transformation = CreateTransformation(transformationName, transformationParameters);
        if (transformation == null)
        {
            return McpToolResult.Error($"Could not create transformation: {transformationName}");
        }

        var result = await session.ApplyTransformationAsync(transformation, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return McpToolResult.Error($"Transformation failed: {result.Message?.Message}");
        }

        return McpToolResult.Success(new
        {
            sessionId,
            transformationName,
            result = result.Value,
            success = true
        });
    }

    private async Task<McpToolResult> HandleAnalyzeCode(
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var sessionId = GetRequiredParameter<string>(parameters, "sessionId");
        var filePath = GetOptionalParameter<string>(parameters, "filePath");
        var position = GetOptionalParameter<int?>(parameters, "position");

        if (!Guid.TryParse(sessionId, out var guid))
        {
            return McpToolResult.Error("Invalid session ID format");
        }

        var session = await _sessionManager.GetSessionAsync(guid, cancellationToken).ConfigureAwait(false);
        if (session == null)
        {
            return McpToolResult.Error($"Session not found: {sessionId}");
        }

        var analysis = new
        {
            sessionId,
            diagnostics = session.Diagnostics,
            hasErrors = session.HasErrors,
            sourceFiles = session.SourceFiles.Keys,
            references = session.References,
            semanticInfo = !string.IsNullOrEmpty(filePath) && position.HasValue
                ? await GetSemanticInfo(session, filePath, position.Value, cancellationToken).ConfigureAwait(false)
                : null
        };

        return McpToolResult.Success(analysis);
    }

    private async Task<McpToolResult> HandleGetCompletions(
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var sessionId = GetRequiredParameter<string>(parameters, "sessionId");
        var filePath = GetRequiredParameter<string>(parameters, "filePath");
        var position = GetRequiredParameter<int>(parameters, "position");

        if (!Guid.TryParse(sessionId, out var guid))
        {
            return McpToolResult.Error("Invalid session ID format");
        }

        var session = await _sessionManager.GetSessionAsync(guid, cancellationToken).ConfigureAwait(false);
        if (session == null)
        {
            return McpToolResult.Error($"Session not found: {sessionId}");
        }

        var completionsResult = await session.GetCompletionsAsync(filePath, position, cancellationToken).ConfigureAwait(false);
        if (completionsResult.IsFailure)
        {
            return McpToolResult.Error($"Failed to get completions: {completionsResult.Message?.Message}");
        }

        return McpToolResult.Success(new
        {
            sessionId,
            filePath,
            position,
            completions = completionsResult.Value,
            success = true
        });
    }

    private async Task<McpToolResult> HandleGetSessionInfo(
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var sessionId = GetRequiredParameter<string>(parameters, "sessionId");

        if (!Guid.TryParse(sessionId, out var guid))
        {
            return McpToolResult.Error("Invalid session ID format");
        }

        var session = await _sessionManager.GetSessionAsync(guid, cancellationToken).ConfigureAwait(false);
        if (session == null)
        {
            return McpToolResult.Error($"Session not found: {sessionId}");
        }

        return McpToolResult.Success(new
        {
            sessionId = session.SessionId,
            language = session.Language,
            created = session.Created,
            lastModified = session.LastModified,
            isValid = session.IsValid,
            sourceFiles = session.SourceFiles.Keys,
            references = session.References,
            hasErrors = session.HasErrors,
            diagnosticCount = session.Diagnostics.Count
        });
    }

    private async Task<McpToolResult> HandleCreateSession(
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var language = GetRequiredParameter<string>(parameters, "language");
        var assemblyName = GetOptionalParameter<string>(parameters, "assemblyName") ?? "GeneratedAssembly";
        var references = GetOptionalParameter<string[]>(parameters, "references") ?? Array.Empty<string>();

        var sessionResult = await _sessionManager.CreateSessionAsync(language, assemblyName, references, cancellationToken).ConfigureAwait(false);
        if (sessionResult.IsFailure)
        {
            return McpToolResult.Error($"Failed to create session: {sessionResult.Message?.Message}");
        }

        var session = sessionResult.Value!;
        return McpToolResult.Success(new
        {
            sessionId = session.SessionId,
            language = session.Language,
            assemblyName = assemblyName,
            created = session.Created,
            success = true
        });
    }

    private async Task<McpToolResult> HandleUpdateSession(
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var sessionId = GetRequiredParameter<string>(parameters, "sessionId");
        var filePath = GetRequiredParameter<string>(parameters, "filePath");
        var source = GetRequiredParameter<string>(parameters, "source");

        if (!Guid.TryParse(sessionId, out var guid))
        {
            return McpToolResult.Error("Invalid session ID format");
        }

        var session = await _sessionManager.GetSessionAsync(guid, cancellationToken).ConfigureAwait(false);
        if (session == null)
        {
            return McpToolResult.Error($"Session not found: {sessionId}");
        }

        var updateResult = await session.UpdateSourceAsync(filePath, source, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
        {
            return McpToolResult.Error($"Failed to update session: {updateResult.Message?.Message}");
        }

        return McpToolResult.Success(new
        {
            sessionId,
            filePath,
            lastModified = session.LastModified,
            hasErrors = session.HasErrors,
            diagnosticCount = session.Diagnostics.Count,
            success = true
        });
    }

    private async Task<McpToolResult> HandleCompileSession(
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var sessionId = GetRequiredParameter<string>(parameters, "sessionId");
        var outputPath = GetOptionalParameter<string>(parameters, "outputPath");

        if (!Guid.TryParse(sessionId, out var guid))
        {
            return McpToolResult.Error("Invalid session ID format");
        }

        var session = await _sessionManager.GetSessionAsync(guid, cancellationToken).ConfigureAwait(false);
        if (session == null)
        {
            return McpToolResult.Error($"Session not found: {sessionId}");
        }

        var compileResult = await session.CompileAsync(outputPath, cancellationToken).ConfigureAwait(false);
        if (compileResult.IsFailure)
        {
            return McpToolResult.Error($"Compilation failed: {compileResult.Message?.Message}");
        }

        return McpToolResult.Success(new
        {
            sessionId,
            compilation = compileResult.Value,
            success = true
        });
    }

    // Helper methods for parameter extraction
    private static T GetRequiredParameter<T>(IReadOnlyDictionary<string, object> parameters, string name)
    {
        if (!parameters.TryGetValue(name, out var value))
        {
            throw new ArgumentException($"Required parameter '{name}' not found");
        }

        if (value is T typedValue)
        {
            return typedValue;
        }

        throw new ArgumentException($"Parameter '{name}' must be of type {typeof(T).Name}");
    }

    private static T? GetOptionalParameter<T>(IReadOnlyDictionary<string, object> parameters, string name)
    {
        if (!parameters.TryGetValue(name, out var value))
        {
            return default;
        }

        if (value is T typedValue)
        {
            return typedValue;
        }

        return default;
    }

    // Helper methods for building code constructs
    private async Task<object> BuildClass(ICodeBuilder codeBuilder, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        var name = GetRequiredParameter<string>(parameters, "name");
        var access = GetOptionalParameter<string>(parameters, "access") ?? "public";
        
        var classBuilder = codeBuilder.CreateClass(name);
        
        if (Enum.TryParse<AccessModifier>(access, true, out var accessModifier))
        {
            classBuilder.WithAccess(accessModifier);
        }

        await Task.CompletedTask; // Keep async signature
        return classBuilder.Build();
    }

    private async Task<object> BuildMethod(ICodeBuilder codeBuilder, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        var name = GetRequiredParameter<string>(parameters, "name");
        var returnType = GetOptionalParameter<string>(parameters, "returnType") ?? "void";
        var access = GetOptionalParameter<string>(parameters, "access") ?? "public";
        
        var methodBuilder = codeBuilder.CreateMethod(name).WithReturnType(returnType);
        
        if (Enum.TryParse<AccessModifier>(access, true, out var accessModifier))
        {
            methodBuilder.WithAccess(accessModifier);
        }

        await Task.CompletedTask; // Keep async signature
        return methodBuilder.Build();
    }

    private async Task<object> BuildProperty(ICodeBuilder codeBuilder, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        var name = GetRequiredParameter<string>(parameters, "name");
        var type = GetRequiredParameter<string>(parameters, "type");
        var access = GetOptionalParameter<string>(parameters, "access") ?? "public";
        
        var propertyBuilder = codeBuilder.CreateProperty(name, type);
        
        if (Enum.TryParse<AccessModifier>(access, true, out var accessModifier))
        {
            propertyBuilder.WithAccess(accessModifier);
        }

        // Add default getter/setter
        propertyBuilder.WithGetter().WithSetter();

        await Task.CompletedTask; // Keep async signature
        return propertyBuilder.Build();
    }

    private async Task<object> BuildInterface(ICodeBuilder codeBuilder, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        var name = GetRequiredParameter<string>(parameters, "name");
        var access = GetOptionalParameter<string>(parameters, "access") ?? "public";
        
        var interfaceBuilder = codeBuilder.CreateInterface(name);
        
        if (Enum.TryParse<AccessModifier>(access, true, out var accessModifier))
        {
            interfaceBuilder.WithAccess(accessModifier);
        }

        await Task.CompletedTask; // Keep async signature
        return interfaceBuilder.Build();
    }

    private ICodeTransformation? CreateTransformation(string transformationName, Dictionary<string, object> parameters)
    {
        return transformationName.ToLowerInvariant() switch
        {
            "rename-symbol" => BuiltInTransformations.RenameSymbol(
                GetRequiredParameter<string>(parameters, "oldName"),
                GetRequiredParameter<string>(parameters, "newName"),
                GetOptionalParameter<string[]>(parameters, "targetFiles")),
            "extract-method" => BuiltInTransformations.ExtractMethod(
                GetRequiredParameter<string>(parameters, "filePath"),
                GetRequiredParameter<int>(parameters, "startPosition"),
                GetRequiredParameter<int>(parameters, "endPosition"),
                GetRequiredParameter<string>(parameters, "methodName")),
            _ => null
        };
    }

    private async Task<object?> GetSemanticInfo(ICodeSession session, string filePath, int position, CancellationToken cancellationToken)
    {
        var result = await session.GetSemanticInfoAsync(filePath, position, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? result.Value : null;
    }

    // Tool definitions
    private static McpTool CreateParseCodeTool() => new()
    {
        Name = "parse-code",
        Description = "Parse source code and return AST structure",
        Parameters = new McpToolParameters
        {
            Type = "object",
            Properties = new Dictionary<string, McpParameter>(StringComparer.OrdinalIgnoreCase)
            {
                ["language"] = new() { Type = "string", Description = "Programming language (e.g., 'csharp', 'typescript')" },
                ["source"] = new() { Type = "string", Description = "Source code to parse" },
                ["filePath"] = new() { Type = "string", Description = "Optional file path for context" }
            },
            Required = new[] { "language", "source" }
        }
    };

    private static McpTool CreateBuildCodeTool() => new()
    {
        Name = "build-code",
        Description = "Build code constructs using the fluent API",
        Parameters = new McpToolParameters
        {
            Type = "object",
            Properties = new Dictionary<string, McpParameter>(StringComparer.OrdinalIgnoreCase)
            {
                ["language"] = new() { Type = "string", Description = "Programming language" },
                ["builderType"] = new() { Type = "string", Description = "Type of construct to build (class, method, property, interface)" },
                ["builderParameters"] = new() { Type = "object", Description = "Parameters for the builder" }
            },
            Required = new[] { "language", "builderType" }
        }
    };

    private static McpTool CreateTransformCodeTool() => new()
    {
        Name = "transform-code",
        Description = "Apply code transformations to a session",
        Parameters = new McpToolParameters
        {
            Type = "object",
            Properties = new Dictionary<string, McpParameter>(StringComparer.OrdinalIgnoreCase)
            {
                ["sessionId"] = new() { Type = "string", Description = "Session ID to transform" },
                ["transformationName"] = new() { Type = "string", Description = "Name of transformation to apply" },
                ["transformationParameters"] = new() { Type = "object", Description = "Parameters for the transformation" }
            },
            Required = new[] { "sessionId", "transformationName" }
        }
    };

    private static McpTool CreateAnalyzeCodeTool() => new()
    {
        Name = "analyze-code",
        Description = "Analyze code in a session for diagnostics and semantic information",
        Parameters = new McpToolParameters
        {
            Type = "object",
            Properties = new Dictionary<string, McpParameter>(StringComparer.OrdinalIgnoreCase)
            {
                ["sessionId"] = new() { Type = "string", Description = "Session ID to analyze" },
                ["filePath"] = new() { Type = "string", Description = "Optional specific file to analyze" },
                ["position"] = new() { Type = "integer", Description = "Optional position for semantic analysis" }
            },
            Required = new[] { "sessionId" }
        }
    };

    private static McpTool CreateGetCompletionsTool() => new()
    {
        Name = "get-completions",
        Description = "Get code completions at a specific position",
        Parameters = new McpToolParameters
        {
            Type = "object",
            Properties = new Dictionary<string, McpParameter>(StringComparer.OrdinalIgnoreCase)
            {
                ["sessionId"] = new() { Type = "string", Description = "Session ID" },
                ["filePath"] = new() { Type = "string", Description = "File path" },
                ["position"] = new() { Type = "integer", Description = "Position in the file" }
            },
            Required = new[] { "sessionId", "filePath", "position" }
        }
    };

    private static McpTool CreateGetSessionInfoTool() => new()
    {
        Name = "get-session-info",
        Description = "Get information about a code session",
        Parameters = new McpToolParameters
        {
            Type = "object",
            Properties = new Dictionary<string, McpParameter>(StringComparer.OrdinalIgnoreCase)
            {
                ["sessionId"] = new() { Type = "string", Description = "Session ID" }
            },
            Required = new[] { "sessionId" }
        }
    };

    private static McpTool CreateCreateSessionTool() => new()
    {
        Name = "create-session",
        Description = "Create a new code session",
        Parameters = new McpToolParameters
        {
            Type = "object",
            Properties = new Dictionary<string, McpParameter>(StringComparer.OrdinalIgnoreCase)
            {
                ["language"] = new() { Type = "string", Description = "Programming language" },
                ["assemblyName"] = new() { Type = "string", Description = "Optional assembly name" },
                ["references"] = new() { Type = "array", Description = "Optional assembly references" }
            },
            Required = new[] { "language" }
        }
    };

    private static McpTool CreateUpdateSessionTool() => new()
    {
        Name = "update-session",
        Description = "Update source code in a session",
        Parameters = new McpToolParameters
        {
            Type = "object",
            Properties = new Dictionary<string, McpParameter>(StringComparer.OrdinalIgnoreCase)
            {
                ["sessionId"] = new() { Type = "string", Description = "Session ID" },
                ["filePath"] = new() { Type = "string", Description = "File path" },
                ["source"] = new() { Type = "string", Description = "Source code content" }
            },
            Required = new[] { "sessionId", "filePath", "source" }
        }
    };

    private static McpTool CreateCompileSessionTool() => new()
    {
        Name = "compile-session",
        Description = "Compile a code session",
        Parameters = new McpToolParameters
        {
            Type = "object",
            Properties = new Dictionary<string, McpParameter>(StringComparer.OrdinalIgnoreCase)
            {
                ["sessionId"] = new() { Type = "string", Description = "Session ID" },
                ["outputPath"] = new() { Type = "string", Description = "Optional output path for compilation" }
            },
            Required = new[] { "sessionId" }
        }
    };
}

// MCP Server base types (simplified implementations)

/// <summary>
/// Base class for MCP servers.
/// </summary>
public abstract class McpServerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerBase"/> class.
    /// </summary>
    protected McpServerBase(string name, string version, ILogger? logger = null)
    {
        Name = name;
        Version = version;
        Logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    }

    /// <summary>
    /// Gets the server name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the server version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the logger for this server.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the available tools for this server.
    /// </summary>
    public abstract IReadOnlyList<McpTool> Tools { get; }

    /// <summary>
    /// Executes a tool with the specified parameters.
    /// </summary>
    public abstract Task<McpToolResult> ExecuteToolAsync(
        string toolName,
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an MCP tool.
/// </summary>
public sealed record McpTool
{
    /// <summary>Gets or sets the tool name.</summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>Gets or sets the tool description.</summary>
    public string Description { get; init; } = string.Empty;
    /// <summary>Gets or sets the tool parameters.</summary>
    public McpToolParameters Parameters { get; init; } = new();
}

/// <summary>
/// Represents MCP tool parameters.
/// </summary>
public sealed record McpToolParameters
{
    /// <summary>Gets or sets the parameter type.</summary>
    public string Type { get; init; } = "object";
    /// <summary>Gets or sets the parameter properties.</summary>
    public IReadOnlyDictionary<string, McpParameter> Properties { get; init; } = 
        new Dictionary<string, McpParameter>(StringComparer.OrdinalIgnoreCase);
    /// <summary>Gets or sets the required parameters.</summary>
    public IReadOnlyList<string> Required { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Represents an MCP parameter.
/// </summary>
public sealed record McpParameter
{
    /// <summary>Gets or sets the parameter type.</summary>
    public string Type { get; init; } = "string";
    /// <summary>Gets or sets the parameter description.</summary>
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// Represents the result of an MCP tool execution.
/// </summary>
public sealed record McpToolResult
{
    /// <summary>Gets whether the execution was successful.</summary>
    public bool IsSuccess { get; init; }
    /// <summary>Gets the result data.</summary>
    public object? Data { get; init; }
    /// <summary>Gets the error message, if any.</summary>
    public string? Error { get; init; }

    /// <summary>Creates a successful result.</summary>
    public static McpToolResult Success(object? data = null) => new() { IsSuccess = true, Data = data };
    /// <summary>Creates an error result.</summary>
    public static McpToolResult Error(string error) => new() { IsSuccess = false, Error = error };
}