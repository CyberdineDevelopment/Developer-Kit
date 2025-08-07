using System;
using System.Collections.Generic;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.DataProviders.Abstractions;

/// <summary>
/// Base class for data provider types that generates the DataProviders collection.
/// Integrates with the EnhancedEnums system for automatic service discovery and registration.
/// </summary>
/// <remarks>
/// This base class provides the Enhanced Enum pattern for data providers in the framework.
/// It handles provider type registration, factory creation, and integration with the EnhancedEnums
/// system for automatic collection generation and lookup capabilities.
/// </remarks>
[EnumCollection(
    CollectionName = "DataProviders", 
    IncludeReferencedAssemblies = true,
    ReturnType = typeof(IDataProvider))]
public abstract class DataProviderType : ServiceTypeBase<IDataProvider, IDataConfiguration>
{
    /// <summary>
    /// Gets the types of data commands this provider type can execute.
    /// </summary>
    /// <value>A collection of command type names supported by this provider type.</value>
    [EnumLookup("GetByCommandType", allowMultiple: true)]
    public IReadOnlyList<string> SupportedCommandTypes { get; }
    
    /// <summary>
    /// Gets the data sources this provider type can work with.
    /// </summary>
    /// <value>A collection of data source identifiers supported by this provider type.</value>
    [EnumLookup("GetByDataSource", allowMultiple: true)]
    public IReadOnlyList<string> SupportedDataSources { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider type supports external connections.
    /// </summary>
    /// <value><c>true</c> if external connections are supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetConnectionSupported")]
    public bool SupportsExternalConnections { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider type supports transactions.
    /// </summary>
    /// <value><c>true</c> if transactions are supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetTransactionSupported")]
    public bool SupportsTransactions { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider type supports batch operations.
    /// </summary>
    /// <value><c>true</c> if batch operations are supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetBatchSupported")]
    public bool SupportsBatchOperations { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider type supports streaming operations.
    /// </summary>
    /// <value><c>true</c> if streaming is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetStreamingSupported")]
    public bool SupportsStreaming { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DataProviderType"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this data provider type.</param>
    /// <param name="name">The display name of this data provider type.</param>
    /// <param name="description">The description of this data provider type.</param>
    /// <param name="supportedCommandTypes">The command types this provider type can execute.</param>
    /// <param name="supportedDataSources">The data sources this provider type can work with.</param>
    /// <param name="supportsExternalConnections">Whether external connections are supported.</param>
    /// <param name="supportsTransactions">Whether transactions are supported.</param>
    /// <param name="supportsBatchOperations">Whether batch operations are supported.</param>
    /// <param name="supportsStreaming">Whether streaming operations are supported.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when any string parameter is empty or whitespace, or when collections are empty.
    /// </exception>
    protected DataProviderType(
        int id, 
        string name, 
        string description,
        IReadOnlyList<string> supportedCommandTypes,
        IReadOnlyList<string> supportedDataSources,
        bool supportsExternalConnections = true,
        bool supportsTransactions = true,
        bool supportsBatchOperations = false,
        bool supportsStreaming = false) 
        : base(id, name, description)
    {
        if (supportedCommandTypes.Count == 0)
        {
            throw new ArgumentException("At least one supported command type must be specified.", nameof(supportedCommandTypes));
        }
        
        if (supportedDataSources.Count == 0)
        {
            throw new ArgumentException("At least one supported data source must be specified.", nameof(supportedDataSources));
        }
        
        // Validate that all command types are not null or empty
        for (int i = 0; i < supportedCommandTypes.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(supportedCommandTypes[i]))
            {
                throw new ArgumentException($"Command type at index {i} cannot be null, empty, or whitespace.", nameof(supportedCommandTypes));
            }
        }
        
        // Validate that all data sources are not null or empty
        for (int i = 0; i < supportedDataSources.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(supportedDataSources[i]))
            {
                throw new ArgumentException($"Data source at index {i} cannot be null, empty, or whitespace.", nameof(supportedDataSources));
            }
        }
        
        SupportedCommandTypes = supportedCommandTypes;
        SupportedDataSources = supportedDataSources;
        SupportsExternalConnections = supportsExternalConnections;
        SupportsTransactions = supportsTransactions;
        SupportsBatchOperations = supportsBatchOperations;
        SupportsStreaming = supportsStreaming;
    }
    
    /// <summary>
    /// Creates a typed factory for this data provider type.
    /// </summary>
    /// <returns>The typed service factory.</returns>
    public abstract override IServiceFactory<IDataProvider, IDataConfiguration> CreateTypedFactory();
}