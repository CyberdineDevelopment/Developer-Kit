using System;
using System.Collections.Generic;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Base class for secret provider types that generates the SecretProviders collection.
/// Integrates with the EnhancedEnums system for automatic service discovery and registration.
/// </summary>
/// <remarks>
/// This base class provides the Enhanced Enum pattern for secret providers in the framework.
/// It handles provider type registration, factory creation, and integration with the EnhancedEnums
/// system for automatic collection generation and lookup capabilities.
/// </remarks>
[EnumCollection(
    CollectionName = "SecretProviders", 
    IncludeReferencedAssemblies = true,
    ReturnType = typeof(ISecretProvider))]
public abstract class SecretProviderType : ServiceTypeBase<ISecretProvider, SecretConfiguration>
{
    /// <summary>
    /// Gets the provider type identifier.
    /// </summary>
    /// <value>The provider type (e.g., "AwsSecretsManager", "AzureKeyVault", "HashiCorpVault").</value>
    [EnumLookup("GetByProviderType")]
    public string ProviderType { get; }
    
    /// <summary>
    /// Gets the version of this provider implementation.
    /// </summary>
    /// <value>The provider version string.</value>
    public string Version { get; }
    
    /// <summary>
    /// Gets the supported secret command types for this provider type.
    /// </summary>
    /// <value>A collection of command type names supported by this provider type.</value>
    [EnumLookup("GetByCommandType", allowMultiple: true)]
    public IReadOnlyCollection<string> SupportedCommandTypes { get; }
    
    /// <summary>
    /// Gets the supported container types for this provider type.
    /// </summary>
    /// <value>A collection of container type names supported by this provider type.</value>
    [EnumLookup("GetByContainerType", allowMultiple: true)]
    public IReadOnlyCollection<string> SupportedContainerTypes { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider type supports secret versioning.
    /// </summary>
    /// <value><c>true</c> if versioning is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetVersioningSupported")]
    public bool SupportsVersioning { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider type supports secret expiration.
    /// </summary>
    /// <value><c>true</c> if expiration is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetExpirationSupported")]
    public bool SupportsExpiration { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider type supports batch operations.
    /// </summary>
    /// <value><c>true</c> if batch operations are supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetBatchSupported")]
    public bool SupportsBatchOperations { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider type supports binary secret data.
    /// </summary>
    /// <value><c>true</c> if binary data is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetBinarySupported")]
    public bool SupportsBinarySecrets { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretProviderType"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this secret provider type.</param>
    /// <param name="name">The display name of this secret provider type.</param>
    /// <param name="description">The description of this secret provider type.</param>
    /// <param name="providerType">The provider type identifier.</param>
    /// <param name="version">The provider version.</param>
    /// <param name="supportedCommandTypes">The command types this provider type can execute.</param>
    /// <param name="supportedContainerTypes">The container types this provider type can work with.</param>
    /// <param name="supportsVersioning">Whether versioning is supported.</param>
    /// <param name="supportsExpiration">Whether expiration is supported.</param>
    /// <param name="supportsBatchOperations">Whether batch operations are supported.</param>
    /// <param name="supportsBinarySecrets">Whether binary secrets are supported.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when any string parameter is empty or whitespace, or when collections are empty.
    /// </exception>
    protected SecretProviderType(
        int id, 
        string name, 
        string description,
        string providerType,
        string version,
        IReadOnlyCollection<string> supportedCommandTypes,
        IReadOnlyCollection<string> supportedContainerTypes,
        bool supportsVersioning = false,
        bool supportsExpiration = false,
        bool supportsBatchOperations = false,
        bool supportsBinarySecrets = true) 
        : base(id, name, description)
    {
        if (string.IsNullOrWhiteSpace(providerType))
        {
            throw new ArgumentException("Provider type cannot be empty or whitespace.", nameof(providerType));
        }
        
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Version cannot be empty or whitespace.", nameof(version));
        }
        
        if (supportedCommandTypes.Count == 0)
        {
            throw new ArgumentException("At least one supported command type must be specified.", nameof(supportedCommandTypes));
        }
        
        if (supportedContainerTypes.Count == 0)
        {
            throw new ArgumentException("At least one supported container type must be specified.", nameof(supportedContainerTypes));
        }
        
        // Validate that all command types are not null or empty
        foreach (var commandType in supportedCommandTypes)
        {
            if (string.IsNullOrWhiteSpace(commandType))
            {
                throw new ArgumentException("Command types cannot be null, empty, or whitespace.", nameof(supportedCommandTypes));
            }
        }
        
        // Validate that all container types are not null or empty
        foreach (var containerType in supportedContainerTypes)
        {
            if (string.IsNullOrWhiteSpace(containerType))
            {
                throw new ArgumentException("Container types cannot be null, empty, or whitespace.", nameof(supportedContainerTypes));
            }
        }
        
        ProviderType = providerType;
        Version = version;
        SupportedCommandTypes = supportedCommandTypes;
        SupportedContainerTypes = supportedContainerTypes;
        SupportsVersioning = supportsVersioning;
        SupportsExpiration = supportsExpiration;
        SupportsBatchOperations = supportsBatchOperations;
        SupportsBinarySecrets = supportsBinarySecrets;
    }
    
    /// <summary>
    /// Creates a typed factory for this secret provider type.
    /// </summary>
    /// <returns>The typed service factory.</returns>
    public abstract override IServiceFactory<ISecretProvider, SecretConfiguration> CreateTypedFactory();
}