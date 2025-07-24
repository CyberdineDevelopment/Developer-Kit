using System;
using System.Threading.Tasks;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services;

/// <summary>
/// Base class for service type definitions.
/// </summary>
[EnhancedEnumBase("ServiceTypes", ReturnType = "IServiceFactory", ReturnTypeNamespace = "FractalDataWorks")]
public abstract class ServiceTypeBase : IServiceFactory
{
    /// <summary>
    /// Gets the unique identifier for this service type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this service type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of this service type.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="description">The description of this service type.</param>
    protected ServiceTypeBase(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Creates a factory for this service type.
    /// </summary>
    /// <returns>The service factory.</returns>
    public virtual IServiceFactory CreateFactory()
    {
        throw new NotSupportedException($"Service type {Name} does not support factory creation.");
    }

    /// <summary>
    /// Creates a service instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of service to create.</typeparam>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    public virtual IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwService
    {
        throw new NotSupportedException($"Service type {Name} does not support direct service creation.");
    }

    /// <summary>
    /// Creates a service instance.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    public virtual IFdwResult<IFdwService> Create(IFdwConfiguration configuration)
    {
        throw new NotSupportedException($"Service type {Name} does not support direct service creation.");
    }
}

/// <summary>
/// Generic service type base class that provides typed factory creation.
/// </summary>
/// <typeparam name="TService">The service type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type.</typeparam>
public abstract class ServiceTypeBase<TService, TConfiguration> : ServiceTypeBase, IServiceFactory<TService, TConfiguration>
    where TService : class, IFdwService
    where TConfiguration : class, IFdwConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeBase{TService, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="description">The description of this service type.</param>
    protected ServiceTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }

    /// <summary>
    /// Creates a typed factory for this service type.
    /// </summary>
    /// <returns>The typed service factory.</returns>
    public abstract IServiceFactory<TService, TConfiguration> CreateTypedFactory();

    /// <summary>
    /// Creates a factory for this service type.
    /// </summary>
    /// <returns>The service factory.</returns>
    public override IServiceFactory CreateFactory() => CreateTypedFactory();

    /// <summary>
    /// Creates a service instance with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    public IFdwResult<TService> Create(TConfiguration configuration)
    {
        return CreateTypedFactory().Create(configuration);
    }

    /// <summary>
    /// Creates a service instance.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    IFdwResult<TService> IServiceFactory<TService>.Create(IFdwConfiguration configuration)
    {
        if (configuration is TConfiguration typedConfig)
        {
            return Create(typedConfig);
        }
        return FdwResult<TService>.Failure(Messages.ServiceMessages.InvalidCommand);
    }

    /// <summary>
    /// Creates a service instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of service to create.</typeparam>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    public override IFdwResult<T> Create<T>(IFdwConfiguration configuration)
    {
        if (typeof(T) == typeof(TService))
        {
            var result = Create(configuration);
            if (result.IsSuccess)
            {
                return (IFdwResult<T>)(object)result;
            }
            return FdwResult<T>.Failure(result.Message!);
        }
        return FdwResult<T>.Failure(Messages.ServiceMessages.InvalidCommand);
    }

    /// <summary>
    /// Creates a service instance.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    public override IFdwResult<IFdwService> Create(IFdwConfiguration configuration)
    {
        var result = Create(configuration);
        if (result.IsSuccess)
        {
            return FdwResult<IFdwService>.Success(result.Value);
        }
        return FdwResult<IFdwService>.Failure(result.Message!);
    }

    /// <summary>
    /// Creates a service instance for the specified configuration name.
    /// </summary>
    /// <param name="configurationName">The name of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task<TService> GetService(string configurationName)
    {
        return CreateTypedFactory().GetService(configurationName);
    }

    /// <summary>
    /// Creates a service instance for the specified configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task<TService> GetService(int configurationId)
    {
        return CreateTypedFactory().GetService(configurationId);
    }
}