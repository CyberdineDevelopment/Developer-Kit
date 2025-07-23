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
    protected ToolTypeFactoryBase(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
    
    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    
    public abstract object Create(TConfiguration configuration);
    public abstract Task<TTool> GetTool(string configurationName);
    public abstract Task<TTool> GetTool(int configurationId);
}