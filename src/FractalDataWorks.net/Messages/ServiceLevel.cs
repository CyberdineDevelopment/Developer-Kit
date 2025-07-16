namespace FractalDataWorks.Messages;

/// <summary>
/// Defines the severity levels for service messages.
/// </summary>
public enum ServiceLevel
{
    /// <summary>
    /// Debug level - detailed diagnostic information.
    /// </summary>
    Debug,

    /// <summary>
    /// Information level - general informational messages.
    /// </summary>
    Information,

    /// <summary>
    /// Warning level - indicates a potential issue.
    /// </summary>
    Warning,

    /// <summary>
    /// Error level - indicates a failure.
    /// </summary>
    Error,

    /// <summary>
    /// Critical level - indicates a critical failure requiring immediate attention.
    /// </summary>
    Critical
}