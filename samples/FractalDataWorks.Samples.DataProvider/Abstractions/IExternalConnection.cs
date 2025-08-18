using FractalDataWorks.Services.DataProvider.Abstractions;

namespace FractalDataWorks.Samples.DataProvider.Abstractions;

/// <summary>
/// External connection abstraction that translates IDataCommand to specific implementations
/// </summary>
public interface IExternalConnection : IDisposable
{
    /// <summary>
    /// Connection identifier/name
    /// </summary>
    string ConnectionName { get; }
    
    /// <summary>
    /// Connection type (SQL, File, API, etc.)
    /// </summary>
    string ConnectionType { get; }
    
    /// <summary>
    /// Indicates if the connection is currently open
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Opens the connection
    /// </summary>
    Task<bool> OpenAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Closes the connection
    /// </summary>
    Task CloseAsync();
    
    /// <summary>
    /// Executes a data command and returns results
    /// </summary>
    Task<TResult> ExecuteAsync<TResult>(IDataCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a data command with no return value
    /// </summary>
    Task ExecuteAsync(IDataCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Begins a transaction
    /// </summary>
    Task<IExternalTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tests the connection
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Transaction abstraction for external connections
/// </summary>
public interface IExternalTransaction : IDisposable
{
    /// <summary>
    /// Transaction ID
    /// </summary>
    Guid TransactionId { get; }
    
    /// <summary>
    /// Commits the transaction
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rolls back the transaction
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}