using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;

namespace FractalDataWorks.Services.Transformations.Abstractions;

/// <summary>
/// Represents a transformation engine that can perform complex data transformations.
/// </summary>
public interface ITransformationEngine
{
    /// <summary>
    /// Gets the unique identifier for this transformation engine instance.
    /// </summary>
    string EngineId { get; }
    
    /// <summary>
    /// Gets the engine type name.
    /// </summary>
    string EngineType { get; }
    
    /// <summary>
    /// Gets a value indicating whether the engine is currently running.
    /// </summary>
    bool IsRunning { get; }
    
    /// <summary>
    /// Executes a transformation using this engine.
    /// </summary>
    /// <param name="request">The transformation request to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous transformation operation.</returns>
    Task<IFdwResult<ITransformationResult>> ExecuteTransformationAsync(ITransformationRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Starts the transformation engine.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    Task<IFdwResult> StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stops the transformation engine.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    Task<IFdwResult> StopAsync(CancellationToken cancellationToken = default);
}