using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Results;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Base implementation of a configuration source.
/// </summary>
public abstract class ConfigurationSourceBase : IFractalConfigurationSource
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationSourceBase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="name">The name of this configuration source.</param>
    protected ConfigurationSourceBase(ILogger logger, string name)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Gets the name of this configuration source.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this source supports write operations.
    /// </summary>
    public abstract bool IsWritable { get; }

    /// <summary>
    /// Gets a value indicating whether this source supports automatic reload.
    /// </summary>
    public abstract bool SupportsReload { get; }

    /// <summary>
    /// Occurs when the configuration source changes.
    /// </summary>
    public event EventHandler<ConfigurationSourceChangedEventArgs>? Changed;

    /// <summary>
    /// Loads configurations from this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to load.</typeparam>
    /// <returns>A task containing the loaded configurations.</returns>
    public abstract Task<FractalResult<IEnumerable<TConfiguration>>> Load<TConfiguration>()
        where TConfiguration : IFractalConfiguration;

    /// <summary>
    /// Saves a configuration to this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to save.</typeparam>
    /// <param name="configuration">The configuration to save.</param>
    /// <returns>A task containing the save operation result.</returns>
    public virtual Task<FractalResult<TConfiguration>> Save<TConfiguration>(TConfiguration configuration)
        where TConfiguration : IFractalConfiguration
    {
        if (!IsWritable)
        {
            return Task.FromResult(
                FractalResult<TConfiguration>.Failure($"Configuration source '{Name}' is read-only"));
        }

        return SaveCore(configuration);
    }

    /// <summary>
    /// Deletes a configuration from this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to delete.</typeparam>
    /// <param name="id">The ID of the configuration to delete.</param>
    /// <returns>A task containing the delete operation result.</returns>
    public virtual Task<FractalResult<NonResult>> Delete<TConfiguration>(int id)
        where TConfiguration : IFractalConfiguration
    {
        if (!IsWritable)
        {
            return Task.FromResult(
                FractalResult<NonResult>.Failure($"Configuration source '{Name}' is read-only"));
        }

        return DeleteCore<TConfiguration>(id);
    }

    /// <summary>
    /// Core implementation of save operation.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to save.</typeparam>
    /// <param name="configuration">The configuration to save.</param>
    /// <returns>A task containing the save operation result.</returns>
    protected abstract Task<FractalResult<TConfiguration>> SaveCore<TConfiguration>(TConfiguration configuration)
        where TConfiguration : IFractalConfiguration;

    /// <summary>
    /// Core implementation of delete operation.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to delete.</typeparam>
    /// <param name="id">The ID of the configuration to delete.</param>
    /// <returns>A task containing the delete operation result.</returns>
    protected abstract Task<FractalResult<NonResult>> DeleteCore<TConfiguration>(int id)
        where TConfiguration : IFractalConfiguration;

    /// <summary>
    /// Raises the Changed event.
    /// </summary>
    /// <param name="changeType">The type of change that occurred.</param>
    /// <param name="configurationType">The type of configuration that changed.</param>
    /// <param name="configurationId">The ID of the configuration that changed.</param>
    protected void OnChanged(
        ConfigurationChangeType changeType,
        Type configurationType,
        int? configurationId = null)
    {
        var args = new ConfigurationSourceChangedEventArgs(changeType, configurationType, configurationId);
        Changed?.Invoke(this, args);

        _logger.LogDebug("Configuration source '{SourceName}' raised {ChangeType} event for {ConfigurationType}",
            Name, changeType, configurationType.Name);
    }
}