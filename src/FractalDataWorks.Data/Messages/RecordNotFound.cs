using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Data.Messages;

/// <summary>
/// Message for record not found errors.
/// </summary>
[EnumOption]
public class RecordNotFound : DataMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecordNotFound"/> class.
    /// </summary>
    public RecordNotFound() 
        : base(1, "RecordNotFound", "DATA_001", "Record not found: {0} with id '{1}'", MessageSeverity.Warning) { }
}
