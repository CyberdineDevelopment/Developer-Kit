using Microsoft.Extensions.Logging;
using FractalDataWorks.Services;
using FractalDataWorks.Data;

namespace FractalDataWorks.Services.DataProviders.Abstractions;

/// <summary>
/// Base class for data provider services.
/// </summary>
/// <typeparam name="TDataCommand">The data command type.</typeparam>
/// <typeparam name="TDataProvidersConfiguration">The data providers configuration type.</typeparam>
/// <typeparam name="TDataProvidersService">The concrete data providers service type for logging category.</typeparam>
public abstract class DataProvidersServiceBase<TDataCommand, TDataProvidersConfiguration, TDataProvidersService> 
    : ServiceBase<TDataCommand, TDataProvidersConfiguration, TDataProvidersService>, IDataService
    where TDataCommand : IDataCommand
    where TDataProvidersConfiguration : IDataProvidersConfiguration
    where TDataProvidersService : DataProvidersServiceBase<TDataCommand, TDataProvidersConfiguration, TDataProvidersService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataProvidersServiceBase{TDataCommand, TDataProvidersConfiguration, TDataProvidersService}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for the concrete service type.</param>
    /// <param name="configuration">The data providers configuration.</param>
    protected DataProvidersServiceBase(ILogger<TDataProvidersService> logger, TDataProvidersConfiguration configuration) 
        : base(logger, configuration) 
    { 
    }
}