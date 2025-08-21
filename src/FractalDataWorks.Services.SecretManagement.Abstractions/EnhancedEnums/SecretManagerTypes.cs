using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services.SecretManagement.Abstractions;

namespace FractalDataWorks.Services.SecretManagement.Abstractions.EnhancedEnums;

/// <summary>
/// Collection of available secret management service types.
/// </summary>
/// <remarks>
/// This Enhanced Enum collection provides discovery and registration of all
/// available secret management providers in the system. Each provider type
/// implements the SecretManagementServiceTypeBase pattern.
/// </remarks>
[EnumCollection]
public static class SecretManagerTypes
{
    /// <summary>
    /// Gets all registered secret management service types.
    /// </summary>
    /// <value>A collection of all available secret management service types.</value>
    public static IReadOnlyList<SecretManagementServiceTypeBase<IFdwService, ISecretManagementConfiguration>> All { get; } = 
        GetRegisteredTypes();

    /// <summary>
    /// Gets a secret management service type by name.
    /// </summary>
    /// <param name="name">The name of the secret management service type.</param>
    /// <returns>The secret management service type, or null if not found.</returns>
    public static SecretManagementServiceTypeBase<IFdwService, ISecretManagementConfiguration>? GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return All.FirstOrDefault(type => string.Equals(type.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets secret management service types that support a specific secret store.
    /// </summary>
    /// <param name="secretStore">The secret store identifier.</param>
    /// <returns>A collection of secret management service types that support the specified store.</returns>
    public static IReadOnlyList<SecretManagementServiceTypeBase<IFdwService, ISecretManagementConfiguration>> GetSupportingStore(string secretStore)
    {
        if (string.IsNullOrWhiteSpace(secretStore))
            return Array.Empty<SecretManagementServiceTypeBase<IFdwService, ISecretManagementConfiguration>>();

        return All.Where(type => type.SupportedSecretStores.Any(store => 
            string.Equals(store, secretStore, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(type => type.Priority)
            .ToList();
    }

    /// <summary>
    /// Gets secret management service types that support a specific authentication method.
    /// </summary>
    /// <param name="authenticationMethod">The authentication method identifier.</param>
    /// <returns>A collection of secret management service types that support the specified authentication method.</returns>
    public static IReadOnlyList<SecretManagementServiceTypeBase<IFdwService, ISecretManagementConfiguration>> GetSupportingAuthentication(string authenticationMethod)
    {
        if (string.IsNullOrWhiteSpace(authenticationMethod))
            return Array.Empty<SecretManagementServiceTypeBase<IFdwService, ISecretManagementConfiguration>>();

        return All.Where(type => type.SupportedAuthenticationMethods.Any(method => 
            string.Equals(method, authenticationMethod, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(type => type.Priority)
            .ToList();
    }

    /// <summary>
    /// Gets secret management service types that support a specific operation.
    /// </summary>
    /// <param name="operation">The operation identifier.</param>
    /// <returns>A collection of secret management service types that support the specified operation.</returns>
    public static IReadOnlyList<SecretManagementServiceTypeBase<IFdwService, ISecretManagementConfiguration>> GetSupportingOperation(string operation)
    {
        if (string.IsNullOrWhiteSpace(operation))
            return Array.Empty<SecretManagementServiceTypeBase<IFdwService, ISecretManagementConfiguration>>();

        return All.Where(type => type.SupportedOperations.Any(op => 
            string.Equals(op, operation, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(type => type.Priority)
            .ToList();
    }

    /// <summary>
    /// Gets secret management service types that support encryption at rest.
    /// </summary>
    /// <returns>A collection of secret management service types that support encryption at rest.</returns>
    public static IReadOnlyList<SecretManagementServiceTypeBase<IFdwService, ISecretManagementConfiguration>> GetWithEncryptionAtRest()
    {
        return All.Where(type => type.SupportsEncryptionAtRest)
            .OrderByDescending(type => type.Priority)
            .ToList();
    }

    /// <summary>
    /// Gets secret management service types that support audit logging.
    /// </summary>
    /// <returns>A collection of secret management service types that support audit logging.</returns>
    public static IReadOnlyList<SecretManagementServiceTypeBase<IFdwService, ISecretManagementConfiguration>> GetWithAuditLogging()
    {
        return All.Where(type => type.SupportsAuditLogging)
            .OrderByDescending(type => type.Priority)
            .ToList();
    }

    private static IReadOnlyList<SecretManagementServiceTypeBase<IFdwService, ISecretManagementConfiguration>> GetRegisteredTypes()
    {
        // This will be populated by the Enhanced Enum framework through reflection
        // when concrete service types are registered via [EnumOption] attributes
        var types = new List<SecretManagementServiceTypeBase<IFdwService, ISecretManagementConfiguration>>();
        
        // NOTE: The Enhanced Enum framework will populate this collection automatically
        // when it discovers types marked with [EnumOption] that inherit from SecretManagementServiceTypeBase
        
        return types.AsReadOnly();
    }
}