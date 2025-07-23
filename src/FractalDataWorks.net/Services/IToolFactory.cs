using System.Threading.Tasks;

namespace FractalDataWorks.Services;

/// <summary>
/// Generic factory interface for creating Tool instances
/// </summary>
public interface IToolFactory
{
    IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwTool;
    IFdwResult<IFdwTool> Create(IFdwConfiguration configuration);
}

/// <summary>
/// Generic factory interface for creating Tool instances of a specific type.
/// </summary>
public interface IToolFactory<TTool> : IToolFactory
    where TTool : IFdwTool
{
    new IFdwResult<TTool> Create(IFdwConfiguration configuration);
}

/// <summary>
/// Generic factory interface for creating Tool instances with specific configuration type.
/// </summary>
public interface IToolFactory<TTool, TConfiguration> : IToolFactory<TTool>
    where TTool : IFdwTool
    where TConfiguration : IFdwConfiguration
{
    IFdwResult<TTool> Create(TConfiguration configuration);
    Task<TTool> GetTool(string configurationName);
    Task<TTool> GetTool(int configurationId);
}