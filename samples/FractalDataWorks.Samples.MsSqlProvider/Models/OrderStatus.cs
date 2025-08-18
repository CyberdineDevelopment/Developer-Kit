namespace FractalDataWorks.Samples.MsSqlProvider.Models;

/// <summary>
/// Represents the status of an order in the system.
/// Values match the database check constraint in sales.Orders table.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order has been created but not yet processed
    /// </summary>
    Pending,
    
    /// <summary>
    /// Order is currently being processed
    /// </summary>
    Processing,
    
    /// <summary>
    /// Order has been shipped to customer
    /// </summary>
    Shipped,
    
    /// <summary>
    /// Order has been delivered to customer
    /// </summary>
    Delivered,
    
    /// <summary>
    /// Order has been cancelled
    /// </summary>
    Cancelled
}