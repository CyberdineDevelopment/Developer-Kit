using System.Threading.Tasks;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.DataProviders.Abstractions;

/// <summary>
/// Base class for data provider service factories.
/// </summary>
/// <typeparam name="TDataService">The data service type.</typeparam>
/// <typeparam name="TDataProvidersConfiguration">The data providers configuration type.</typeparam>
public abstract class DataProvidersServiceFactoryBase<TDataService, TDataProvidersConfiguration>
    : ServiceFactoryBase, IServiceFactory<TDataService, TDataProvidersConfiguration>
    where TDataService : IDataService<IDataCommand>, IDataService
    where TDataProvidersConfiguration : IDataProvidersConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataProvidersServiceFactoryBase{TDataService, TDataProvidersConfiguration}"/> class.
    /// </summary>
    protected DataProvidersServiceFactoryBase() 
        : base() 
    { 
    }

    /// <summary>
    /// Creates a data provider service instance with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    public abstract IFdwResult<TDataService> Create(TDataProvidersConfiguration configuration);
    
    /// <summary>
    /// Creates a data provider service instance for the specified configuration name.
    /// </summary>
    /// <param name="configurationName">The name of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public abstract Task<TDataService> GetService(string configurationName);
    
    /// <summary>
    /// Creates a data provider service instance for the specified configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public abstract Task<TDataService> GetService(int configurationId);

    /// <inheritdoc/>
    public override IFdwResult<IFdwService> Create(IFdwConfiguration configuration)
    {
        if (configuration is TDataProvidersConfiguration dataConfig)
        {
            var result = Create(dataConfig);
            if (result.IsSuccess)
            {
                return FdwResult<IFdwService>.Success(result.Value!);
            }
            return FdwResult<IFdwService>.Failure(result.Message!);
        }
        return FdwResult<IFdwService>.Failure("Invalid configuration type.");
    }

    /// <inheritdoc/>
    IFdwResult<TDataService> IServiceFactory<TDataService>.Create(IFdwConfiguration configuration)
    {
        if (configuration is TDataProvidersConfiguration dataConfig)
        {
            return Create(dataConfig);
        }
        return FdwResult<TDataService>.Failure("Invalid configuration type.");
    }
}