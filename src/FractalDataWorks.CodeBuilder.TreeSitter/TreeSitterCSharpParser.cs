using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks;
using static TreeSitter.Bindings.Native;

namespace FractalDataWorks.CodeBuilder.TreeSitter;

/// <summary>
/// TreeSitter-based parser for C# code.
/// </summary>
public sealed class TreeSitterCSharpParser : ICodeParser, IDisposable
{
    private readonly IntPtr _parser;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeSitterCSharpParser"/> class.
    /// </summary>
    public TreeSitterCSharpParser()
    {
        _parser = TsParserNew();
        // Note: In a real implementation, you would load the C# language library here
        // For now, this is a placeholder that demonstrates the structure
        // TsParserSetLanguage(_parser, csharpLanguage);
    }

    /// <inheritdoc/>
    public string Language => "csharp";

    /// <inheritdoc/>
    public async Task<IFdwResult<ISyntaxTree>> ParseAsync(
        string sourceCode,
        string? filePath = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sourceCode))
        {
            return FdwResult<ISyntaxTree>.Failure("Source code cannot be null or empty");
        }

        try
        {
            return await Task.Run(() =>
            {
                // Convert string to UTF-8 bytes
                var sourceBytes = Encoding.UTF8.GetBytes(sourceCode);
                
                // Parse the source code
                var tree = TsParserParseString(_parser, IntPtr.Zero, sourceCode, (uint)sourceBytes.Length);
                
                if (tree == IntPtr.Zero)
                {
                    return FdwResult<ISyntaxTree>.Failure("Failed to parse source code");
                }

                var syntaxTree = new TreeSitterSyntaxTree(tree, sourceCode, Language, filePath);
                return FdwResult<ISyntaxTree>.Success(syntaxTree);
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return FdwResult<ISyntaxTree>.Failure($"Parse error: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> ValidateAsync(
        string sourceCode,
        CancellationToken cancellationToken = default)
    {
        var parseResult = await ParseAsync(sourceCode, null, cancellationToken).ConfigureAwait(false);
        
        if (parseResult.IsFailure)
        {
            return FdwResult.Failure(parseResult.Message ?? "Validation failed");
        }

        if (parseResult.Value!.HasErrors)
        {
            var errorCount = 0;
            foreach (var _ in parseResult.Value.GetErrors())
            {
                errorCount++;
            }
            return FdwResult.Failure($"Source code contains {errorCount} syntax error(s)");
        }

        return FdwResult.Success();
    }

    /// <summary>
    /// Disposes the parser.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_parser != IntPtr.Zero)
            {
                TsParserDelete(_parser);
            }
            _disposed = true;
        }
    }
}