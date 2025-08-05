using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.CodeBuilder.Abstractions;

/// <summary>
/// Base class for all immutable code definition products.
/// These represent the actual code constructs created by builders.
/// </summary>
public abstract record CodeDefinition : IAstNode
{
    /// <summary>
    /// Gets the unique identifier for this code definition.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the type of this AST node.
    /// </summary>
    public abstract string NodeType { get; }

    /// <summary>
    /// Gets the name of this code definition.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the parent node of this definition.
    /// </summary>
    public IAstNode? Parent { get; init; }

    /// <summary>
    /// Gets the child nodes of this definition.
    /// </summary>
    public abstract IReadOnlyList<IAstNode> Children { get; }

    /// <summary>
    /// Gets the metadata associated with this definition.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = 
        new Dictionary<string, object>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the source location information for this definition.
    /// </summary>
    public SourceLocation? Location { get; init; }

    /// <summary>
    /// Gets a specific child node by name.
    /// </summary>
    /// <param name="name">The name of the child node to retrieve.</param>
    /// <returns>The child node if found, otherwise null.</returns>
    public virtual IAstNode? GetChild(string name)
    {
        foreach (var child in Children)
        {
            if (string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets all child nodes of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of child nodes to retrieve.</typeparam>
    /// <returns>An enumerable of child nodes of the specified type.</returns>
    public virtual IEnumerable<T> GetChildren<T>() where T : class, IAstNode
    {
        foreach (var child in Children)
        {
            if (child is T typedChild)
            {
                yield return typedChild;
            }
        }
    }

    /// <summary>
    /// Accepts a visitor for traversing the AST.
    /// </summary>
    /// <typeparam name="T">The return type of the visitor.</typeparam>
    /// <param name="visitor">The visitor to accept.</param>
    /// <returns>The result of the visitor operation.</returns>
    public abstract T Accept<T>(IAstVisitor<T> visitor);
}

/// <summary>
/// Represents an immutable class definition.
/// </summary>
public sealed record ClassDefinition : CodeDefinition, IClassDefinition
{
    /// <inheritdoc/>
    public override string NodeType => "class";

    /// <summary>
    /// Gets the access modifier for this class.
    /// </summary>
    public AccessModifier Access { get; init; } = AccessModifier.None;

    /// <summary>
    /// Gets the base class name, if any.
    /// </summary>
    public string? BaseClass { get; init; }

    /// <summary>
    /// Gets the interfaces implemented by this class.
    /// </summary>
    public IReadOnlyList<string> Interfaces { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets whether this class is abstract.
    /// </summary>
    public bool IsAbstract { get; init; }

    /// <summary>
    /// Gets whether this class is sealed.
    /// </summary>
    public bool IsSealed { get; init; }

    /// <summary>
    /// Gets whether this class is static.
    /// </summary>
    public bool IsStatic { get; init; }

    /// <summary>
    /// Gets whether this class is partial.
    /// </summary>
    public bool IsPartial { get; init; }

    /// <summary>
    /// Gets the generic type parameters for this class.
    /// </summary>
    public IReadOnlyList<GenericParameterDefinition> GenericParameters { get; init; } = 
        Array.Empty<GenericParameterDefinition>();

    /// <summary>
    /// Gets the methods defined in this class.
    /// </summary>
    public IReadOnlyList<MethodDefinition> Methods { get; init; } = Array.Empty<MethodDefinition>();

    /// <summary>
    /// Gets the properties defined in this class.
    /// </summary>
    public IReadOnlyList<PropertyDefinition> Properties { get; init; } = Array.Empty<PropertyDefinition>();

    /// <summary>
    /// Gets the fields defined in this class.
    /// </summary>
    public IReadOnlyList<FieldDefinition> Fields { get; init; } = Array.Empty<FieldDefinition>();

    /// <summary>
    /// Gets the attributes applied to this class.
    /// </summary>
    public IReadOnlyList<AttributeDefinition> Attributes { get; init; } = Array.Empty<AttributeDefinition>();

    /// <summary>
    /// Gets the documentation for this class.
    /// </summary>
    public string? Documentation { get; init; }

    /// <inheritdoc/>
    public override IReadOnlyList<IAstNode> Children
    {
        get
        {
            var children = new List<IAstNode>();
            children.AddRange(Methods);
            children.AddRange(Properties);
            children.AddRange(Fields);
            return children;
        }
    }

    /// <inheritdoc/>
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Represents an immutable interface definition.
/// </summary>
public sealed record InterfaceDefinition : CodeDefinition, IInterfaceDefinition
{
    /// <inheritdoc/>
    public override string NodeType => "interface";

    /// <summary>
    /// Gets the access modifier for this interface.
    /// </summary>
    public AccessModifier Access { get; init; } = AccessModifier.None;

    /// <summary>
    /// Gets the base interfaces extended by this interface.
    /// </summary>
    public IReadOnlyList<string> BaseInterfaces { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the generic type parameters for this interface.
    /// </summary>
    public IReadOnlyList<GenericParameterDefinition> GenericParameters { get; init; } = 
        Array.Empty<GenericParameterDefinition>();

    /// <summary>
    /// Gets the methods defined in this interface.
    /// </summary>
    public IReadOnlyList<MethodDefinition> Methods { get; init; } = Array.Empty<MethodDefinition>();

    /// <summary>
    /// Gets the properties defined in this interface.
    /// </summary>
    public IReadOnlyList<PropertyDefinition> Properties { get; init; } = Array.Empty<PropertyDefinition>();

    /// <summary>
    /// Gets the attributes applied to this interface.
    /// </summary>
    public IReadOnlyList<AttributeDefinition> Attributes { get; init; } = Array.Empty<AttributeDefinition>();

    /// <summary>
    /// Gets the documentation for this interface.
    /// </summary>
    public string? Documentation { get; init; }

    /// <inheritdoc/>
    public override IReadOnlyList<IAstNode> Children
    {
        get
        {
            var children = new List<IAstNode>();
            children.AddRange(Methods);
            children.AddRange(Properties);
            return children;
        }
    }

    /// <inheritdoc/>
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Represents an immutable method definition.
/// </summary>
public sealed record MethodDefinition : CodeDefinition, IMethodDefinition
{
    /// <inheritdoc/>
    public override string NodeType => "method";

    /// <summary>
    /// Gets the access modifier for this method.
    /// </summary>
    public AccessModifier Access { get; init; } = AccessModifier.None;

    /// <summary>
    /// Gets the return type of this method.
    /// </summary>
    public string ReturnType { get; init; } = "void";

    /// <summary>
    /// Gets the parameters of this method.
    /// </summary>
    public IReadOnlyList<ParameterDefinition> Parameters { get; init; } = Array.Empty<ParameterDefinition>();

    /// <summary>
    /// Gets the body of this method.
    /// </summary>
    public string? Body { get; init; }

    /// <summary>
    /// Gets whether this method is abstract.
    /// </summary>
    public bool IsAbstract { get; init; }

    /// <summary>
    /// Gets whether this method is virtual.
    /// </summary>
    public bool IsVirtual { get; init; }

    /// <summary>
    /// Gets whether this method is an override.
    /// </summary>
    public bool IsOverride { get; init; }

    /// <summary>
    /// Gets whether this method is static.
    /// </summary>
    public bool IsStatic { get; init; }

    /// <summary>
    /// Gets the attributes applied to this method.
    /// </summary>
    public IReadOnlyList<AttributeDefinition> Attributes { get; init; } = Array.Empty<AttributeDefinition>();

    /// <summary>
    /// Gets the documentation for this method.
    /// </summary>
    public string? Documentation { get; init; }

    /// <inheritdoc/>
    public override IReadOnlyList<IAstNode> Children => Parameters.Cast<IAstNode>().ToArray();

    /// <inheritdoc/>
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Represents an immutable property definition.
/// </summary>
public sealed record PropertyDefinition : CodeDefinition, IPropertyDefinition
{
    /// <inheritdoc/>
    public override string NodeType => "property";

    /// <summary>
    /// Gets the access modifier for this property.
    /// </summary>
    public AccessModifier Access { get; init; } = AccessModifier.None;

    /// <summary>
    /// Gets the type of this property.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets the getter accessor, if any.
    /// </summary>
    public AccessorDefinition? Getter { get; init; }

    /// <summary>
    /// Gets the setter accessor, if any.
    /// </summary>
    public AccessorDefinition? Setter { get; init; }

    /// <summary>
    /// Gets whether this property is static.
    /// </summary>
    public bool IsStatic { get; init; }

    /// <summary>
    /// Gets whether this property is virtual.
    /// </summary>
    public bool IsVirtual { get; init; }

    /// <summary>
    /// Gets whether this property is an override.
    /// </summary>
    public bool IsOverride { get; init; }

    /// <summary>
    /// Gets the attributes applied to this property.
    /// </summary>
    public IReadOnlyList<AttributeDefinition> Attributes { get; init; } = Array.Empty<AttributeDefinition>();

    /// <summary>
    /// Gets the documentation for this property.
    /// </summary>
    public string? Documentation { get; init; }

    /// <inheritdoc/>
    public override IReadOnlyList<IAstNode> Children
    {
        get
        {
            var children = new List<IAstNode>();
            if (Getter != null) children.Add(Getter);
            if (Setter != null) children.Add(Setter);
            return children;
        }
    }

    /// <inheritdoc/>
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Represents an immutable field definition.
/// </summary>
public sealed record FieldDefinition : CodeDefinition, IFieldDefinition
{
    /// <inheritdoc/>
    public override string NodeType => "field";

    /// <summary>
    /// Gets the access modifier for this field.
    /// </summary>
    public AccessModifier Access { get; init; } = AccessModifier.None;

    /// <summary>
    /// Gets the type of this field.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets the initial value for this field.
    /// </summary>
    public string? InitialValue { get; init; }

    /// <summary>
    /// Gets whether this field is static.
    /// </summary>
    public bool IsStatic { get; init; }

    /// <summary>
    /// Gets whether this field is readonly.
    /// </summary>
    public bool IsReadOnly { get; init; }

    /// <summary>
    /// Gets whether this field is const.
    /// </summary>
    public bool IsConst { get; init; }

    /// <summary>
    /// Gets the attributes applied to this field.
    /// </summary>
    public IReadOnlyList<AttributeDefinition> Attributes { get; init; } = Array.Empty<AttributeDefinition>();

    /// <summary>
    /// Gets the documentation for this field.
    /// </summary>
    public string? Documentation { get; init; }

    /// <inheritdoc/>
    public override IReadOnlyList<IAstNode> Children => Array.Empty<IAstNode>();

    /// <inheritdoc/>
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Represents an immutable parameter definition.
/// </summary>
public sealed record ParameterDefinition : CodeDefinition, IParameterDefinition
{
    /// <inheritdoc/>
    public override string NodeType => "parameter";

    /// <summary>
    /// Gets the type of this parameter.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets the default value for this parameter.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Gets whether this parameter is optional.
    /// </summary>
    public bool IsOptional { get; init; }

    /// <summary>
    /// Gets whether this parameter is a reference parameter.
    /// </summary>
    public bool IsRef { get; init; }

    /// <summary>
    /// Gets whether this parameter is an output parameter.
    /// </summary>
    public bool IsOut { get; init; }

    /// <summary>
    /// Gets whether this parameter is a params array parameter.
    /// </summary>
    public bool IsParams { get; init; }

    /// <summary>
    /// Gets the attributes applied to this parameter.
    /// </summary>
    public IReadOnlyList<AttributeDefinition> Attributes { get; init; } = Array.Empty<AttributeDefinition>();

    /// <inheritdoc/>
    public override IReadOnlyList<IAstNode> Children => Array.Empty<IAstNode>();

    /// <inheritdoc/>
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Represents an immutable namespace definition.
/// </summary>
public sealed record NamespaceDefinition : CodeDefinition, INamespaceDefinition
{
    /// <inheritdoc/>
    public override string NodeType => "namespace";

    /// <summary>
    /// Gets the classes defined in this namespace.
    /// </summary>
    public IReadOnlyList<ClassDefinition> Classes { get; init; } = Array.Empty<ClassDefinition>();

    /// <summary>
    /// Gets the interfaces defined in this namespace.
    /// </summary>
    public IReadOnlyList<InterfaceDefinition> Interfaces { get; init; } = Array.Empty<InterfaceDefinition>();

    /// <summary>
    /// Gets the nested namespaces defined in this namespace.
    /// </summary>
    public IReadOnlyList<NamespaceDefinition> Namespaces { get; init; } = Array.Empty<NamespaceDefinition>();

    /// <inheritdoc/>
    public override IReadOnlyList<IAstNode> Children
    {
        get
        {
            var children = new List<IAstNode>();
            children.AddRange(Classes);
            children.AddRange(Interfaces);
            children.AddRange(Namespaces);
            return children;
        }
    }

    /// <inheritdoc/>
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Represents an immutable compilation unit definition.
/// </summary>
public sealed record CompilationUnitDefinition : CodeDefinition, ICompilationUnitDefinition
{
    /// <inheritdoc/>
    public override string NodeType => "compilation_unit";

    /// <summary>
    /// Gets the using statements/imports in this compilation unit.
    /// </summary>
    public IReadOnlyList<string> Usings { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the namespaces defined in this compilation unit.
    /// </summary>
    public IReadOnlyList<NamespaceDefinition> Namespaces { get; init; } = Array.Empty<NamespaceDefinition>();

    /// <summary>
    /// Gets the top-level classes defined in this compilation unit.
    /// </summary>
    public IReadOnlyList<ClassDefinition> Classes { get; init; } = Array.Empty<ClassDefinition>();

    /// <summary>
    /// Gets the top-level interfaces defined in this compilation unit.
    /// </summary>
    public IReadOnlyList<InterfaceDefinition> Interfaces { get; init; } = Array.Empty<InterfaceDefinition>();

    /// <inheritdoc/>
    public override IReadOnlyList<IAstNode> Children
    {
        get
        {
            var children = new List<IAstNode>();
            children.AddRange(Namespaces);
            children.AddRange(Classes);
            children.AddRange(Interfaces);
            return children;
        }
    }

    /// <inheritdoc/>
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Represents an immutable accessor definition (getter/setter).
/// </summary>
public sealed record AccessorDefinition : CodeDefinition
{
    /// <inheritdoc/>
    public override string NodeType => "accessor";

    /// <summary>
    /// Gets the access modifier for this accessor.
    /// </summary>
    public AccessModifier? Access { get; init; }

    /// <summary>
    /// Gets the body of this accessor (null for auto-implemented).
    /// </summary>
    public string? Body { get; init; }

    /// <inheritdoc/>
    public override IReadOnlyList<IAstNode> Children => Array.Empty<IAstNode>();

    /// <inheritdoc/>
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Represents an immutable attribute definition.
/// </summary>
public sealed record AttributeDefinition : CodeDefinition
{
    /// <inheritdoc/>
    public override string NodeType => "attribute";

    /// <summary>
    /// Gets the arguments for this attribute.
    /// </summary>
    public IReadOnlyList<object> Arguments { get; init; } = Array.Empty<object>();

    /// <inheritdoc/>
    public override IReadOnlyList<IAstNode> Children => Array.Empty<IAstNode>();

    /// <inheritdoc/>
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

/// <summary>
/// Represents an immutable generic parameter definition.
/// </summary>
public sealed record GenericParameterDefinition : CodeDefinition
{
    /// <inheritdoc/>
    public override string NodeType => "generic_parameter";

    /// <summary>
    /// Gets the constraints for this generic parameter.
    /// </summary>
    public IReadOnlyList<string> Constraints { get; init; } = Array.Empty<string>();

    /// <inheritdoc/>
    public override IReadOnlyList<IAstNode> Children => Array.Empty<IAstNode>();

    /// <inheritdoc/>
    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
}

// Product interfaces (these define the contract for the immutable products)

/// <summary>
/// Interface for class definition products.
/// </summary>
public interface IClassDefinition : IAstNode
{
    /// <summary>Gets the access modifier for this class.</summary>
    AccessModifier Access { get; }
    /// <summary>Gets the base class name, if any.</summary>
    string? BaseClass { get; }
    /// <summary>Gets the interfaces implemented by this class.</summary>
    IReadOnlyList<string> Interfaces { get; }
    /// <summary>Gets whether this class is abstract.</summary>
    bool IsAbstract { get; }
    /// <summary>Gets whether this class is sealed.</summary>
    bool IsSealed { get; }
    /// <summary>Gets whether this class is static.</summary>
    bool IsStatic { get; }
    /// <summary>Gets whether this class is partial.</summary>
    bool IsPartial { get; }
    /// <summary>Gets the methods defined in this class.</summary>
    IReadOnlyList<MethodDefinition> Methods { get; }
    /// <summary>Gets the properties defined in this class.</summary>
    IReadOnlyList<PropertyDefinition> Properties { get; }
    /// <summary>Gets the fields defined in this class.</summary>
    IReadOnlyList<FieldDefinition> Fields { get; }
}

/// <summary>
/// Interface for interface definition products.
/// </summary>
public interface IInterfaceDefinition : IAstNode
{
    /// <summary>Gets the access modifier for this interface.</summary>
    AccessModifier Access { get; }
    /// <summary>Gets the base interfaces extended by this interface.</summary>
    IReadOnlyList<string> BaseInterfaces { get; }
    /// <summary>Gets the methods defined in this interface.</summary>
    IReadOnlyList<MethodDefinition> Methods { get; }
    /// <summary>Gets the properties defined in this interface.</summary>
    IReadOnlyList<PropertyDefinition> Properties { get; }
}

/// <summary>
/// Interface for method definition products.
/// </summary>
public interface IMethodDefinition : IAstNode
{
    /// <summary>Gets the access modifier for this method.</summary>
    AccessModifier Access { get; }
    /// <summary>Gets the return type of this method.</summary>
    string ReturnType { get; }
    /// <summary>Gets the parameters of this method.</summary>
    IReadOnlyList<ParameterDefinition> Parameters { get; }
    /// <summary>Gets the body of this method.</summary>
    string? Body { get; }
}

/// <summary>
/// Interface for property definition products.
/// </summary>
public interface IPropertyDefinition : IAstNode
{
    /// <summary>Gets the access modifier for this property.</summary>
    AccessModifier Access { get; }
    /// <summary>Gets the type of this property.</summary>
    string Type { get; }
    /// <summary>Gets the getter accessor, if any.</summary>
    AccessorDefinition? Getter { get; }
    /// <summary>Gets the setter accessor, if any.</summary>
    AccessorDefinition? Setter { get; }
}

/// <summary>
/// Interface for field definition products.
/// </summary>
public interface IFieldDefinition : IAstNode
{
    /// <summary>Gets the access modifier for this field.</summary>
    AccessModifier Access { get; }
    /// <summary>Gets the type of this field.</summary>
    string Type { get; }
    /// <summary>Gets the initial value for this field.</summary>
    string? InitialValue { get; }
}

/// <summary>
/// Interface for parameter definition products.
/// </summary>
public interface IParameterDefinition : IAstNode
{
    /// <summary>Gets the type of this parameter.</summary>
    string Type { get; }
    /// <summary>Gets the default value for this parameter.</summary>
    string? DefaultValue { get; }
}

/// <summary>
/// Interface for namespace definition products.
/// </summary>
public interface INamespaceDefinition : IAstNode
{
    /// <summary>Gets the classes defined in this namespace.</summary>
    IReadOnlyList<ClassDefinition> Classes { get; }
    /// <summary>Gets the interfaces defined in this namespace.</summary>
    IReadOnlyList<InterfaceDefinition> Interfaces { get; }
}

/// <summary>
/// Interface for compilation unit definition products.
/// </summary>
public interface ICompilationUnitDefinition : IAstNode
{
    /// <summary>Gets the using statements/imports in this compilation unit.</summary>
    IReadOnlyList<string> Usings { get; }
    /// <summary>Gets the namespaces defined in this compilation unit.</summary>
    IReadOnlyList<NamespaceDefinition> Namespaces { get; }
}