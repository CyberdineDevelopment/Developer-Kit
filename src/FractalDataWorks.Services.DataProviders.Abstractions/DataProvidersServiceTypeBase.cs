using FractalDataWorks.Services;

namespace FractalDataWorks.Services.DataProviders.Abstractions;

/// <summary>
/// Base class for data provider service type definitions.
/// </summary>
/// <typeparam name="TDataService">The data service type.</typeparam>
/// <typeparam name="TDataProvidersConfiguration">The data providers configuration type.</typeparam>
public abstract class DataProvidersServiceTypeBase<TDataService, TDataProvidersConfiguration>
    : ServiceTypeBase<TDataService, TDataProvidersConfiguration>
    where TDataService : IDataService
    where TDataProvidersConfiguration : IDataProvidersConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataProvidersServiceTypeBase{TDataService, TDataProvidersConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="description">The description of this service type.</param>
    protected DataProvidersServiceTypeBase(int id, string name, string description) 
        : base(id, name, description) 
    { 
    }
}