using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FractalDataWorks.Services.ExternalConnections.Abstractions;

namespace FractalDataWorks.Services.DataProviders.Abstractions;

/// <summary>
/// Interface for data providers in the FractalDataWorks framework.
/// Provides a unified interface for executing data commands against various data sources.
/// </summary>
/// <remarks>
/// Data providers abstract the complexity of different data access technologies
/// and provide a consistent command execution interface. They handle connection management,
/// transaction coordination, and result processing for data operations.
/// </remarks>
public interface IDataProvider : IFdwService
{
    /// <summary>
    /// Gets the types of data commands this provider can execute.
    /// </summary>
    /// <value>A collection of command type names supported by this provider.</value>
    /// <remarks>
    /// Supported command types help the framework route commands to appropriate providers.
    /// Common command types include "Query", "Insert", "Update", "Delete", "StoredProcedure",
    /// "BulkInsert", "Stream". Providers may support subset of these or custom types.
    /// </remarks>
    IReadOnlyList<string> SupportedCommandTypes { get; }
    
    /// <summary>
    /// Gets the data sources this provider can work with.
    /// </summary>
    /// <value>A collection of data source identifiers supported by this provider.</value>
    /// <remarks>
    /// Data sources identify the specific databases, APIs, or data stores this provider
    /// can access. This enables the framework to match providers with data access requirements.
    /// </remarks>
    IReadOnlyList<string> SupportedDataSources { get; }
    
    /// <summary>
    /// Gets the external connection used by this provider.
    /// </summary>
    /// <value>The external connection instance, or null if not connection-based.</value>
    /// <remarks>
    /// Many data providers rely on external connections to access data sources.
    /// This property provides access to the underlying connection for advanced scenarios
    /// or connection state monitoring.
    /// </remarks>
    IExternalConnection? Connection { get; }
    
    /// <summary>
    /// Executes a data command and returns the result.
    /// </summary>
    /// <param name="command">The data command to execute.</param>
    /// <returns>
    /// A task representing the asynchronous command execution operation.
    /// The result contains the command execution outcome and any returned data.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the provider is not available or the command type is not supported.
    /// </exception>
    /// <remarks>
    /// This is the primary method for data access in the framework. It handles
    /// command validation, execution, and result processing. The provider is responsible
    /// for managing connections, transactions, and error handling during execution.
    /// </remarks>
    Task<IFdwResult<object?>> Execute(IDataCommand command) where IDataCommand : notnull;
    
    /// <summary>
    /// Executes a typed data command and returns the strongly-typed result.
    /// </summary>
    /// <typeparam name="TResult">The expected type of the command result.</typeparam>
    /// <param name="command">The typed data command to execute.</param>
    /// <returns>
    /// A task representing the asynchronous command execution operation.
    /// The result contains the strongly-typed command execution outcome.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the provider is not available or the command type is not supported.
    /// </exception>
    /// <remarks>
    /// This method provides type-safe command execution for scenarios where the
    /// result type is known at compile time. It eliminates the need for runtime
    /// type checking and casting.
    /// </remarks>
    Task<IFdwResult<TResult>> Execute<TResult>(IDataCommand<TResult> command) where IDataCommand<TResult> : notnull;
    
    /// <summary>
    /// Validates whether this provider can execute the specified command.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>A result indicating whether the command can be executed by this provider.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <remarks>
    /// This method enables command routing and provider selection based on command
    /// requirements. It performs lightweight validation without executing the command.
    /// </remarks>
    IFdwResult ValidateCommand(IDataCommand command) where IDataCommand : notnull;
    
    /// <summary>
    /// Gets performance metrics for this provider.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous metrics collection operation.
    /// The result contains provider performance metrics if available.
    /// </returns>
    /// <remarks>
    /// Performance metrics help monitor provider health and optimize data access patterns.
    /// Metrics may include execution times, connection pool status, error rates, etc.
    /// </remarks>
    Task<IFdwResult<IProviderMetrics>> GetMetricsAsync();
    
    /// <summary>
    /// Begins a transaction scope for executing multiple commands atomically.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous transaction creation operation.
    /// The result contains the transaction scope if successful.
    /// </returns>
    /// <remarks>
    /// Transaction scopes enable atomic execution of multiple data commands.
    /// Not all providers support transactions - check provider capabilities before use.
    /// </remarks>
    Task<IFdwResult<IDataTransaction>> BeginTransactionAsync();
    
    /// <summary>
    /// Executes multiple commands as a batch operation.
    /// </summary>
    /// <param name="commands">The collection of commands to execute as a batch.</param>
    /// <returns>
    /// A task representing the asynchronous batch execution operation.
    /// The result contains the outcomes of all commands in the batch.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commands"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="commands"/> is empty.</exception>
    /// <remarks>
    /// Batch execution can provide significant performance improvements for multiple operations.
    /// The provider handles optimization and may execute commands in parallel or as a single batch.
    /// Individual command failures are reported in the batch result.
    /// </remarks>
    Task<IFdwResult<IBatchResult>> ExecuteBatch(IReadOnlyList<IDataCommand> commands);
}

/// <summary>
/// Generic interface for data providers with typed configuration.
/// Extends the base provider interface with configuration-specific functionality.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration this provider requires.</typeparam>
/// <remarks>
/// Use this interface for providers that require specific configuration beyond
/// basic connection information. Provides type safety for provider configuration.
/// </remarks>
public interface IDataProvider<TConfiguration> : IDataProvider, IFdwService<TConfiguration>
    where TConfiguration : FdwConfigurationBase
{
    /// <summary>
    /// Gets the configuration object used by this provider.
    /// </summary>
    /// <value>The provider configuration object.</value>
    /// <remarks>
    /// This property provides access to the typed configuration used to initialize
    /// and configure the data provider instance.
    /// </remarks>
    new TConfiguration? Configuration { get; }
}