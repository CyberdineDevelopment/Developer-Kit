using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.CodeBuilder.CSharp.Builders;

namespace FractalDataWorks.EnhancedEnums.SourceGenerators.Services.Builders;

/// <summary>
/// Concrete implementation of IEnumCollectionBuilder that builds enhanced enum collections
/// using the Gang of Four Builder pattern with fluent API.
/// This builder generates collection classes from enum type definitions and values during source generation.
/// </summary>
#pragma warning disable MA0026 // TODO
// TODO: Break this class into smaller, focused classes (e.g., MethodBuilder, FieldBuilder, ConstructorBuilder)
#pragma warning restore MA0026
public sealed class EnumCollectionBuilder : IEnumCollectionBuilder
{
    private CollectionGenerationMode _mode;
    private EnumTypeInfo? _definition;
    private IList<EnumValueInfo>? _values;
    private string? _returnType;
    private Compilation? _compilation;
    private ClassBuilder? _classBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumCollectionBuilder"/> class.
    /// </summary>
    public EnumCollectionBuilder()
    {
        _mode = CollectionGenerationMode.StaticCollection;
    }

    /// <inheritdoc/>
    public IEnumCollectionBuilder Configure(CollectionGenerationMode mode)
    {
        if (!Enum.IsDefined(typeof(CollectionGenerationMode), mode))
        {
            throw new ArgumentException($"Invalid generation mode: {mode}", nameof(mode));
        }

        _mode = mode;
        return this;
    }

    /// <inheritdoc/>
    public IEnumCollectionBuilder WithDefinition(EnumTypeInfo definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        return this;
    }

    /// <inheritdoc/>
    public IEnumCollectionBuilder WithValues(IList<EnumValueInfo> values)
    {
        _values = values ?? throw new ArgumentNullException(nameof(values));
        return this;
    }

    /// <inheritdoc/>
    public IEnumCollectionBuilder WithReturnType(string returnType)
    {
        if (string.IsNullOrEmpty(returnType))
        {
            throw new ArgumentException("Return type cannot be null or empty.", nameof(returnType));
        }

        _returnType = returnType;
        return this;
    }

    /// <inheritdoc/>
    public IEnumCollectionBuilder WithCompilation(Compilation compilation)
    {
        _compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
        return this;
    }

    /// <inheritdoc/>
    public string Build()
    {
        ValidateConfiguration();

        _classBuilder = new ClassBuilder();
        
        BuildUsings();
        
        // Add #nullable enable directive after usings
        var result = BuildCore();
        
        // Insert #nullable enable after the using statements
        var lines = result.Split('\n').ToList();
        var lastUsingIndex = lines.FindLastIndex(l => l.TrimStart().StartsWith("using ", StringComparison.Ordinal));
        if (lastUsingIndex >= 0)
        {
            lines.Insert(lastUsingIndex + 1, "");
            lines.Insert(lastUsingIndex + 2, "#nullable enable");
        }
        
        return string.Join("\n", lines);
    }
    
    private string BuildCore()
    {
        BuildNamespace();
        BuildClass();
        AddCommonElements();
        
        return _mode switch
        {
            CollectionGenerationMode.StaticCollection => BuildDefaultCollection(),
            CollectionGenerationMode.InstanceCollection => BuildInstanceCollection(),
            CollectionGenerationMode.FactoryCollection => BuildFactoryCollection(),
            CollectionGenerationMode.ServiceCollection => BuildServiceCollection(),
            _ => throw new InvalidOperationException($"Unsupported generation mode: {_mode}")
        };
    }

    /// <summary>
    /// Validates that all required configuration has been provided.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing.</exception>
    private void ValidateConfiguration()
    {
        if (_definition == null)
        {
            throw new InvalidOperationException("Enum type definition must be provided using WithDefinition().");
        }

        if (_values == null)
        {
            throw new InvalidOperationException("Enum values must be provided using WithValues().");
        }

        if (string.IsNullOrEmpty(_returnType))
        {
            throw new InvalidOperationException("Return type must be provided using WithReturnType().");
        }

        if (_compilation == null)
        {
            throw new InvalidOperationException("Compilation context must be provided using WithCompilation().");
        }
    }

    /// <summary>
    /// Builds the using statements for the generated class.
    /// </summary>
    private void BuildUsings()
    {
        var usings = new List<string>
        {
            "System",
            "System.Collections.Generic",
            "System.Collections.Immutable",
            "System.Linq"
        };

        // Add EnhancedEnums namespace if inheriting from base
        if (_definition!.InheritsFromCollectionBase)
        {
            usings.Add("FractalDataWorks.EnhancedEnums");
        }

        // Add namespaces for all enum value types so constructors can find the classes
        foreach (var value in _values!.Where(v => v.Include))
        {
            if (!string.IsNullOrEmpty(value.ReturnTypeNamespace) && 
                !string.Equals(value.ReturnTypeNamespace, _definition!.Namespace, StringComparison.Ordinal))
            {
                usings.Add(value.ReturnTypeNamespace!);
            }
        }

        // Add additional namespaces based on return type and requirements
        if (!string.IsNullOrEmpty(_definition!.ReturnTypeNamespace) && 
            !string.Equals(_definition.ReturnTypeNamespace, _definition.Namespace, StringComparison.Ordinal))
        {
            usings.Add(_definition.ReturnTypeNamespace!);
        }

        foreach (var ns in _definition.RequiredNamespaces)
        {
            if (!string.Equals(ns, _definition.Namespace, StringComparison.Ordinal))
            {
                usings.Add(ns);
            }
        }

        _classBuilder!.WithUsings(usings.Distinct(StringComparer.Ordinal).ToArray());
    }

    /// <summary>
    /// Builds the namespace declaration.
    /// </summary>
    private void BuildNamespace()
    {
        _classBuilder!.WithNamespace(_definition!.Namespace);
    }

    /// <summary>
    /// Builds the class structure and contents.
    /// </summary>
    private void BuildClass()
    {
        _classBuilder!.WithName(_definition!.CollectionName)
                      .WithXmlDoc($"Provides a collection of {_definition.ClassName} enum values.")
                      .AsAbstract();

        // No inheritance - create self-contained abstract class
        // We'll copy all members from EnumCollectionBase and the decorated class
    }

    /// <summary>
    /// Adds common elements shared across all generation modes.
    /// </summary>
    private void AddCommonElements()
    {
        // Add static fields for enum values
        foreach (var value in _values!.Where(v => v.Include))
        {
            var fieldBuilder = new FieldBuilder()
                .WithName(value.Name)
                .WithType(_returnType!)
                .WithAccessModifier("public")
                .AsStatic()
                .AsReadOnly()
                .WithInitializer(GenerateValueInitializer(value))
                .WithXmlDoc($"Gets the {value.Name} enum value.");

            _classBuilder!.WithField(fieldBuilder);
        }
    }

    /// <summary>
    /// Generates the initializer expression for an enum value field.
    /// </summary>
    /// <param name="value">The enum value to generate an initializer for.</param>
    /// <returns>The initializer expression as a string.</returns>
    private static string GenerateValueInitializer(EnumValueInfo value)
    {
        if (value.Constructors.Count > 0)
        {
            var constructor = value.Constructors.FirstOrDefault(c => c.Parameters.Count == 0) ?? value.Constructors.First();
            var parameters = string.Join(", ", constructor.Parameters.Select(p => p.DefaultValue ?? "default"));
            return $"new {value.ShortTypeName}({parameters})";
        }

        return $"new {value.ShortTypeName}()";
    }

    /// <summary>
    /// Builds default collection as self-contained abstract class.
    /// </summary>
    /// <returns>The generated source code.</returns>
    private string BuildDefaultCollection()
    {
        // Add all EnumCollectionBase fields (protected static)
        AddStaticFields();
        
#pragma warning disable MA0026 // TODO
        // TODO: Add compiler directives for FrozenDictionary support on .NET 8+
#pragma warning restore MA0026
        // #if NET8_0_OR_GREATER use FrozenDictionary, else use Dictionary
        // This requires adding preprocessor directive support to FieldBuilder
        
        // Add dictionary fields for all lookup properties
        AddLookupDictionaries();
        
        // Add static constructor
        AddStaticConstructor();
        
        // Add all EnumCollectionBase static methods with concrete type
        BuildAllMethod(true);
        AddEmptyMethod();
        AddAsEnumerableMethod();
        BuildCountProperty(true);
        AddAnyMethod();
        AddGetByIndexMethod();
        
        // Add lookup methods from attributes (including Name and Id from EnumOptionBase)
        // These will use the dictionaries for O(1) lookups
        BuildLookupMethods(true);
        
        return _classBuilder!.Build();
    }

    /// <summary>
    /// Builds collection for instance generation mode (singleton pattern).
    /// </summary>
    /// <returns>The generated source code.</returns>
    private string BuildInstanceCollection()
    {
        // Add singleton instance field
        var instanceField = new FieldBuilder()
            .WithName("Instance")
            .WithType(_definition!.CollectionName)
            .WithAccessModifier("public")
            .AsStatic()
            .AsReadOnly()
            .WithInitializer($"new {_definition.CollectionName}()")
            .WithXmlDoc("Gets the singleton instance of the collection.");
        
        _classBuilder!.WithField(instanceField);

        // Add private constructor
        var constructor = new ConstructorBuilder()
            .WithClassName(_definition.CollectionName)
            .WithAccessModifier("private")
            .WithXmlDoc("Initializes a new instance of the collection (private to enforce singleton pattern).");
        
        _classBuilder.WithConstructor(constructor);

        BuildAllMethod(false);
        BuildCountProperty(false);
        BuildLookupMethods(false);
        
        return _classBuilder.Build();
    }

    /// <summary>
    /// Builds collection for factory generation mode.
    /// </summary>
    /// <returns>The generated source code.</returns>
    private string BuildFactoryCollection()
    {
        BuildAllMethod(true);
        BuildCountProperty(true);
        BuildLookupMethods(true);
        
        // Add factory methods for individual values
        foreach (var value in _values!.Where(v => v.Include))
        {
            var factoryMethod = new MethodBuilder()
                .WithName($"Create{value.Name}")
                .WithReturnType(_returnType!)
                .WithAccessModifier("public")
                .AsStatic()
                .WithXmlDoc($"Creates a new instance of the {value.Name} enum value.")
                .WithReturnDoc($"A new instance of the {value.Name} enum value.")
                .WithExpressionBody(GenerateValueInitializer(value));
            
            _classBuilder!.WithMethod(factoryMethod);
        }
        
        return _classBuilder!.Build();
    }

    /// <summary>
    /// Builds collection for service generation mode (dependency injection).
    /// </summary>
    /// <returns>The generated source code.</returns>
    private string BuildServiceCollection()
    {
        // Add public constructor for DI
        var constructor = new ConstructorBuilder()
            .WithClassName(_definition!.CollectionName)
            .WithAccessModifier("public")
            .WithXmlDoc("Initializes a new instance of the service collection.");
        
        _classBuilder!.WithConstructor(constructor);

        BuildAllMethod(false);
        BuildCountProperty(false);
        BuildLookupMethods(false);
        
        return _classBuilder.Build();
    }

    /// <summary>
    /// Builds the All() method that returns all enum values.
    /// </summary>
    /// <param name="isStatic">Whether the method should be static.</param>
    private void BuildAllMethod(bool isStatic)
    {
        var returnTypeForCollection = $"ImmutableArray<{_returnType}>";
        string methodBody;

        if (_definition!.UseSingletonInstances)
        {
            // Singleton mode: return cached instances
            methodBody = "_all";
        }
        else
        {
            // Factory mode: create new instances each time
            var factoryCalls = string.Join(", ", _values!.Where(v => v.Include).Select(v => $"new {v.ShortTypeName}()"));
            methodBody = $"ImmutableArray.Create<{_returnType}>({factoryCalls})";
        }

        var methodBuilder = new MethodBuilder()
            .WithName("All")
            .WithReturnType(returnTypeForCollection)
            .WithAccessModifier("public")
            .WithXmlDoc("Gets all enum values in the collection.")
            .WithReturnDoc("A read-only list containing all enum values.");

        methodBuilder.WithExpressionBody(methodBody);

        if (isStatic)
        {
            methodBuilder.AsStatic();
        }

        _classBuilder!.WithMethod(methodBuilder);
    }

    /// <summary>
    /// Builds the Count property that returns the total number of enum values.
    /// </summary>
    /// <param name="isStatic">Whether the property should be static.</param>
    private void BuildCountProperty(bool isStatic)
    {
        var propertyBuilder = new PropertyBuilder()
            .WithName("Count")
            .WithType("int")
            .WithAccessModifier("public")
            .AsReadOnly()
            .WithXmlDoc("Gets the total number of enum values in the collection.")
            .WithExpressionBody("_all.Length");

        if (isStatic)
        {
            propertyBuilder.AsStatic();
        }

        _classBuilder!.WithProperty(propertyBuilder);
    }

    /// <summary>
    /// Builds lookup methods for properties defined in the enum type definition.
    /// </summary>
    /// <param name="isStatic">Whether the methods should be static.</param>
    private void BuildLookupMethods(bool isStatic)
    {
        foreach (var lookup in _definition!.LookupProperties)
        {
            BuildByPropertyMethod(lookup, isStatic);
            
            if (lookup.GenerateTryGet)
            {
                BuildTryGetByPropertyMethod(lookup, isStatic);
            }
        }

        // Don't add default ByName methods - they come from EnumLookup attributes on EnumOptionBase
    }

    /// <summary>
    /// Builds a lookup method for a specific property.
    /// </summary>
    /// <param name="lookup">The property lookup information.</param>
    /// <param name="isStatic">Whether the method should be static.</param>
#pragma warning disable MA0051 // Method is too long
    private void BuildByPropertyMethod(PropertyLookupInfo lookup, bool isStatic)
    {
        // GetByName and GetById should return nullable, others might not
        var isNullableReturn = string.Equals(lookup.LookupMethodName, "GetByName", StringComparison.Ordinal) || 
                              string.Equals(lookup.LookupMethodName, "GetById", StringComparison.Ordinal);
        var returnType = lookup.AllowMultiple ? $"IEnumerable<{_returnType}>" : 
                        (isNullableReturn ? $"{_returnType}?" : _returnType!);
        
        var fieldName = $"_by{lookup.PropertyName}";
        string methodBody;
        
        if (_definition!.UseSingletonInstances)
        {
            // Use dictionaries for O(1) lookup
            if (lookup.AllowMultiple)
            {
                methodBody = $"return {fieldName}[value];";
            }
            else
            {
                if (isNullableReturn)
                {
                    methodBody = $"return {fieldName}.TryGetValue(value, out var result) ? result : null;";
                }
                else
                {
                    methodBody = $"return {fieldName}.TryGetValue(value, out var result) ? result : throw new ArgumentException($\"No enum value found with {lookup.PropertyName}: {{value}}\");";
                }
            }
        }
        else
        {
            // Factory mode - linear search with new instances
            var comparisonLogic = GenerateComparisonLogic(lookup);
            if (lookup.AllowMultiple)
            {
                methodBody = $"return All().Where(x => {comparisonLogic});";
            }
            else
            {
                if (isNullableReturn)
                {
                    methodBody = $"return All().FirstOrDefault(x => {comparisonLogic});";
                }
                else
                {
                    methodBody = $"return All().FirstOrDefault(x => {comparisonLogic}) ?? throw new ArgumentException($\"No enum value found with {lookup.PropertyName}: {{value}}\");";
                }
            }
        }

        var methodBuilder = new MethodBuilder()
            .WithName(lookup.LookupMethodName)
            .WithReturnType(returnType)
            .WithAccessModifier("public")
            .WithParameter(lookup.PropertyType, "value")
            .WithXmlDoc($"Gets enum value(s) by {lookup.PropertyName}.")
            .WithParamDoc("value", $"The {lookup.PropertyName} value to search for.")
            .WithReturnDoc($"{(lookup.AllowMultiple ? "All enum values" : "The enum value")} with the specified {lookup.PropertyName}.")
            .WithBody(methodBody);

        if (isStatic)
        {
            methodBuilder.AsStatic();
        }

        _classBuilder!.WithMethod(methodBuilder);
    }
#pragma warning restore MA0051 // Method is too long

    /// <summary>
    /// Builds a TryGet lookup method for a specific property.
    /// </summary>
    /// <param name="lookup">The property lookup information.</param>
    /// <param name="isStatic">Whether the method should be static.</param>
    private void BuildTryGetByPropertyMethod(PropertyLookupInfo lookup, bool isStatic)
    {
        var comparisonLogic = GenerateComparisonLogic(lookup);
        var methodName = $"TryGet{lookup.LookupMethodName.Substring(2)}";
        
        var methodBody = $"result = All().FirstOrDefault(x => {comparisonLogic});\nreturn result != null;";

        var methodBuilder = new MethodBuilder()
            .WithName(methodName)
            .WithReturnType("bool")
            .WithAccessModifier("public")
            .WithParameter(lookup.PropertyType, "value")
            .WithParameter($"out {_returnType}?", "result")
            .WithXmlDoc($"Attempts to find an enum value by {lookup.PropertyName}.")
            .WithParamDoc("value", $"The {lookup.PropertyName} value to search for.")
            .WithParamDoc("result", "When this method returns, contains the enum value if found; otherwise, null.")
            .WithReturnDoc("true if an enum value was found; otherwise, false.")
            .WithBody(methodBody);

        if (isStatic)
        {
            methodBuilder.AsStatic();
        }

        _classBuilder!.WithMethod(methodBuilder);
    }

    /// <summary>
    /// Builds the default ByName lookup method.
    /// </summary>
    /// <param name="isStatic">Whether the method should be static.</param>
    private void BuildByNameMethod(bool isStatic)
    {
        var comparison = _definition!.NameComparison == StringComparison.Ordinal ? 
            "string.Equals(x.Name, name, StringComparison.Ordinal)" :
            "string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)";

        var methodBody = $"return All().FirstOrDefault(x => {comparison}) ?? throw new ArgumentException($\"No enum value found with name: {{name}}\");";

        var methodBuilder = new MethodBuilder()
            .WithName("ByName")
            .WithReturnType(_returnType!)
            .WithAccessModifier("public")
            .WithParameter("string", "name")
            .WithXmlDoc("Gets an enum value by its name.")
            .WithParamDoc("name", "The name of the enum value to find.")
            .WithReturnDoc("The enum value with the specified name.")
            .WithBody(methodBody);

        if (isStatic)
        {
            methodBuilder.AsStatic();
        }

        _classBuilder!.WithMethod(methodBuilder);
    }

    /// <summary>
    /// Builds the default TryGetByName lookup method.
    /// </summary>
    /// <param name="isStatic">Whether the method should be static.</param>
    private void BuildTryGetByNameMethod(bool isStatic)
    {
        var comparison = _definition!.NameComparison == StringComparison.Ordinal ? 
            "string.Equals(x.Name, name, StringComparison.Ordinal)" :
            "string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)";

        var methodBody = $"result = All().FirstOrDefault(x => {comparison});\nreturn result != null;";

        var methodBuilder = new MethodBuilder()
            .WithName("TryGetByName")
            .WithReturnType("bool")
            .WithAccessModifier("public")
            .WithParameter("string", "name")
            .WithParameter($"out {_returnType}?", "result")
            .WithXmlDoc("Attempts to find an enum value by its name.")
            .WithParamDoc("name", "The name of the enum value to find.")
            .WithParamDoc("result", "When this method returns, contains the enum value if found; otherwise, null.")
            .WithReturnDoc("true if an enum value was found; otherwise, false.")
            .WithBody(methodBody);

        if (isStatic)
        {
            methodBuilder.AsStatic();
        }

        _classBuilder!.WithMethod(methodBuilder);
    }

    /// <summary>
    /// Generates comparison logic for property lookups.
    /// </summary>
    /// <param name="lookup">The property lookup information.</param>
    /// <returns>The comparison logic as a string.</returns>
    private static string GenerateComparisonLogic(PropertyLookupInfo lookup)
    {
        if (string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal))
        {
            return lookup.StringComparison == StringComparison.Ordinal
                ? $"string.Equals(x.{lookup.PropertyName}, value, StringComparison.Ordinal)"
                : $"string.Equals(x.{lookup.PropertyName}, value, StringComparison.OrdinalIgnoreCase)";
        }

        if (!string.IsNullOrEmpty(lookup.Comparer))
        {
            return $"{lookup.Comparer}.Equals(x.{lookup.PropertyName}, value)";
        }

        return $"EqualityComparer<{lookup.PropertyType}>.Default.Equals(x.{lookup.PropertyName}, value)";
    }
    
    /// <summary>
    /// Adds lookup dictionary fields for all properties with EnumLookup attribute.
    /// </summary>
    private void AddLookupDictionaries()
    {
        foreach (var lookup in _definition!.LookupProperties)
        {
            string fieldName = $"_by{lookup.PropertyName}";
            string fieldType;
            
            if (lookup.AllowMultiple)
            {
                // For multiple values, use ILookup
                fieldType = $"ILookup<{lookup.PropertyType}, {_returnType}>";
            }
            else
            {
                // For single values, use IReadOnlyDictionary
#pragma warning disable MA0026 // TODO
                // TODO: When we add preprocessor support, use FrozenDictionary for .NET 8+
#pragma warning restore MA0026
                fieldType = $"IReadOnlyDictionary<{lookup.PropertyType}, {_returnType}>";
            }
            
            var dictionaryField = new FieldBuilder()
                .WithName(fieldName)
                .WithType(fieldType)
                .WithAccessModifier("private")
                .AsStatic()
                .AsReadOnly()
                .WithXmlDoc($"Lookup dictionary for {lookup.PropertyName}-based searches.");
            
            _classBuilder!.WithField(dictionaryField);
        }
    }
    
    /// <summary>
    /// Adds static fields (_all and _empty).
    /// </summary>
    private void AddStaticFields()
    {
        if (_definition!.UseSingletonInstances)
        {
            // Singleton mode: store instances
            var allField = new FieldBuilder()
                .WithName("_all")
                .WithType($"ImmutableArray<{_returnType}>")
                .WithAccessModifier("protected")
                .AsStatic()
                .WithInitializer("ImmutableArray<" + _returnType + ">.Empty")
                .WithXmlDoc("Static collection of all enum options.");
            
            _classBuilder!.WithField(allField);
        }
        else
        {
            // Factory mode: store factory functions
            var factoriesField = new FieldBuilder()
                .WithName("_factories")
                .WithType($"ImmutableArray<Func<{_returnType}>>")
                .WithAccessModifier("private")
                .AsStatic()
                .WithInitializer("ImmutableArray<Func<" + _returnType + ">>.Empty")
                .WithXmlDoc("Factory functions for creating enum options.");
            
            _classBuilder!.WithField(factoriesField);
        }
        
        // Add _empty field (always needed)
        var emptyField = new FieldBuilder()
            .WithName("_empty")
            .WithType(_returnType!)
            .WithAccessModifier("protected")
            .AsStatic()
            .WithInitializer("default!")
            .WithXmlDoc("Static empty instance.");
        
        _classBuilder!.WithField(emptyField);
    }
    
    /// <summary>
    /// Adds the static constructor to initialize the collection.
    /// </summary>
#pragma warning disable MA0051 // Method is too long
    private void AddStaticConstructor()
    {
        var constructorBody = new StringBuilder();
        
        if (_definition!.UseSingletonInstances)
        {
            // Singleton mode: initialize instances and dictionaries
            constructorBody.AppendLine($"var values = new {_returnType}[]");
            constructorBody.AppendLine("{");
            
            foreach (var value in _values!.Where(v => v.Include))
            {
                constructorBody.AppendLine($"    {value.Name},");
            }
            
            constructorBody.AppendLine("};");
            constructorBody.AppendLine("_all = values.ToImmutableArray();");
            
            // Initialize lookup dictionaries
            foreach (var lookup in _definition.LookupProperties)
            {
                var fieldName = $"_by{lookup.PropertyName}";
                
                if (lookup.AllowMultiple)
                {
                    // Use ToLookup for multiple values
                    constructorBody.AppendLine($"{fieldName} = values.ToLookup(x => x.{lookup.PropertyName});");
                }
                else
                {
                    // Use ToDictionary for single values
                    constructorBody.AppendLine($"{fieldName} = values.ToDictionary(x => x.{lookup.PropertyName});");
                }
            }
        }
        else
        {
            // Factory mode: initialize factory functions
            constructorBody.AppendLine($"var factories = new Func<{_returnType}>[]");
            constructorBody.AppendLine("{");
            
            foreach (var value in _values!.Where(v => v.Include))
            {
                constructorBody.AppendLine($"    () => new {value.ShortTypeName}(),");
            }
            
            constructorBody.AppendLine("};");
            constructorBody.AppendLine("_factories = factories.ToImmutableArray();");
            
            // For factory mode, we still need dictionaries but they store factories
#pragma warning disable MA0026 // TODO
            // TODO: Implement factory-based dictionaries
#pragma warning restore MA0026
        }
        
        // Initialize empty instance
        var emptyValue = _values!.FirstOrDefault(v => v.Name.Contains("Empty", StringComparison.OrdinalIgnoreCase));
        if (emptyValue != null)
        {
            constructorBody.AppendLine($"_empty = {emptyValue.Name};");
        }
        else
        {
            constructorBody.AppendLine("_empty = default!;");
        }
        
        var constructor = new ConstructorBuilder()
            .WithClassName(_definition!.CollectionName)
            .AsStatic()
            .WithBody(constructorBody.ToString());
        
        _classBuilder!.WithConstructor(constructor);
    }
#pragma warning restore MA0051 // Method is too long
    
    /// <summary>
    /// Adds a static constructor for classes that inherit from EnumCollectionBase.
    /// </summary>
    private void AddStaticConstructorForInheritance()
    {
        var constructorBody = new StringBuilder();
        constructorBody.AppendLine($"var values = new {_returnType}[]");
        constructorBody.AppendLine("{");
        
        foreach (var value in _values!.Where(v => v.Include))
        {
            constructorBody.AppendLine($"    {value.Name},");
        }
        
        constructorBody.AppendLine("};");
        constructorBody.AppendLine("_all = values.ToImmutableArray();");
        
        // Check if there's an empty value defined
        var emptyValue = _values.FirstOrDefault(v => v.Name.Contains("Empty", StringComparison.OrdinalIgnoreCase));
        if (emptyValue != null)
        {
            constructorBody.AppendLine($"_empty = {emptyValue.Name};");
        }
        else
        {
            constructorBody.AppendLine($"_empty = default!;");
        }
        
        var constructor = new ConstructorBuilder()
            .WithClassName(_definition!.CollectionName)
            .AsStatic()
            .WithBody(constructorBody.ToString());
        
        _classBuilder!.WithConstructor(constructor);
    }
    
    /// <summary>
    /// Adds the Empty() method.
    /// </summary>
    private void AddEmptyMethod()
    {
        // Check if there's an empty value in the collection
        var emptyValue = _values?.FirstOrDefault(v => v.Name.Contains("Empty", StringComparison.OrdinalIgnoreCase));
        var emptyExpression = emptyValue != null ? emptyValue.Name : "default!";
        
        var method = new MethodBuilder()
            .WithName("Empty")
            .WithReturnType(_returnType!)
            .WithAccessModifier("public")
            .AsStatic()
            .WithXmlDoc("Gets an empty instance of the enum option type.")
            .WithExpressionBody(emptyExpression);
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the GetByName() method.
    /// </summary>
    private void AddGetByNameMethod()
    {
        var method = new MethodBuilder()
            .WithName("GetByName")
            .WithReturnType($"{_returnType}?")
            .WithAccessModifier("public")
            .AsStatic()
            .WithParameter("string", "name")
            .WithXmlDoc("Gets an enum option by name (case-insensitive).")
            .WithBody(@"if (string.IsNullOrWhiteSpace(name)) return null;
return All().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));");
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the GetById() method.
    /// </summary>
    private void AddGetByIdMethod()
    {
        var method = new MethodBuilder()
            .WithName("GetById")
            .WithReturnType($"{_returnType}?")
            .WithAccessModifier("public")
            .AsStatic()
            .WithParameter("int", "id")
            .WithXmlDoc("Gets an enum option by ID.")
            .WithBody("return All().FirstOrDefault(x => x.Id == id);");
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the TryGetByName() method.
    /// </summary>
    private void AddTryGetByNameMethod()
    {
        var method = new MethodBuilder()
            .WithName("TryGetByName")
            .WithReturnType("bool")
            .WithAccessModifier("public")
            .AsStatic()
            .WithParameter("string", "name")
            .WithParameter($"out {_returnType}?", "value")
            .WithXmlDoc("Tries to get an enum option by name.")
            .WithBody(@"value = GetByName(name);
return value != null;");
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the TryGetById() method.
    /// </summary>
    private void AddTryGetByIdMethod()
    {
        var method = new MethodBuilder()
            .WithName("TryGetById")
            .WithReturnType("bool")
            .WithAccessModifier("public")
            .AsStatic()
            .WithParameter("int", "id")
            .WithParameter($"out {_returnType}?", "value")
            .WithXmlDoc("Tries to get an enum option by ID.")
            .WithBody(@"value = GetById(id);
return value != null;");
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the AsEnumerable() method.
    /// </summary>
    private void AddAsEnumerableMethod()
    {
        var method = new MethodBuilder()
            .WithName("AsEnumerable")
            .WithReturnType($"IEnumerable<{_returnType}>")
            .WithAccessModifier("public")
            .AsStatic()
            .WithXmlDoc("Gets all enum options as an enumerable.")
            .WithBody("return All();");
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the Any() method.
    /// </summary>
    private void AddAnyMethod()
    {
        var method = new MethodBuilder()
            .WithName("Any")
            .WithReturnType("bool")
            .WithAccessModifier("public")
            .AsStatic()
            .WithXmlDoc("Checks if the collection contains any items.")
            .WithBody("return _all.Length > 0;");
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the GetByIndex() method.
    /// </summary>
    private void AddGetByIndexMethod()
    {
        var method = new MethodBuilder()
            .WithName("GetByIndex")
            .WithReturnType(_returnType!)
            .WithAccessModifier("public")
            .AsStatic()
            .WithParameter("int", "index")
            .WithXmlDoc("Gets an enum option by index.")
            .WithBody("return _all[index];");
        
        _classBuilder!.WithMethod(method);
    }
}