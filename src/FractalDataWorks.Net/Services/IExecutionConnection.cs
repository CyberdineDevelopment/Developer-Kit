using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Data;

namespace FractalDataWorks.Services;

/// <summary>
/// Connection interface - connections "execute" operations.
/// More specific than IProcessingService.
/// </summary>
public interface IPlatformConnection : IProcessingService
{
    /// <summary>
    /// Executes an operation.
    /// More specific than Process - indicates this connection executes operations.
    /// </summary>
    /// <typeparam name="T">The expected result type</typeparam>
    /// <param name="operation">The operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>IGenericResult of the execution</returns>
    Task<IGenericResult<T>> Execute<T>(IDataOperation operation, CancellationToken cancellationToken = default);
}

/// <summary>
/// Connection interface with strongly-typed configuration
/// </summary>
/// <typeparam name="TConfiguration">The connection configuration type</typeparam>
public interface IExecutionConnection<TConfiguration> : IPlatformConnection, IProcessingService<TConfiguration>
    where TConfiguration : IConfigurationBase
{
    /// <summary>
    /// Gets the connection configuration
    /// </summary>
    new TConfiguration Configuration { get; }
}
