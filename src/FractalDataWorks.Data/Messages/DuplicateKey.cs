using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Data.Messages;

/// <summary>
/// Message for duplicate key errors.
/// </summary>
[EnumOption]
public class DuplicateKey : DataMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateKey"/> class.
    /// </summary>
    public DuplicateKey() 
        : base(2, "DuplicateKey", "DATA_002", "Duplicate key violation: {0} already exists", MessageSeverity.Error) { }
}
