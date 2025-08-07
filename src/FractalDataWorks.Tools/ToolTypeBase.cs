using System;
using System.Threading.Tasks;
using FractalDataWorks.Services;

namespace FractalDataWorks.Tools;

/// <summary>
/// Base class for tool type definitions.
/// Note: Enhanced Enum attributes temporarily disabled due to compatibility issues.
/// </summary>
// TODO: Re-enable when Enhanced Enum supports factory pattern
// [EnhancedEnumBase("ToolTypes", ReturnType = "IToolFactory", ReturnTypeNamespace = "FractalDataWorks")]
public abstract class ToolTypeBase
{
    /// <summary>
    /// Gets the unique identifier for this tool type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this tool type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of this tool type.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this tool type.</param>
    /// <param name="name">The name of this tool type.</param>
    /// <param name="description">The description of this tool type.</param>
    protected ToolTypeBase(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Creates a factory for this tool type.
    /// </summary>
    /// <returns>The tool factory.</returns>
    public virtual IToolFactory CreateFactory()
    {
        throw new NotImplementedException("Enhanced Enum implementation should override this method.");
    }
}

/// <summary>
/// Generic tool type base class that provides typed factory creation.
/// </summary>
/// <typeparam name="TTool">The tool type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type.</typeparam>
public abstract class ToolTypeBase<TTool, TConfiguration> : ToolTypeBase
    where TTool : class, IFdwTool
    where TConfiguration : class, IFdwConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ToolTypeBase{TTool, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this tool type.</param>
    /// <param name="name">The name of this tool type.</param>
    /// <param name="description">The description of this tool type.</param>
    protected ToolTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }

    /// <summary>
    /// Creates a typed factory for this tool type.
    /// </summary>
    /// <returns>The typed tool factory.</returns>
    public abstract IToolFactory<TTool, TConfiguration> CreateTypedFactory();

    /// <summary>
    /// Creates a factory for this tool type.
    /// </summary>
    /// <returns>The tool factory.</returns>
    public override IToolFactory CreateFactory() => CreateTypedFactory();
}