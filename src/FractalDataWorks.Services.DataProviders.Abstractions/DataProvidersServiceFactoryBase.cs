using System.Threading.Tasks;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.DataProviders.Abstractions;

/// <summary>
/// Base class for data provider service factories.
/// </summary>
/// <typeparam name="TDataService">The data service type.</typeparam>
/// <typeparam name="TDataProvidersConfiguration">The data providers configuration type.</typeparam>
public abstract class DataProvidersServiceFactoryBase<TDataService, TDataProvidersConfiguration>
    : ServiceFactoryBase<TDataService, TDataProvidersConfiguration>
    where TDataService : class, IDataService<IDataCommand>, IDataService
    where TDataProvidersConfiguration : class, IDataProvidersConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataProvidersServiceFactoryBase{TDataService, TDataProvidersConfiguration}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    protected DataProvidersServiceFactoryBase(ILogger? logger = null) 
        : base(logger) 
    { 
    }

    
    /// <summary>
    /// Creates a data provider service instance for the specified configuration name.
    /// </summary>
    /// <param name="configurationName">The name of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override abstract Task<TDataService> GetService(string configurationName);
    
    /// <summary>
    /// Creates a data provider service instance for the specified configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override abstract Task<TDataService> GetService(int configurationId);

    /// <summary>
    /// Creates a service instance with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    protected abstract override IFdwResult<TDataService> CreateCore(TDataProvidersConfiguration configuration);
}