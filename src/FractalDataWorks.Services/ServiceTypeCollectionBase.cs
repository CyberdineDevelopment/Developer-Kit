using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FractalDataWorks.Services;

/// <summary>
/// Base class for Enhanced Enum collections of service types.
/// This class should be inherited by source-generated Enhanced Enum collections.
/// </summary>
/// <typeparam name="TServiceType">The service type that inherits from ServiceTypeBase.</typeparam>
/// <typeparam name="TService">The service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type.</typeparam>
/// <ExcludeFromTest>Abstract base class for service type collections with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage(Justification = "Abstract base class for service type collections with no business logic")]
public abstract class ServiceTypeCollectionBase<TServiceType, TService, TConfiguration>
    where TServiceType : ServiceTypeBase<TService, TConfiguration>
    where TService : class, IFdwService
    where TConfiguration : class, IFdwConfiguration
{
    private static readonly Dictionary<string, TServiceType> _serviceTypesByName = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<int, TServiceType> _serviceTypesById = new();
    private static readonly List<TServiceType> _allServiceTypes = new();
    private static bool _isInitialized;
    private static readonly object _initLock = new();

    /// <summary>
    /// Gets all registered service types.
    /// </summary>
    public static IReadOnlyList<TServiceType> All
    {
        get
        {
            EnsureInitialized();
            return _allServiceTypes.AsReadOnly();
        }
    }

    /// <summary>
    /// Static factory method that creates a service instance using the appropriate service type.
    /// </summary>
    /// <param name="serviceTypeName">The name of the service type to use.</param>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error.</returns>
    public static IFdwResult<TService> CreateService(string serviceTypeName, TConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(serviceTypeName))
        {
            return FdwResult<TService>.Failure("Service type name cannot be null or empty");
        }

        if (configuration == null)
        {
            return FdwResult<TService>.Failure("Configuration cannot be null");
        }

        EnsureInitialized();

        if (!_serviceTypesByName.TryGetValue(serviceTypeName, out var serviceType))
        {
            return FdwResult<TService>.Failure($"Service type '{serviceTypeName}' not found");
        }

        return serviceType.Create(configuration);
    }

    /// <summary>
    /// Static factory method that creates a service instance using the service type ID.
    /// </summary>
    /// <param name="serviceTypeId">The ID of the service type to use.</param>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error.</returns>
    public static IFdwResult<TService> CreateService(int serviceTypeId, TConfiguration configuration)
    {
        if (configuration == null)
        {
            return FdwResult<TService>.Failure("Configuration cannot be null");
        }

        EnsureInitialized();

        if (!_serviceTypesById.TryGetValue(serviceTypeId, out var serviceType))
        {
            return FdwResult<TService>.Failure($"Service type with ID '{serviceTypeId}' not found");
        }

        return serviceType.Create(configuration);
    }

    /// <summary>
    /// Gets a service type by name.
    /// </summary>
    /// <param name="name">The name of the service type.</param>
    /// <returns>The service type, or null if not found.</returns>
    public static TServiceType? GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        EnsureInitialized();
        return _serviceTypesByName.TryGetValue(name, out var serviceType) ? serviceType : null;
    }

    /// <summary>
    /// Gets a service type by ID.
    /// </summary>
    /// <param name="id">The ID of the service type.</param>
    /// <returns>The service type, or null if not found.</returns>
    public static TServiceType? GetById(int id)
    {
        EnsureInitialized();
        return _serviceTypesById.TryGetValue(id, out var serviceType) ? serviceType : null;
    }

    /// <summary>
    /// Tries to get a service type by name.
    /// </summary>
    /// <param name="name">The name of the service type.</param>
    /// <param name="serviceType">The service type, if found.</param>
    /// <returns>True if the service type was found; otherwise, false.</returns>
    public static bool TryGetByName(string name, out TServiceType? serviceType)
    {
        serviceType = GetByName(name);
        return serviceType != null;
    }

    /// <summary>
    /// Tries to get a service type by ID.
    /// </summary>
    /// <param name="id">The ID of the service type.</param>
    /// <param name="serviceType">The service type, if found.</param>
    /// <returns>True if the service type was found; otherwise, false.</returns>
    public static bool TryGetById(int id, out TServiceType? serviceType)
    {
        serviceType = GetById(id);
        return serviceType != null;
    }

    /// <summary>
    /// Registers a service type with the collection.
    /// This method is called by the source-generated code.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    protected static void Register(TServiceType serviceType)
    {
        if (serviceType == null)
        {
            throw new ArgumentNullException(nameof(serviceType));
        }

        lock (_initLock)
        {
            if (_serviceTypesByName.ContainsKey(serviceType.Name))
            {
                throw new InvalidOperationException($"Service type '{serviceType.Name}' is already registered");
            }

            if (_serviceTypesById.ContainsKey(serviceType.Id))
            {
                throw new InvalidOperationException($"Service type with ID '{serviceType.Id}' is already registered");
            }

            _serviceTypesByName[serviceType.Name] = serviceType;
            _serviceTypesById[serviceType.Id] = serviceType;
            _allServiceTypes.Add(serviceType);
        }
    }

    /// <summary>
    /// Initializes the collection.
    /// This method should be overridden by the source-generated code to register all service types.
    /// </summary>
    protected abstract void Initialize();

    /// <summary>
    /// Ensures the collection has been initialized.
    /// </summary>
    private static void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        lock (_initLock)
        {
            if (_isInitialized)
            {
                return;
            }

            // Create an instance to trigger the Initialize method
            var instance = Activator.CreateInstance<ServiceTypeCollectionBase<TServiceType, TService, TConfiguration>>();
            instance.Initialize();
            _isInitialized = true;
        }
    }

    /// <summary>
    /// Gets the count of registered service types.
    /// </summary>
    public static int Count
    {
        get
        {
            EnsureInitialized();
            return _allServiceTypes.Count;
        }
    }

    /// <summary>
    /// Checks if a service type with the specified name exists.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if a service type with the name exists; otherwise, false.</returns>
    public static bool Contains(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        EnsureInitialized();
        return _serviceTypesByName.ContainsKey(name);
    }

    /// <summary>
    /// Checks if a service type with the specified ID exists.
    /// </summary>
    /// <param name="id">The ID to check.</param>
    /// <returns>True if a service type with the ID exists; otherwise, false.</returns>
    public static bool Contains(int id)
    {
        EnsureInitialized();
        return _serviceTypesById.ContainsKey(id);
    }
}