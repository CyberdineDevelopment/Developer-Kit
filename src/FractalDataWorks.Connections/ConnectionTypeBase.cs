using System;
using System.Threading.Tasks;
using FractalDataWorks.Services;

namespace FractalDataWorks.Connections;

/// <summary>
/// Base class for connection type definitions.
/// </summary>
public abstract class ConnectionTypeBase
{
    /// <summary>
    /// Gets the unique identifier for this connection type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this connection type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of this connection type.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this connection type.</param>
    /// <param name="name">The name of this connection type.</param>
    /// <param name="description">The description of this connection type.</param>
    protected ConnectionTypeBase(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Creates a factory for this connection type.
    /// </summary>
    /// <returns>The connection factory.</returns>
    public abstract IConnectionFactory CreateFactory();
}

/// <summary>
/// Generic connection type base class that provides typed factory creation.
/// </summary>
/// <typeparam name="TConnection">The connection type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type.</typeparam>
public abstract class ConnectionTypeBase<TConnection, TConfiguration> : ConnectionTypeBase
    where TConnection : class, IExternalConnection
    where TConfiguration : class, IFdwConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTypeBase{TConnection, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this connection type.</param>
    /// <param name="name">The name of this connection type.</param>
    /// <param name="description">The description of this connection type.</param>
    protected ConnectionTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }

    /// <summary>
    /// Creates a typed factory for this connection type.
    /// </summary>
    /// <returns>The typed connection factory.</returns>
    public abstract IConnectionFactory<TConnection, TConfiguration> CreateTypedFactory();

    /// <summary>
    /// Creates a factory for this connection type.
    /// </summary>
    /// <returns>The connection factory.</returns>
    public override IConnectionFactory CreateFactory() => CreateTypedFactory();
}