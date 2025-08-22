using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Discovery;

/// <summary>
/// Symbol extension methods for metadata and helper operations.
/// </summary>
internal static class SymbolExtensions
{
    /// <summary>
    /// Gets the full metadata name of the symbol.
    /// </summary>
    /// <param name="symbol">Symbol.</param>
    /// <returns>Full metadata name.</returns>
    public static string GetFullMetadataName(this ISymbol symbol)
    {
        if (symbol == null)
            return string.Empty;

        var containingNamespace = symbol.ContainingNamespace;
        var namespaceName = containingNamespace == null || containingNamespace.IsGlobalNamespace
            ? string.Empty
            : containingNamespace.GetFullMetadataName();

        return string.IsNullOrEmpty(namespaceName)
            ? symbol.MetadataName
            : $"{namespaceName}.{symbol.MetadataName}";
    }
}