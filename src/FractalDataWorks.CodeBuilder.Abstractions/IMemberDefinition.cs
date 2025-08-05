using System;
using System.Collections.Generic;

namespace FractalDataWorks.CodeBuilder.Abstractions;

/// <summary>
/// Base interface for all class/interface member definitions (methods, properties, fields).
/// </summary>
public interface IMemberDefinition : IAstNode
{
    /// <summary>
    /// Gets the access modifier for this member.
    /// </summary>
    AccessModifier Access { get; }

    /// <summary>
    /// Gets whether this member is static.
    /// </summary>
    bool IsStatic { get; }

    /// <summary>
    /// Gets the attributes applied to this member.
    /// </summary>
    IReadOnlyList<AttributeDefinition> Attributes { get; }

    /// <summary>
    /// Gets the documentation for this member.
    /// </summary>
    string? Documentation { get; }
}

/// <summary>
/// Interface for members that can be virtual/override (methods, properties, events).
/// </summary>
public interface IVirtualizableMember : IMemberDefinition
{
    /// <summary>
    /// Gets whether this member is virtual.
    /// </summary>
    bool IsVirtual { get; }

    /// <summary>
    /// Gets whether this member is an override.
    /// </summary>
    bool IsOverride { get; }
}

/// <summary>
/// Interface for members that can be abstract (methods, properties, events).
/// </summary>
public interface IAbstractableMember : IVirtualizableMember
{
    /// <summary>
    /// Gets whether this member is abstract.
    /// </summary>
    bool IsAbstract { get; }
}