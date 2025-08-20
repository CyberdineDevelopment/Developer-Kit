using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FractalDataWorks.Services.EnhancedEnums;

/// <summary>
/// Base class for service type collections that provides common functionality
/// for managing and registering service types. This class is used by the Enhanced Enums 
/// generator when it detects that an enum inherits from ServiceTypeBase. Instead of 
/// generating a static collection class, it generates a sealed class that inherits 
/// from this base to provide dependency injection integration.
/// </summary>
/// <typeparam name="TServiceType">The service type that this collection manages (your concrete ServiceType enum option).</typeparam>
/// <typeparam name="TService">The service interface type that the service types provide.</typeparam>
/// <typeparam name="TConfiguration">The configuration type used by the service types.</typeparam>
/// <remarks>
/// This class is automatically used by Enhanced Enums code generation when ServiceType 
/// inheritance is detected. Generated collection classes inherit from this base and 
/// use the protected Register method in their static constructors.
/// </remarks>
public abstract class ServiceTypeCollectionBase<TServiceType, TService, TConfiguration>
    where TServiceType : class
    where TService : class
{
    private static readonly ConcurrentDictionary<string, TServiceType> _registeredTypes = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<int, TServiceType> _registeredTypesById = new();

    /// <summary>
    /// Gets all registered service types.
    /// </summary>
    public static ImmutableArray<TServiceType> All => _registeredTypes.Values.ToImmutableArray();

    /// <summary>
    /// Gets the count of registered service types.
    /// </summary>
    public static int Count => _registeredTypes.Count;

    /// <summary>
    /// Gets a service type by its name.
    /// </summary>
    /// <param name="name">The name of the service type.</param>
    /// <returns>The service type if found; otherwise, null.</returns>
    public static TServiceType? GetByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        return _registeredTypes.TryGetValue(name, out var serviceType) ? serviceType : null;
    }

    /// <summary>
    /// Tries to get a service type by its name.
    /// </summary>
    /// <param name="name">The name of the service type.</param>
    /// <param name="serviceType">When this method returns, contains the service type if found; otherwise, null.</param>
    /// <returns>true if the service type was found; otherwise, false.</returns>
    public static bool TryGetByName(string name, out TServiceType? serviceType)
    {
        serviceType = GetByName(name);
        return serviceType != null;
    }

    /// <summary>
    /// Gets a service type by its ID.
    /// </summary>
    /// <param name="id">The ID of the service type.</param>
    /// <returns>The service type if found; otherwise, null.</returns>
    public static TServiceType? GetById(int id)
    {
        return _registeredTypesById.TryGetValue(id, out var serviceType) ? serviceType : null;
    }

    /// <summary>
    /// Tries to get a service type by its ID.
    /// </summary>
    /// <param name="id">The ID of the service type.</param>
    /// <param name="serviceType">When this method returns, contains the service type if found; otherwise, null.</param>
    /// <returns>true if the service type was found; otherwise, false.</returns>
    public static bool TryGetById(int id, out TServiceType? serviceType)
    {
        serviceType = GetById(id);
        return serviceType != null;
    }

    /// <summary>
    /// Determines whether the collection contains a service type with the specified name.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>true if the collection contains a service type with the specified name; otherwise, false.</returns>
    public static bool Contains(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;
        return _registeredTypes.ContainsKey(name);
    }

    /// <summary>
    /// Determines whether the collection contains a service type with the specified ID.
    /// </summary>
    /// <param name="id">The ID to check.</param>
    /// <returns>true if the collection contains a service type with the specified ID; otherwise, false.</returns>
    public static bool Contains(int id)
    {
        return _registeredTypesById.ContainsKey(id);
    }

    /// <summary>
    /// Registers a service type with this collection. This method is called automatically
    /// by generated Enhanced Enum collection classes in their static constructors.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <param name="name">The name to register the service type under. If null, will attempt to extract from the service type using reflection.</param>
    /// <param name="id">The ID to register the service type under. If null, will attempt to extract from the service type using reflection.</param>
    protected static void Register(TServiceType serviceType, string? name = null, int? id = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        // If no name provided, try to get it from the service type
        var serviceName = name ?? ExtractServiceTypeName(serviceType);
        if (string.IsNullOrEmpty(serviceName))
            throw new ArgumentException("Unable to determine name for service type", nameof(serviceType));

        // If no ID provided, try to get it from the service type
        var serviceId = id ?? ExtractServiceTypeId(serviceType);
        if (!serviceId.HasValue)
            throw new ArgumentException("Unable to determine ID for service type", nameof(serviceType));

        _registeredTypes.TryAdd(serviceName, serviceType);
        _registeredTypesById.TryAdd(serviceId.Value, serviceType);
    }

    /// <summary>
    /// Extracts the name from a service type using reflection on the Name property.
    /// This method can be overridden in derived collection classes to customize name extraction.
    /// </summary>
    /// <param name="serviceType">The service type to extract the name from.</param>
    /// <returns>The extracted name from the Name property, or null if not found.</returns>
    protected static string? ExtractServiceTypeName(TServiceType serviceType)
    {
        // Try to get the Name property using reflection
        var nameProperty = serviceType.GetType().GetProperty("Name");
        return nameProperty?.GetValue(serviceType)?.ToString();
    }

    /// <summary>
    /// Extracts the ID from a service type using reflection on the Id property.
    /// This method can be overridden in derived collection classes to customize ID extraction.
    /// </summary>
    /// <param name="serviceType">The service type to extract the ID from.</param>
    /// <returns>The extracted ID from the Id property, or null if not found.</returns>
    protected static int? ExtractServiceTypeId(TServiceType serviceType)
    {
        // Try to get the Id property using reflection
        var idProperty = serviceType.GetType().GetProperty("Id");
        var idValue = idProperty?.GetValue(serviceType);

        if (idValue is int intId)
            return intId;

        return null;
    }
}