using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Services;

/// <summary>
/// Interface for the main data provider service that routes commands to named connections.
/// </summary>
/// <remarks>
/// IDataProvider serves as the consumer-facing entry point for all data operations within
/// the FractalDataWorks framework. It accepts provider-agnostic DataCommandBase commands
/// and routes them to the appropriate external data connections based on the ConnectionName
/// property of each command.
/// 
/// This service abstracts away the complexity of managing multiple data connections and
/// provides a unified interface for data operations across different data stores such as
/// SQL databases, NoSQL databases, file systems, REST APIs, and other external systems.
/// </remarks>
public interface IDataProvider : IFdwService<DataCommandBase>
{
    /// <summary>
    /// Executes a data command against the specified connection and returns a typed result.
    /// </summary>
    /// <typeparam name="T">The expected return type of the command execution.</typeparam>
    /// <param name="command">The data command to execute containing the connection name and operation details.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult&lt;T&gt; with the command execution result or error information.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
    /// <remarks>
    /// This method:
    /// 1. Validates the incoming command
    /// 2. Routes the command to the appropriate external connection based on command.ConnectionName
    /// 3. Executes the command against the target data store
    /// 4. Returns the typed result or error information
    /// 
    /// The method supports all command types defined in the DataCommandBase hierarchy
    /// including queries, inserts, updates, deletes, and bulk operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Execute a query command
    /// var customers = await dataProvider.Execute&lt;List&lt;Customer&gt;&gt;(
    ///     DataCommands.Query&lt;Customer&gt;(c =&gt; c.IsActive).WithConnection("ProductionDB"),
    ///     cancellationToken
    /// );
    /// 
    /// // Execute an insert command
    /// var insertResult = await dataProvider.Execute&lt;int&gt;(
    ///     DataCommands.Insert(newCustomer).WithConnection("ProductionDB"),
    ///     cancellationToken
    /// );
    /// </code>
    /// </example>
    Task<IFdwResult<T>> Execute<T>(DataCommandBase command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discovers the schema structure of a data store starting from an optional path.
    /// </summary>
    /// <param name="connectionName">The name of the connection to discover schema for.</param>
    /// <param name="startPath">Optional starting path for schema discovery. If null, discovers from root.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult with a collection of discovered data containers and their metadata.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when connectionName is null or empty.</exception>
    /// <remarks>
    /// This method enables runtime discovery of available data containers within a data store.
    /// The behavior varies by provider type:
    /// 
    /// - SQL Databases: Discovers databases, schemas, tables, views, and stored procedures
    /// - File Systems: Discovers directories and files with metadata
    /// - REST APIs: Discovers available endpoints and their schemas
    /// - NoSQL Databases: Discovers collections, indexes, and document structures
    /// 
    /// The startPath parameter allows for targeted discovery of specific portions of the
    /// schema hierarchy, improving performance when full schema discovery is not needed.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Discover full schema for a SQL Server connection
    /// var schemaResult = await dataProvider.DiscoverSchema("ProductionDB");
    /// 
    /// // Discover schema starting from a specific database
    /// var salesSchema = await dataProvider.DiscoverSchema(
    ///     "ProductionDB", 
    ///     DataPath.Create(".", "SalesDB")
    /// );
    /// </code>
    /// </example>
    Task<IFdwResult<IEnumerable<DataContainer>>> DiscoverSchema(
        string connectionName, 
        DataPath? startPath = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata about the available connections and their capabilities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult with a dictionary of connection names and their metadata.
    /// </returns>
    /// <remarks>
    /// This method provides information about all configured connections including:
    /// - Connection status (available/unavailable)
    /// - Supported operations and capabilities
    /// - Provider type and version information
    /// - Performance characteristics and limitations
    /// 
    /// This information can be used for connection health monitoring, capability
    /// discovery, and dynamic connection selection.
    /// </remarks>
    /// <example>
    /// <code>
    /// var connectionsInfo = await dataProvider.GetConnectionsInfo();
    /// if (connectionsInfo.IsSuccess)
    /// {
    ///     foreach (var connection in connectionsInfo.Value)
    ///     {
    ///         Console.WriteLine($"Connection: {connection.Key}, Status: {connection.Value.IsAvailable}");
    ///     }
    /// }
    /// </code>
    /// </example>
    Task<IFdwResult<IDictionary<string, object>>> GetConnectionsInfo(CancellationToken cancellationToken = default);
}