namespace FractalDataWorks;

/// <summary>
/// Base interface for all FDW services.
/// </summary>
public interface IFdwService
{
    /// <summary>
    /// Gets the name of this service.
    /// </summary>
    string ServiceName { get; }
    
    /// <summary>
    /// Gets a value indicating whether this service is currently running.
    /// </summary>
    bool IsRunning { get; }
}
