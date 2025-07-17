using System;

namespace FractalDataWorks.Data;

/// <summary>
/// Base class for entities with GUID primary keys.
/// </summary>
public abstract class GuidEntityBase : EntityBase<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GuidEntityBase"/> class.
    /// </summary>
    protected GuidEntityBase()
    {
        Id = Guid.NewGuid();
    }
}