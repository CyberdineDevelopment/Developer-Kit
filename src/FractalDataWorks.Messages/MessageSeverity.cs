namespace FractalDataWorks;

/// <summary>
/// Defines the severity levels for framework messages.
/// </summary>
public enum MessageSeverity
{
    /// <summary>
    /// Informational messages that provide context or status updates.
    /// </summary>
    Information = 0,

    /// <summary>
    /// Warning messages that indicate potential issues but don't prevent operation.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error messages that indicate failures or critical problems.
    /// </summary>
    Error = 2,

    /// <summary>
    /// Critical messages that indicate system-level failures.
    /// </summary>
    Critical = 3
}