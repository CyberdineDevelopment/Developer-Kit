using System;
using System.Threading.Tasks;

namespace FractalDataWorks.Services;

/// <summary>
/// Base class for service type definitions.
/// Note: Enhanced Enum attributes temporarily removed due to compatibility issues.
/// </summary>
public abstract class ServiceTypeBase
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
        throw new NotImplementedException("Enhanced Enum implementation should override this method.");
    }
}

/// <summary>
/// Generic service type base class that inherits from non-generic Enhanced Enum base.
/// </summary>
/// <typeparam name="TService">The service type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type.</typeparam>
public abstract class ServiceTypeBase<TService, TConfiguration> : ServiceTypeBase
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
}