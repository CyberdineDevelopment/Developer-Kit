using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.CodeBuilder.CSharp.Builders;

namespace FractalDataWorks.EnhancedEnums.SourceGenerators.Services.Builders;

/// <summary>
/// Director class for the Gang of Four Builder pattern that orchestrates the construction
/// of enhanced enum collections. This class determines the appropriate construction strategy
/// and manages the building process for different collection scenarios.
/// </summary>
public sealed class EnumCollectionDirector
{
    private readonly IEnumCollectionBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumCollectionDirector"/> class.
    /// </summary>
    /// <param name="builder">The enum collection builder to orchestrate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public EnumCollectionDirector(IEnumCollectionBuilder builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>
    /// Constructs a complete enum collection with all features enabled.
    /// This includes static fields, lookup methods, factory methods, and full functionality.
    /// Suitable for comprehensive enum collections that need all available features.
    /// </summary>
    /// <param name="definition">The enum type definition containing metadata for code generation.</param>
    /// <param name="values">The list of enum values to include in the collection.</param>
    /// <param name="returnType">The return type for generated collection methods and properties.</param>
    /// <param name="compilation">The compilation context from the source generator.</param>
    /// <returns>The generated source code for the complete enum collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="returnType"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the builder configuration is invalid.</exception>
    public string ConstructFullCollection(
        EnumTypeInfo definition,
        IList<EnumValueInfo> values,
        string returnType,
        Compilation compilation)
    {
        ValidateParameters(definition, values, returnType, compilation);

        var mode = DetermineGenerationMode(definition);
        
        return _builder
            .Configure(mode)
            .WithDefinition(definition)
            .WithValues(values)
            .WithReturnType(returnType)
            .WithCompilation(compilation)
            .Build();
    }

    /// <summary>
    /// Constructs a simplified enum collection for types inheriting from EnumCollectionBase.
    /// This generates only the necessary method implementations and avoids duplicating
    /// functionality already provided by the base class.
    /// </summary>
    /// <param name="definition">The enum type definition containing metadata for code generation.</param>
    /// <param name="values">The list of enum values to include in the collection.</param>
    /// <param name="returnType">The return type for generated collection methods and properties.</param>
    /// <param name="compilation">The compilation context from the source generator.</param>
    /// <returns>The generated source code for the simplified enum collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="returnType"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the definition does not inherit from collection base or when the builder configuration is invalid.
    /// </exception>
    public string ConstructSimplifiedCollection(
        EnumTypeInfo definition,
        IList<EnumValueInfo> values,
        string returnType,
        Compilation compilation)
    {
        ValidateParameters(definition, values, returnType, compilation);

        if (!definition.InheritsFromCollectionBase)
        {
            throw new InvalidOperationException("Simplified collection construction requires the definition to inherit from EnumCollectionBase.");
        }

        // For simplified collections, always use static pattern as base class handles instantiation
        var mode = CollectionGenerationMode.StaticCollection;
        
        return _builder
            .Configure(mode)
            .WithDefinition(definition)
            .WithValues(values)
            .WithReturnType(returnType)
            .WithCompilation(compilation)
            .Build();
    }

    /// <summary>
    /// Constructs a generic enum collection that supports parameterized types.
    /// This pattern enables type-safe collections that can work with multiple enum types
    /// through generic constraints and type parameters.
    /// </summary>
    /// <param name="definition">The enum type definition containing metadata for code generation.</param>
    /// <param name="values">The list of enum values to include in the collection.</param>
    /// <param name="returnType">The return type for generated collection methods and properties.</param>
    /// <param name="compilation">The compilation context from the source generator.</param>
    /// <returns>The generated source code for the generic enum collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="returnType"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the definition is not configured for generic types or when the builder configuration is invalid.
    /// </exception>
    public string ConstructGenericCollection(
        EnumTypeInfo definition,
        IList<EnumValueInfo> values,
        string returnType,
        Compilation compilation)
    {
        ValidateParameters(definition, values, returnType, compilation);

        if (!definition.Generic && !definition.IsGenericType)
        {
            throw new InvalidOperationException("Generic collection construction requires the definition to be configured for generic types.");
        }

        // Generic collections are typically static for type safety and performance
        var mode = definition.UseSingletonInstances 
            ? CollectionGenerationMode.StaticCollection 
            : CollectionGenerationMode.FactoryCollection;
        
        return _builder
            .Configure(mode)
            .WithDefinition(definition)
            .WithValues(values)
            .WithReturnType(returnType)
            .WithCompilation(compilation)
            .Build();
    }

    /// <summary>
    /// Determines the appropriate collection generation mode based on the enum type definition properties.
    /// This method analyzes the definition's configuration to select the optimal generation strategy.
    /// </summary>
    /// <param name="definition">The enum type definition to analyze.</param>
    /// <returns>The recommended collection generation mode.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
    private static CollectionGenerationMode DetermineGenerationMode(EnumTypeInfo definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        // If explicitly configured for static generation
        if (definition.GenerateStaticCollection)
        {
            return CollectionGenerationMode.StaticCollection;
        }

        // If using singleton instances, prefer instance collection with singleton pattern
        if (definition.UseSingletonInstances)
        {
            return CollectionGenerationMode.InstanceCollection;
        }

        // If factory methods are enabled without singleton instances, use factory pattern
        if (definition.GenerateFactoryMethods)
        {
            return CollectionGenerationMode.FactoryCollection;
        }

        // Check strategy name for service pattern
        if (string.Equals(definition.Strategy, "Service", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.Strategy, "DependencyInjection", StringComparison.OrdinalIgnoreCase))
        {
            return CollectionGenerationMode.ServiceCollection;
        }

        // Default to static collection for best performance
        return CollectionGenerationMode.StaticCollection;
    }

    /// <summary>
    /// Validates that all required parameters are provided and valid.
    /// </summary>
    /// <param name="definition">The enum type definition to validate.</param>
    /// <param name="values">The enum values to validate.</param>
    /// <param name="returnType">The return type to validate.</param>
    /// <param name="compilation">The compilation context to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="returnType"/> is null or empty.</exception>
    private static void ValidateParameters(
        EnumTypeInfo definition,
        IList<EnumValueInfo> values,
        string returnType,
        Compilation compilation)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (string.IsNullOrEmpty(returnType))
        {
            throw new ArgumentException("Return type cannot be null or empty.", nameof(returnType));
        }

        if (compilation == null)
        {
            throw new ArgumentNullException(nameof(compilation));
        }
    }
}