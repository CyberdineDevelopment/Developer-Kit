using System.Collections.Generic;
using FractalDataWorks.EnhancedEnums.Models;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.SourceGenerators.Models;

/// <summary>
/// Helper class to carry EnumTypeInfo with its compilation context and discovered option types.
/// </summary>
internal sealed class EnumTypeInfoWithCompilation
{
    public EnumTypeInfo EnumTypeInfo { get; }
    public Compilation Compilation { get; }
    public List<INamedTypeSymbol> DiscoveredOptionTypes { get; }

    public EnumTypeInfoWithCompilation(EnumTypeInfo enumTypeInfo, Compilation compilation, List<INamedTypeSymbol> discoveredOptionTypes)
    {
        EnumTypeInfo = enumTypeInfo;
        Compilation = compilation;
        DiscoveredOptionTypes = discoveredOptionTypes;
    }
}