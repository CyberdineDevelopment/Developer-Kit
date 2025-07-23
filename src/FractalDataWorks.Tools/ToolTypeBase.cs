using System;
using System.Threading.Tasks;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Tools;

/// <summary>
/// Base class for tool type definitions using Enhanced Enums.
/// </summary>
[EnhancedEnumBase("ToolTypes", 
    ReturnType = "IToolFactory<IFdwTool, IFdwConfiguration>",
    ReturnTypeNamespace = "FractalDataWorks.Services")]
public abstract class ToolTypeBase<TTool, TConfiguration> : ToolTypeFactoryBase<TTool, TConfiguration>
    where TTool : class, IFdwTool
    where TConfiguration : class, IFdwConfiguration
{
    protected ToolTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }
}