namespace FractalDataWorks;

/// <summary>
/// Registry for managing configuration objects.
/// </summary>
public interface IConfigurationRegistry
{
    /// <summary>
    /// Registers a configuration object.
    /// </summary>
    /// <typeparam name="T">The type of configuration.</typeparam>
    /// <param name="configuration">The configuration object to register.</param>
    void Register<T>(T configuration) where T : IFdwConfiguration;
    
    /// <summary>
    /// Gets a configuration object by type.
    /// </summary>
    /// <typeparam name="T">The type of configuration.</typeparam>
    /// <returns>The configuration object if found; otherwise, null.</returns>
    T? Get<T>() where T : IFdwConfiguration;
    
    /// <summary>
    /// Checks if a configuration type is registered.
    /// </summary>
    /// <typeparam name="T">The type of configuration.</typeparam>
    /// <returns>True if the configuration type is registered; otherwise, false.</returns>
    bool IsRegistered<T>() where T : IFdwConfiguration;
}
