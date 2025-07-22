using System;
using System.Collections.Generic;
using System.Text;

namespace FractalDataWorks.Services;

/// <summary>
/// Base implementation of the service factory.
/// </summary>
public abstract class ServiceFactoryBase : IServiceFactory
{
    /// <inheritdoc/>
    public abstract IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwService;

    /// <inheritdoc/>
    public abstract IFdwResult<IFdwService> Create(IFdwConfiguration configuration);
}
