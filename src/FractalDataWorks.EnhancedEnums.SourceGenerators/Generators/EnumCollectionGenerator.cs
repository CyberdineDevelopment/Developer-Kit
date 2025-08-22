using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.EnhancedEnums.Discovery;
using FractalDataWorks.EnhancedEnums.Services;
using FractalDataWorks.EnhancedEnums.SourceGenerators.Services.Builders;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.EnhancedEnums.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.EnhancedEnums.SourceGenerators.Generators;

/// <summary>
/// Source generator for Enhanced Enums that supports both local and cross-assembly discovery.
/// 
/// LOCAL DISCOVERY (default):
/// - Uses [EnumCollection] attribute
/// - Scans the current compilation for enum options
/// 
/// GLOBAL DISCOVERY (opt-in):
/// - Uses [GlobalEnumCollection] attribute  
/// - Scans ALL referenced assemblies for enum options
/// - Enables cross-assembly enum composition patterns
/// </summary>
[Generator]
public class EnumCollectionGenerator : IIncrementalGenerator
{
    // Cache for assembly types to avoid re-scanning
    private static readonly ConcurrentDictionary<string, List<INamedTypeSymbol>> _assemblyTypeCache = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes the incremental source generator using the same pattern as GlobalEnhancedEnumGenerator.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        RegisterMainSourceOutput(context);
#if DEBUG
        RegisterPostInitializationOutput(context);
#endif
    }

    private static void RegisterPostInitializationOutput(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("EnumCollectionGenerator_Debug.cs", 
                "// EnumCollectionGenerator Initialize() was called - generator is loaded!");
        });
    }

    private static void RegisterMainSourceOutput(IncrementalGeneratorInitializationContext context)
    {
        var enumDefinitionsProvider = context.CompilationProvider
            .Select((compilation, _) => DiscoverAllCollectionDefinitions(compilation));

        context.RegisterSourceOutput(enumDefinitionsProvider, (context, enumDefinitions) =>
        {
            foreach (var enumDefinition in enumDefinitions)
            {
                Execute(context, enumDefinition.EnumTypeInfo, enumDefinition.Compilation, enumDefinition.DiscoveredOptionTypes);
            }
        });
    }

    /// <summary>
    /// Discovers all collection definitions by scanning for classes with [EnumCollection] or [GlobalEnumCollection] attributes.
    /// </summary>
    private static ImmutableArray<EnumTypeInfoWithCompilation> DiscoverAllCollectionDefinitions(Compilation compilation)
    {
        var results = new List<EnumTypeInfoWithCompilation>();

        // Step 1: Scan for collection classes with [EnumCollection] or [GlobalEnumCollection]
        var collectionClasses = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        
        // Always scan current compilation
        ScanForCollectionClasses(compilation.GlobalNamespace, collectionClasses);
        
        // For each collection class found, determine if we should scan globally
        foreach (var collectionClass in collectionClasses)
        {
            var isGlobal = HasGlobalEnumCollectionAttribute(collectionClass);
            var baseType = ExtractBaseTypeFromCollection(collectionClass);
            
            if (baseType == null)
                continue;
                
            var optionTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            
            if (isGlobal)
            {
                // Scan all referenced assemblies
                foreach (var reference in compilation.References)
                {
                    if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
                    {
                        ScanForOptionTypesOfBase(assemblySymbol.GlobalNamespace, baseType, optionTypes);
                    }
                }
            }
            else
            {
                // Scan only current compilation
                ScanForOptionTypesOfBase(compilation.GlobalNamespace, baseType, optionTypes);
            }
            
            // Create enum definition for this collection
            if (optionTypes.Count > 0 || true) // Always generate even if empty for Empty() support
            {
                var enumDefinition = BuildEnumDefinitionFromCollection(collectionClass, baseType, optionTypes.ToList(), compilation);
                if (enumDefinition != null)
                {
                    results.Add(new EnumTypeInfoWithCompilation(enumDefinition, compilation, optionTypes.ToList()));
                }
            }
        }

        return results.ToImmutableArray();
    }

    private static void ScanForCollectionClasses(INamespaceSymbol namespaceSymbol, HashSet<INamedTypeSymbol> collectionClasses)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            if (HasCollectionAttribute(type))
            {
                collectionClasses.Add(type);
            }
            
            // Recursively scan nested types
            ScanNestedTypesForCollections(type, collectionClasses);
        }

        // Recursively scan nested namespaces
        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            ScanForCollectionClasses(nestedNamespace, collectionClasses);
        }
    }

    private static void ScanNestedTypesForCollections(INamedTypeSymbol typeSymbol, HashSet<INamedTypeSymbol> collectionClasses)
    {
        foreach (var nestedType in typeSymbol.GetTypeMembers())
        {
            if (HasCollectionAttribute(nestedType))
            {
                collectionClasses.Add(nestedType);
            }
            
            // Recursively scan further nested types
            ScanNestedTypesForCollections(nestedType, collectionClasses);
        }
    }

    private static bool HasCollectionAttribute(INamedTypeSymbol type)
    {
        return type.GetAttributes().Any(attr =>
            string.Equals(attr.AttributeClass?.Name, "EnumCollectionAttribute", StringComparison.Ordinal) ||
            string.Equals(attr.AttributeClass?.Name, "EnumCollection", StringComparison.Ordinal) ||
            string.Equals(attr.AttributeClass?.Name, "GlobalEnumCollectionAttribute", StringComparison.Ordinal) ||
            string.Equals(attr.AttributeClass?.Name, "GlobalEnumCollection", StringComparison.Ordinal));
    }

    private static bool HasGlobalEnumCollectionAttribute(INamedTypeSymbol type)
    {
        return type.GetAttributes().Any(attr =>
            string.Equals(attr.AttributeClass?.Name, "GlobalEnumCollectionAttribute", StringComparison.Ordinal) ||
            string.Equals(attr.AttributeClass?.Name, "GlobalEnumCollection", StringComparison.Ordinal));
    }

    private static INamedTypeSymbol? ExtractBaseTypeFromCollection(INamedTypeSymbol collectionClass)
    {
        // For collection-first pattern: extract from generic constraint
        // e.g., ColorsCollectionBase<T> where T : ColorOptionBase
        if (collectionClass.IsGenericType && collectionClass.TypeParameters.Length > 0)
        {
            var firstTypeParam = collectionClass.TypeParameters[0];
            if (firstTypeParam.ConstraintTypes.Length > 0)
            {
                return firstTypeParam.ConstraintTypes[0] as INamedTypeSymbol;
            }
        }
        
        // For classes that inherit from EnumCollectionBase<T>, extract T
        // e.g., DocumentTypeCollection : EnumCollectionBase<DocumentTypeBase>
        var currentBase = collectionClass.BaseType;
        while (currentBase != null)
        {
            if (currentBase.IsGenericType && currentBase.TypeArguments.Length > 0)
            {
                // Check if this is EnumCollectionBase<T>
                var constructedFrom = currentBase.ConstructedFrom;
                if (constructedFrom != null && 
                    (string.Equals(constructedFrom.Name, "EnumCollectionBase", StringComparison.Ordinal) || 
                     constructedFrom.ToDisplayString().Contains("EnumCollectionBase")))
                {
                    return currentBase.TypeArguments[0] as INamedTypeSymbol;
                }
            }
            currentBase = currentBase.BaseType;
        }
        
        // Return null if it's not a proper collection-first pattern
        return null;
    }

    private static void ScanForOptionTypesOfBase(INamespaceSymbol namespaceSymbol, INamedTypeSymbol baseType, HashSet<INamedTypeSymbol> optionTypes)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            if (!type.IsAbstract && DerivesFromBaseType(type, baseType))
            {
                // Check for [EnumOption] attribute is optional - any concrete type deriving from base is included
                optionTypes.Add(type);
            }
            
            // Recursively scan nested types
            ScanNestedTypesForOptionTypesOfBase(type, baseType, optionTypes);
        }

        // Recursively scan nested namespaces
        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            ScanForOptionTypesOfBase(nestedNamespace, baseType, optionTypes);
        }
    }

    private static void ScanNestedTypesForOptionTypesOfBase(INamedTypeSymbol typeSymbol, INamedTypeSymbol baseType, HashSet<INamedTypeSymbol> optionTypes)
    {
        foreach (var nestedType in typeSymbol.GetTypeMembers())
        {
            if (!nestedType.IsAbstract && DerivesFromBaseType(nestedType, baseType))
            {
                optionTypes.Add(nestedType);
            }
            
            // Recursively scan further nested types
            ScanNestedTypesForOptionTypesOfBase(nestedType, baseType, optionTypes);
        }
    }

    private static bool DerivesFromBaseType(INamedTypeSymbol derivedType, INamedTypeSymbol baseType)
    {
        var currentBase = derivedType.BaseType;
        while (currentBase != null)
        {
            if (SymbolEqualityComparer.Default.Equals(currentBase, baseType))
            {
                return true;
            }
            
            // For generic types, compare the unbound generic type
            if (baseType.IsGenericType && currentBase.IsGenericType)
            {
                var currentUnbound = currentBase.ConstructedFrom ?? currentBase;
                var baseUnbound = baseType.ConstructedFrom ?? baseType;
                
                if (SymbolEqualityComparer.Default.Equals(currentUnbound, baseUnbound))
                {
                    return true;
                }
            }
            
            currentBase = currentBase.BaseType;
        }
        return false;
    }

    private static bool CheckIfInheritsFromEnumCollectionBase(INamedTypeSymbol collectionClass, Compilation compilation)
    {
        // Get the EnumCollectionBase<T> type from the compilation
        var enumCollectionBase = compilation.GetTypeByMetadataName("FractalDataWorks.EnhancedEnums.EnumCollectionBase`1");
        if (enumCollectionBase == null)
            return false;
        
        // Check if the collection class inherits from EnumCollectionBase<T>
        var currentBase = collectionClass.BaseType;
        while (currentBase != null)
        {
            // Check for generic match
            if (currentBase.IsGenericType && currentBase.ConstructedFrom != null)
            {
                if (SymbolEqualityComparer.Default.Equals(currentBase.ConstructedFrom, enumCollectionBase))
                {
                    return true;
                }
            }
            
            currentBase = currentBase.BaseType;
        }
        
        return false;
    }

    private static EnumTypeInfo? BuildEnumDefinitionFromCollection(
        INamedTypeSymbol collectionClass, 
        INamedTypeSymbol baseType,
        List<INamedTypeSymbol> optionTypes, 
        Compilation compilation)
    {
        // Extract attribute data
        var attr = collectionClass.GetAttributes().FirstOrDefault(a => 
            string.Equals(a.AttributeClass?.Name, "EnumCollectionAttribute", StringComparison.Ordinal) ||
            string.Equals(a.AttributeClass?.Name, "EnumCollection", StringComparison.Ordinal) ||
            string.Equals(a.AttributeClass?.Name, "GlobalEnumCollectionAttribute", StringComparison.Ordinal) ||
            string.Equals(a.AttributeClass?.Name, "GlobalEnumCollection", StringComparison.Ordinal));
        if (attr == null)
            return null;
            
        var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);
        
        // Check if the collection class inherits from EnumCollectionBase<T>
        var inheritsFromBase = CheckIfInheritsFromEnumCollectionBase(collectionClass, compilation);
        
        // Build EnumTypeInfo from the collection class
        var defaultNamespace = collectionClass.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        var enumTypeInfo = new EnumTypeInfo
        {
            Namespace = named.TryGetValue("Namespace", out var ns) && ns.Value is string nsValue && !string.IsNullOrEmpty(nsValue) 
                ? nsValue : defaultNamespace,
            ClassName = baseType.Name,
            FullTypeName = baseType.ToDisplayString(),
            CollectionName = ExtractCollectionName(attr, collectionClass),
            CollectionBaseType = baseType.ToDisplayString(),
            IsGenericType = baseType.IsGenericType,
            GenerateFactoryMethods = named.TryGetValue("GenerateFactoryMethods", out var gfm) ? (bool)(gfm.Value ?? true) : true,
            GenerateStaticCollection = named.TryGetValue("GenerateStaticCollection", out var gsc) ? (bool)(gsc.Value ?? true) : true,
            Generic = named.TryGetValue("Generic", out var gen) ? (bool)(gen.Value ?? false) : false,
            NameComparison = named.TryGetValue("NameComparison", out var nc) && nc.Value is int ic 
                ? (StringComparison)ic : StringComparison.OrdinalIgnoreCase,
            UseSingletonInstances = named.TryGetValue("UseSingletonInstances", out var usi) ? (bool)(usi.Value ?? true) : true,
            ReturnType = named.TryGetValue("ReturnType", out var rt) && rt.Value is INamedTypeSymbol rts ? rts.ToDisplayString() : null,
            DefaultGenericReturnType = named.TryGetValue("DefaultGenericReturnType", out var dgrt) && dgrt.Value is INamedTypeSymbol dgrts ? dgrts.ToDisplayString() : null,
            LookupProperties = ExtractLookupPropertiesFromBaseType(baseType),
            InheritsFromCollectionBase = inheritsFromBase,
            CollectionClassName = collectionClass.ToDisplayString()
        };
        
        return enumTypeInfo;
    }

    private static string ExtractCollectionName(AttributeData attr, INamedTypeSymbol collectionClass)
    {
        // Try to get from attribute
        var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);
        if (named.TryGetValue("CollectionName", out var cn) && cn.Value is string collName && !string.IsNullOrEmpty(collName))
        {
            return collName;
        }
        
        // Derive from class name
        var className = collectionClass.Name;
        if (className.EndsWith("CollectionBase", StringComparison.Ordinal))
        {
            return className.Substring(0, className.Length - "CollectionBase".Length);
        }
        
        return className + "s"; // Default: add 's'
    }

    private static EquatableArray<PropertyLookupInfo> ExtractLookupPropertiesFromBaseType(INamedTypeSymbol baseType)
    {
        var lookupProperties = new List<PropertyLookupInfo>();
        
        // Traverse the inheritance chain to find all properties with [EnumLookup] attributes
        var currentType = baseType;
        while (currentType != null)
        {
            foreach (var prop in currentType.GetMembers().OfType<IPropertySymbol>())
            {
                var lookupAttr = prop.GetAttributes()
                    .FirstOrDefault(ad => string.Equals(ad.AttributeClass?.Name, "EnumLookupAttribute", StringComparison.Ordinal) ||
                                         string.Equals(ad.AttributeClass?.Name, "EnumLookup", StringComparison.Ordinal));
                if (lookupAttr == null)
                    continue;
                
                // Get constructor arguments - MethodName is the first parameter
                var constructorArgs = lookupAttr.ConstructorArguments;
                var methodName = constructorArgs.Length > 0 && constructorArgs[0].Value is string mn 
                    ? mn : $"GetBy{prop.Name}";
                
                var allowMultiple = constructorArgs.Length > 1 && constructorArgs[1].Value is bool mu && mu;
                var returnType = constructorArgs.Length > 2 && constructorArgs[2].Value is INamedTypeSymbol rts 
                    ? rts.ToDisplayString() : null;

                lookupProperties.Add(new PropertyLookupInfo
                {
                    PropertyName = prop.Name,
                    PropertyType = prop.Type.ToDisplayString(),
                    LookupMethodName = methodName,
                    AllowMultiple = allowMultiple,
                    IsNullable = prop.Type.NullableAnnotation == NullableAnnotation.Annotated,
                    ReturnType = returnType,
                    RequiresOverride = prop.IsAbstract,
                });
            }
            
            currentType = currentType.BaseType;
        }
        
        return new EquatableArray<PropertyLookupInfo>(lookupProperties);
    }

    private static void Execute(
        SourceProductionContext context, 
        EnumTypeInfo def, 
        Compilation compilation,
        List<INamedTypeSymbol> discoveredOptionTypes)
    {
        if (def == null)
            throw new ArgumentNullException(nameof(def));
        if (compilation == null)
            throw new ArgumentNullException(nameof(compilation));

        var baseTypeSymbol = compilation.GetTypeByMetadataName(def.CollectionBaseType ?? def.FullTypeName);
        if (baseTypeSymbol == null)
            return;

        // Auto-detect return type if not specified
        if (string.IsNullOrEmpty(def.ReturnType))
        {
            def.ReturnType = DetectReturnType(baseTypeSymbol, compilation);
        }

        // Convert discovered option types to EnumValueInfo objects
        var values = new List<EnumValueInfo>();
        foreach (var optionType in discoveredOptionTypes)
        {
            var enumValueInfo = new EnumValueInfo
            {
                ShortTypeName = optionType.Name,
                FullTypeName = optionType.ToDisplayString(),
                Name = optionType.Name,
                ReturnTypeNamespace = optionType.ContainingNamespace?.ToDisplayString() ?? string.Empty
            };
            values.Add(enumValueInfo);
        }

        // Generate the collection class
        GenerateCollection(context, def, new EquatableArray<EnumValueInfo>(values), compilation);
    }

    private static string DetectReturnType(INamedTypeSymbol baseTypeSymbol, Compilation compilation)
    {
        // Get IEnumOption interface from FractalDataWorks core
        var enhancedEnumInterface = compilation.GetTypeByMetadataName("FractalDataWorks.IEnumOption") 
            ?? compilation.GetTypeByMetadataName("FractalDataWorks.EnhancedEnums.IEnumOption");
        if (enhancedEnumInterface == null)
        {
            return baseTypeSymbol.ToDisplayString();
        }

        // Check all interfaces implemented by the base type
        foreach (var iface in baseTypeSymbol.AllInterfaces)
        {
            if (iface.AllInterfaces.Contains(enhancedEnumInterface, SymbolEqualityComparer.Default))
            {
                return iface.ToDisplayString();
            }
            
            if (SymbolEqualityComparer.Default.Equals(iface, enhancedEnumInterface))
            {
                continue;
            }
        }

        return baseTypeSymbol.ToDisplayString();
    }
    
    private static void GenerateCollection(SourceProductionContext context, EnumTypeInfo def, EquatableArray<EnumValueInfo> values, Compilation compilation)
    {
        if (def == null)
            throw new ArgumentNullException(nameof(def));

        var baseTypeSymbol = GetBaseTypeSymbol(def, compilation);
        var effectiveReturnType = DetermineEffectiveReturnType(def, baseTypeSymbol, compilation);

        // Use the new builder pattern with director
        var builder = new EnumCollectionBuilder();
        var director = new EnumCollectionDirector(builder);
        
        var generatedCode = def.InheritsFromCollectionBase 
            ? director.ConstructSimplifiedCollection(def, values.ToList(), effectiveReturnType!, compilation)
            : director.ConstructFullCollection(def, values.ToList(), effectiveReturnType!, compilation);
        
        var fileName = $"{def.CollectionName}.g.cs";
        context.AddSource(fileName, generatedCode);
        
        // Conditionally emit to disk if GeneratorOutPutTo is specified
        EmitFileToDiskIfRequested(context, fileName, generatedCode);
    }

    private static INamedTypeSymbol? GetBaseTypeSymbol(EnumTypeInfo def, Compilation compilation)
    {
        return def.IsGenericType && !string.IsNullOrEmpty(def.UnboundTypeName)
            ? compilation.GetTypeByMetadataName(def.UnboundTypeName)
            : compilation.GetTypeByMetadataName(def.FullTypeName);
    }

    private static string? DetermineEffectiveReturnType(EnumTypeInfo def, INamedTypeSymbol? baseTypeSymbol, Compilation compilation)
    {
        if (!string.IsNullOrEmpty(def.ReturnType))
            return def.ReturnType;
        
        if (def.IsGenericType && !string.IsNullOrEmpty(def.DefaultGenericReturnType))
            return def.DefaultGenericReturnType;
        
        return baseTypeSymbol != null ? DetectReturnType(baseTypeSymbol, compilation) : def.FullTypeName;
    }

    /// <summary>
    /// Conditionally emits the generated file to disk if GeneratorOutPutTo MSBuild property is set.
    /// </summary>
    private static void EmitFileToDiskIfRequested(SourceProductionContext context, string fileName, string content)
    {
        // File I/O removed - not allowed in source generators
    }
}