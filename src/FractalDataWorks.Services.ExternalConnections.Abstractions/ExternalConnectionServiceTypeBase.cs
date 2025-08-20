using System;
using System.Collections.Generic;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions;

/// <summary>
/// Base class for external connection service type definitions.
/// Used by Enhanced Enums to register different external connection providers.
/// </summary>
/// <typeparam name="TService">The external connection service type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the connection service.</typeparam>
/// <remarks>
/// This base class enables the data gateway pattern where the DataProvider can discover
/// and route commands to appropriate connection types based on datastore requirements.
/// Each connection type (MsSql, Oracle, PostgreSQL, etc.) inherits from this base to
/// provide metadata about what datastores it supports and how to create instances.
/// </remarks>
public abstract class ExternalConnectionServiceTypeBase<TService, TConfiguration>
    : ServiceTypeBase<TService, TConfiguration>
    where TService : class, IFdwService
    where TConfiguration : class, IFdwConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalConnectionServiceTypeBase{TService, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this connection service type.</param>
    /// <param name="name">The name of this connection service type.</param>
    /// <param name="description">The description of this connection service type.</param>
    protected ExternalConnectionServiceTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }

    /// <summary>
    /// Gets the datastore types supported by this connection provider.
    /// </summary>
    /// <value>An array of datastore identifiers this connection can handle.</value>
    /// <remarks>
    /// Examples: ["SqlServer", "MSSQL", "Microsoft SQL Server"] for SQL Server,
    /// ["PostgreSQL", "Postgres", "PostGres"] for PostgreSQL.
    /// Used by the DataProvider to route commands to appropriate connections
    /// based on configuration or command metadata.
    /// </remarks>
    public abstract string[] SupportedDataStores { get; }

    /// <summary>
    /// Gets the provider name for this connection type.
    /// </summary>
    /// <value>The technical name of the underlying provider or driver.</value>
    /// <remarks>
    /// Examples: "Microsoft.Data.SqlClient", "Npgsql", "Oracle.ManagedDataAccess".
    /// Used for diagnostics, logging, and provider-specific behavior.
    /// </remarks>
    public abstract string ProviderName { get; }

    /// <summary>
    /// Gets the connection modes supported by this provider.
    /// </summary>
    /// <value>A read-only list of supported connection modes.</value>
    /// <remarks>
    /// Common modes include:
    /// - "ReadWrite": Full read/write access
    /// - "ReadOnly": Read-only access for queries
    /// - "Bulk": Optimized for bulk operations
    /// - "Streaming": Supports streaming large datasets
    /// - "Transactional": Supports database transactions
    /// </remarks>
    public abstract IReadOnlyList<string> SupportedConnectionModes { get; }

    /// <summary>
    /// Gets the priority of this connection provider.
    /// </summary>
    /// <value>An integer representing selection priority (higher values = higher priority).</value>
    /// <remarks>
    /// When multiple connection providers support the same datastore,
    /// the DataProvider selects the one with the highest priority.
    /// Use this to prefer newer/better providers over legacy ones.
    /// Typical values: 0-100 (100 being highest priority).
    /// </remarks>
    public abstract int Priority { get; }
}