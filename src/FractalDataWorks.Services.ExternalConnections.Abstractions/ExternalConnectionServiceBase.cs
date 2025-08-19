using Microsoft.Extensions.Logging;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions;

/// <summary>
/// Base class for external connection services.
/// </summary>
/// <typeparam name="TExternalConnectionCommand">The external connection command type.</typeparam>
/// <typeparam name="TExternalConnectionConfiguration">The external connection configuration type.</typeparam>
/// <typeparam name="TExternalConnectionService">The concrete external connection service type for logging category.</typeparam>
public abstract class ExternalConnectionServiceBase<TExternalConnectionCommand, TExternalConnectionConfiguration, TExternalConnectionService> 
    : ServiceBase<TExternalConnectionCommand, TExternalConnectionConfiguration, TExternalConnectionService>, IExternalConnectionService<TExternalConnectionCommand>
    where TExternalConnectionCommand : IExternalConnectionCommand
    where TExternalConnectionConfiguration : IExternalConnectionConfiguration
    where TExternalConnectionService : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalConnectionServiceBase{TExternalConnectionCommand, TExternalConnectionConfiguration, TExternalConnectionService}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for the concrete service type.</param>
    /// <param name="configuration">The external connection configuration.</param>
    protected ExternalConnectionServiceBase(ILogger<TExternalConnectionService> logger, TExternalConnectionConfiguration configuration) 
        : base(logger, configuration) 
    { 
    }
}