using System.Threading.Tasks;

namespace FractalDataWorks.Services;

/// <summary>
/// Generic factory interface for creating Tool instances
/// </summary>
public interface IToolFactory
{
    /// <summary>
    /// Creates a tool of the specified type using the provided configuration.
    /// </summary>
    /// <typeparam name="T">The type of tool to create.</typeparam>
    /// <param name="configuration">The configuration to use for creating the tool.</param>
    /// <returns>A result containing the created tool or an error.</returns>
    IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwTool;

    /// <summary>
    /// Creates a tool using the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use for creating the tool.</param>
    /// <returns>A result containing the created tool or an error.</returns>
    IFdwResult<IFdwTool> Create(IFdwConfiguration configuration);
}

/// <summary>
/// Generic factory interface for creating Tool instances of a specific type.
/// </summary>
public interface IToolFactory<TTool> : IToolFactory
    where TTool : IFdwTool
{
    /// <summary>
    /// Creates a tool of the specified type using the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use for creating the tool.</param>
    /// <returns>A result containing the created tool or an error.</returns>
    new IFdwResult<TTool> Create(IFdwConfiguration configuration);
}

/// <summary>
/// Generic factory interface for creating Tool instances with specific configuration type.
/// </summary>
public interface IToolFactory<TTool, TConfiguration> : IToolFactory<TTool>
    where TTool : IFdwTool
    where TConfiguration : IFdwConfiguration
{
    /// <summary>
    /// Creates a tool using the provided typed configuration.
    /// </summary>
    /// <param name="configuration">The typed configuration to use for creating the tool.</param>
    /// <returns>A result containing the created tool or an error.</returns>
    IFdwResult<TTool> Create(TConfiguration configuration);

    /// <summary>
    /// Gets a tool by configuration name.
    /// </summary>
    /// <param name="configurationName">The name of the configuration.</param>
    /// <returns>A task containing the tool.</returns>
    Task<TTool> GetTool(string configurationName);

    /// <summary>
    /// Gets a tool by configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration.</param>
    /// <returns>A task containing the tool.</returns>
    Task<TTool> GetTool(int configurationId);
}