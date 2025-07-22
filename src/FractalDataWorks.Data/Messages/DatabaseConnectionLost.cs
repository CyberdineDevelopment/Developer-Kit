using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Data.Messages;

/// <summary>
/// Message for database connection lost errors.
/// </summary>
[EnumOption]
public class DatabaseConnectionLost : DataMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseConnectionLost"/> class.
    /// </summary>
    public DatabaseConnectionLost() 
        : base(3, "DatabaseConnectionLost", "DATA_003", "Database connection lost: {0}", MessageSeverity.Critical) { }
}
