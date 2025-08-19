using System.Collections.Generic;
using FractalDataWorks.Configuration.Abstractions;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.DataProvider.Abstractions;

/// <summary>
/// Base class for data provider service providers.
/// </summary>
/// <typeparam name="TDataProvidersServiceType">The data provider service type.</typeparam>
/// <typeparam name="TDataService">The data service interface.</typeparam>
/// <typeparam name="TDataProvidersConfiguration">The data providers configuration type.</typeparam>
public abstract class DataProvidersServiceProviderBase<TDataProvidersServiceType, TDataService, TDataProvidersConfiguration>
    : ServiceTypeProviderBase<TDataService, TDataProvidersServiceType, TDataProvidersConfiguration>
    where TDataProvidersServiceType : DataProvidersServiceTypeBase<TDataService, TDataProvidersConfiguration>
    where TDataService : class, IDataService
    where TDataProvidersConfiguration : class, IDataProvidersConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataProvidersServiceProviderBase{TDataProvidersServiceType, TDataService, TDataProvidersConfiguration}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configurationRegistry">The configuration registry.</param>
    /// <param name="serviceTypes">The collection of available service types.</param>
    protected DataProvidersServiceProviderBase(
        ILogger<DataProvidersServiceProviderBase<TDataProvidersServiceType, TDataService, TDataProvidersConfiguration>> logger,
        IConfigurationRegistry<TDataProvidersConfiguration> configurationRegistry,
        IEnumerable<TDataProvidersServiceType> serviceTypes)
        : base(logger, configurationRegistry, serviceTypes) 
    { 
    }
}