using System;
using System.Collections.Generic;
using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.CodeBuilder.Abstractions;

/// <summary>
/// Main interface for fluent code building operations.
/// Provides language-agnostic methods for constructing code.
/// </summary>
public interface ICodeBuilder
{
    /// <summary>
    /// Gets the language this builder supports.
    /// </summary>
    string Language { get; }

    /// <summary>
    /// Creates a new class builder.
    /// </summary>
    /// <param name="name">The name of the class.</param>
    /// <returns>A class builder instance.</returns>
    IClassBuilder CreateClass(string name);

    /// <summary>
    /// Creates a new interface builder.
    /// </summary>
    /// <param name="name">The name of the interface.</param>
    /// <returns>An interface builder instance.</returns>
    IInterfaceBuilder CreateInterface(string name);

    /// <summary>
    /// Creates a new method builder.
    /// </summary>
    /// <param name="name">The name of the method.</param>
    /// <returns>A method builder instance.</returns>
    IMethodBuilder CreateMethod(string name);

    /// <summary>
    /// Creates a new property builder.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="type">The type of the property.</param>
    /// <returns>A property builder instance.</returns>
    IPropertyBuilder CreateProperty(string name, string type);

    /// <summary>
    /// Creates a new field builder.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <param name="type">The type of the field.</param>
    /// <returns>A field builder instance.</returns>
    IFieldBuilder CreateField(string name, string type);

    /// <summary>
    /// Creates a new parameter builder.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="type">The type of the parameter.</param>
    /// <returns>A parameter builder instance.</returns>
    IParameterBuilder CreateParameter(string name, string type);

    /// <summary>
    /// Creates a new namespace builder.
    /// </summary>
    /// <param name="name">The name of the namespace.</param>
    /// <returns>A namespace builder instance.</returns>
    INamespaceBuilder CreateNamespace(string name);

    /// <summary>
    /// Creates a new compilation unit builder (represents a complete source file).
    /// </summary>
    /// <returns>A compilation unit builder instance.</returns>
    ICompilationUnitBuilder CreateCompilationUnit();
}

/// <summary>
/// Base interface for all type builders (classes, interfaces, structs, etc.).
/// </summary>
public interface ITypeBuilder<out TBuilder, out TDefinition> : IAstBuilder<TDefinition>
    where TBuilder : ITypeBuilder<TBuilder, TDefinition>
    where TDefinition : class, IAstNode
{
    /// <summary>
    /// Sets the access modifier for this type.
    /// </summary>
    /// <param name="access">The access modifier.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder WithAccess(AccessModifier access);

    /// <summary>
    /// Adds a method to this type.
    /// </summary>
    /// <param name="configure">Configuration action for the method.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder AddMethod(Action<IMethodBuilder> configure);

    /// <summary>
    /// Adds a property to this type.
    /// </summary>
    /// <param name="configure">Configuration action for the property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder AddProperty(Action<IPropertyBuilder> configure);

    /// <summary>
    /// Adds a field to this type.
    /// </summary>
    /// <param name="configure">Configuration action for the field.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder AddField(Action<IFieldBuilder> configure);

    /// <summary>
    /// Adds an attribute/annotation to this type.
    /// </summary>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="arguments">Optional attribute arguments.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder AddAttribute(string name, params object[] arguments);

    /// <summary>
    /// Adds XML documentation to this type.
    /// </summary>
    /// <param name="documentation">The documentation text.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder WithDocumentation(string documentation);
}

/// <summary>
/// Builder for class definitions.
/// </summary>
public interface IClassBuilder : ITypeBuilder<IClassBuilder, IClassDefinition>
{
    /// <summary>
    /// Sets the base class for this class.
    /// </summary>
    /// <param name="baseClass">The name of the base class.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IClassBuilder WithBaseClass(string baseClass);

    /// <summary>
    /// Adds an interface implementation to this class.
    /// </summary>
    /// <param name="interfaceName">The name of the interface to implement.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IClassBuilder ImplementsInterface(string interfaceName);

    /// <summary>
    /// Makes this class abstract.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IClassBuilder AsAbstract();

    /// <summary>
    /// Makes this class sealed/final.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IClassBuilder AsSealed();

    /// <summary>
    /// Makes this class static.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IClassBuilder AsStatic();

    /// <summary>
    /// Makes this class partial.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IClassBuilder AsPartial();

    /// <summary>
    /// Adds a generic type parameter to this class.
    /// </summary>
    /// <param name="name">The name of the type parameter.</param>
    /// <param name="constraints">Optional constraints for the type parameter.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IClassBuilder AddGenericParameter(string name, params string[] constraints);
}

/// <summary>
/// Builder for interface definitions.
/// </summary>
public interface IInterfaceBuilder : ITypeBuilder<IInterfaceBuilder, IInterfaceDefinition>
{
    /// <summary>
    /// Adds a base interface to this interface.
    /// </summary>
    /// <param name="interfaceName">The name of the base interface.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IInterfaceBuilder ExtendsInterface(string interfaceName);

    /// <summary>
    /// Adds a generic type parameter to this interface.
    /// </summary>
    /// <param name="name">The name of the type parameter.</param>
    /// <param name="constraints">Optional constraints for the type parameter.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IInterfaceBuilder AddGenericParameter(string name, params string[] constraints);
}

/// <summary>
/// Builder for method definitions.
/// </summary>
public interface IMethodBuilder : IAstBuilder<IMethodDefinition>
{
    /// <summary>
    /// Sets the access modifier for this method.
    /// </summary>
    /// <param name="access">The access modifier.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IMethodBuilder WithAccess(AccessModifier access);

    /// <summary>
    /// Sets the return type for this method.
    /// </summary>
    /// <param name="returnType">The return type.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IMethodBuilder WithReturnType(string returnType);

    /// <summary>
    /// Adds a parameter to this method.
    /// </summary>
    /// <param name="configure">Configuration action for the parameter.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IMethodBuilder AddParameter(Action<IParameterBuilder> configure);

    /// <summary>
    /// Sets the method body.
    /// </summary>
    /// <param name="body">The method body code.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IMethodBuilder WithBody(string body);

    /// <summary>
    /// Makes this method abstract.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IMethodBuilder AsAbstract();

    /// <summary>
    /// Makes this method virtual.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IMethodBuilder AsVirtual();

    /// <summary>
    /// Makes this method override.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IMethodBuilder AsOverride();

    /// <summary>
    /// Makes this method static.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IMethodBuilder AsStatic();

    /// <summary>
    /// Adds an attribute to this method.
    /// </summary>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="arguments">Optional attribute arguments.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IMethodBuilder AddAttribute(string name, params object[] arguments);

    /// <summary>
    /// Adds XML documentation to this method.
    /// </summary>
    /// <param name="documentation">The documentation text.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IMethodBuilder WithDocumentation(string documentation);
}

/// <summary>
/// Builder for property definitions.
/// </summary>
public interface IPropertyBuilder : IAstBuilder<IPropertyDefinition>
{
    /// <summary>
    /// Sets the access modifier for this property.
    /// </summary>
    /// <param name="access">The access modifier.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IPropertyBuilder WithAccess(AccessModifier access);

    /// <summary>
    /// Sets the property type.
    /// </summary>
    /// <param name="type">The property type.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IPropertyBuilder WithType(string type);

    /// <summary>
    /// Adds a getter to this property.
    /// </summary>
    /// <param name="body">Optional getter body (null for auto-implemented).</param>
    /// <param name="access">Optional getter access modifier.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IPropertyBuilder WithGetter(string? body = null, AccessModifier? access = null);

    /// <summary>
    /// Adds a setter to this property.
    /// </summary>
    /// <param name="body">Optional setter body (null for auto-implemented).</param>
    /// <param name="access">Optional setter access modifier.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IPropertyBuilder WithSetter(string? body = null, AccessModifier? access = null);

    /// <summary>
    /// Makes this property static.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IPropertyBuilder AsStatic();

    /// <summary>
    /// Makes this property virtual.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IPropertyBuilder AsVirtual();

    /// <summary>
    /// Makes this property override.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IPropertyBuilder AsOverride();

    /// <summary>
    /// Adds an attribute to this property.
    /// </summary>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="arguments">Optional attribute arguments.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IPropertyBuilder AddAttribute(string name, params object[] arguments);

    /// <summary>
    /// Adds XML documentation to this property.
    /// </summary>
    /// <param name="documentation">The documentation text.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IPropertyBuilder WithDocumentation(string documentation);
}

/// <summary>
/// Builder for field definitions.
/// </summary>
public interface IFieldBuilder : IAstBuilder<IFieldDefinition>
{
    /// <summary>
    /// Sets the access modifier for this field.
    /// </summary>
    /// <param name="access">The access modifier.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IFieldBuilder WithAccess(AccessModifier access);

    /// <summary>
    /// Sets the field type.
    /// </summary>
    /// <param name="type">The field type.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IFieldBuilder WithType(string type);

    /// <summary>
    /// Sets the initial value for this field.
    /// </summary>
    /// <param name="value">The initial value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IFieldBuilder WithInitialValue(string value);

    /// <summary>
    /// Makes this field static.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IFieldBuilder AsStatic();

    /// <summary>
    /// Makes this field readonly.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IFieldBuilder AsReadOnly();

    /// <summary>
    /// Makes this field const.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IFieldBuilder AsConst();

    /// <summary>
    /// Adds an attribute to this field.
    /// </summary>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="arguments">Optional attribute arguments.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IFieldBuilder AddAttribute(string name, params object[] arguments);

    /// <summary>
    /// Adds XML documentation to this field.
    /// </summary>
    /// <param name="documentation">The documentation text.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IFieldBuilder WithDocumentation(string documentation);
}

/// <summary>
/// Builder for parameter definitions.
/// </summary>
public interface IParameterBuilder : IAstBuilder<IParameterDefinition>
{
    /// <summary>
    /// Sets the parameter type.
    /// </summary>
    /// <param name="type">The parameter type.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IParameterBuilder WithType(string type);

    /// <summary>
    /// Sets the default value for this parameter.
    /// </summary>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IParameterBuilder WithDefaultValue(string defaultValue);

    /// <summary>
    /// Makes this parameter optional.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IParameterBuilder AsOptional();

    /// <summary>
    /// Makes this parameter a reference parameter.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IParameterBuilder AsRef();

    /// <summary>
    /// Makes this parameter an output parameter.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IParameterBuilder AsOut();

    /// <summary>
    /// Makes this parameter a params array parameter.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    IParameterBuilder AsParams();

    /// <summary>
    /// Adds an attribute to this parameter.
    /// </summary>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="arguments">Optional attribute arguments.</param>
    /// <returns>The builder instance for method chaining.</returns>
    IParameterBuilder AddAttribute(string name, params object[] arguments);
}

/// <summary>
/// Builder for namespace definitions.
/// </summary>
public interface INamespaceBuilder : IAstBuilder<INamespaceDefinition>
{
    /// <summary>
    /// Adds a class to this namespace.
    /// </summary>
    /// <param name="configure">Configuration action for the class.</param>
    /// <returns>The builder instance for method chaining.</returns>
    INamespaceBuilder AddClass(Action<IClassBuilder> configure);

    /// <summary>
    /// Adds an interface to this namespace.
    /// </summary>
    /// <param name="configure">Configuration action for the interface.</param>
    /// <returns>The builder instance for method chaining.</returns>
    INamespaceBuilder AddInterface(Action<IInterfaceBuilder> configure);

    /// <summary>
    /// Adds a nested namespace to this namespace.
    /// </summary>
    /// <param name="configure">Configuration action for the nested namespace.</param>
    /// <returns>The builder instance for method chaining.</returns>
    INamespaceBuilder AddNamespace(Action<INamespaceBuilder> configure);
}

/// <summary>
/// Builder for compilation unit definitions (represents a complete source file).
/// </summary>
public interface ICompilationUnitBuilder : IAstBuilder<ICompilationUnitDefinition>
{
    /// <summary>
    /// Adds a using statement/import to this compilation unit.
    /// </summary>
    /// <param name="namespace">The namespace to import.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ICompilationUnitBuilder AddUsing(string @namespace);

    /// <summary>
    /// Adds a namespace to this compilation unit.
    /// </summary>
    /// <param name="configure">Configuration action for the namespace.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ICompilationUnitBuilder AddNamespace(Action<INamespaceBuilder> configure);

    /// <summary>
    /// Adds a top-level class to this compilation unit.
    /// </summary>
    /// <param name="configure">Configuration action for the class.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ICompilationUnitBuilder AddClass(Action<IClassBuilder> configure);

    /// <summary>
    /// Adds a top-level interface to this compilation unit.
    /// </summary>
    /// <param name="configure">Configuration action for the interface.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ICompilationUnitBuilder AddInterface(Action<IInterfaceBuilder> configure);
}

/// <summary>
/// Represents access modifiers for code elements.
/// </summary>
[EnhancedEnum]
public partial record AccessModifier
{
    /// <summary>No explicit access modifier.</summary>
    public static readonly AccessModifier None = new("None", "");
    /// <summary>Public access.</summary>
    public static readonly AccessModifier Public = new("Public", "public");
    /// <summary>Private access.</summary>
    public static readonly AccessModifier Private = new("Private", "private");
    /// <summary>Protected access.</summary>
    public static readonly AccessModifier Protected = new("Protected", "protected");
    /// <summary>Internal access (C#) / package-private (Java).</summary>
    public static readonly AccessModifier Internal = new("Internal", "internal");
    /// <summary>Protected internal access (C#).</summary>
    public static readonly AccessModifier ProtectedInternal = new("ProtectedInternal", "protected internal");
    /// <summary>Private protected access (C#).</summary>
    public static readonly AccessModifier PrivateProtected = new("PrivateProtected", "private protected");

    /// <summary>
    /// Gets the keyword representation of this access modifier.
    /// </summary>
    public string Keyword => _keyword;
    
    private readonly string _keyword;

    private AccessModifier(string name, string keyword) : base(name)
    {
        _keyword = keyword;
    }
}