namespace FractalDataWorks;

/// <summary>
/// Priority levels for notifications.
/// </summary>
public enum NotificationPriority
{
    /// <summary>
    /// Low priority notifications (informational).
    /// </summary>
    Low = 1,
    
    /// <summary>
    /// Normal priority notifications.
    /// </summary>
    Normal = 2,
    
    /// <summary>
    /// High priority notifications (urgent).
    /// </summary>
    High = 3,
    
    /// <summary>
    /// Critical priority notifications (immediate attention required).
    /// </summary>
    Critical = 4
}