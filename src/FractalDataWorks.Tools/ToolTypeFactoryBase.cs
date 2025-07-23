using System;
using System.Threading.Tasks;

namespace FractalDataWorks.Tools;

/// <summary>
/// Base class for tool type factory definitions that create tool instances.
/// This is a generic base with basic constraints but no Enhanced Enum attributes.
/// </summary>
public abstract class ToolTypeFactoryBase<TTool, TConfiguration>
    where TTool : class, IFdwTool
    where TConfiguration : class, IFdwConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ToolTypeFactoryBase{TTool, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this tool type.</param>
    /// <param name="name">The name of this tool type.</param>
    /// <param name="description">The description of this tool type.</param>
    protected ToolTypeFactoryBase(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
    
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
    /// Creates a new instance of the tool with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use for creating the tool.</param>
    /// <returns>A new tool instance.</returns>
    public abstract object Create(TConfiguration configuration);
    
    /// <summary>
    /// Gets a tool instance by configuration name.
    /// </summary>
    /// <param name="configurationName">The name of the configuration to use.</param>
    /// <returns>A task containing the tool instance.</returns>
    public abstract Task<TTool> GetTool(string configurationName);
    
    /// <summary>
    /// Gets a tool instance by configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to use.</param>
    /// <returns>A task containing the tool instance.</returns>
    public abstract Task<TTool> GetTool(int configurationId);
}