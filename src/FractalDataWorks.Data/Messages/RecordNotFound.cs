using FractalDataWorks.EnhancedEnums.Abstractions;
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
        : base("DATA_001", "Record not found: {0} with id '{1}'", MessageSeverity.Warning) { }
}
