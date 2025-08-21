using System;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.ExternalConnections.Http;

/// <summary>
/// Factory for creating HTTP external connection services.
/// </summary>
public sealed class HttpConnectionFactory : ServiceFactoryBase<HttpExternalConnectionService, HttpConnectionConfiguration>
{
    private readonly ILoggerFactory _loggerFactory;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpConnectionFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="loggerFactory">The logger factory for creating service loggers.</param>
    public HttpConnectionFactory(ILogger<HttpConnectionFactory>? logger, ILoggerFactory loggerFactory)
        : base(logger)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }
    
    /// <summary>
    /// Parameterless constructor for Enhanced Enum creation.
    /// </summary>
    public HttpConnectionFactory()
        : base(null)
    {
        _loggerFactory = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
    }
    
    /// <inheritdoc/>
    protected override IFdwResult<HttpExternalConnectionService> CreateCore(HttpConnectionConfiguration configuration)
    {
        if (configuration == null)
        {
            return FdwResult<HttpExternalConnectionService>.Failure("Configuration cannot be null");
        }
        
        try
        {
            Logger.LogDebug("Creating HTTP external connection service");
            
            var service = new HttpExternalConnectionService(
                _loggerFactory.CreateLogger<HttpExternalConnectionService>(),
                _loggerFactory,
                configuration);
            
            Logger.LogInformation("Successfully created HTTP external connection service");
            return FdwResult<HttpExternalConnectionService>.Success(service);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create HTTP external connection service");
            return FdwResult<HttpExternalConnectionService>.Failure($"Failed to create service: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public override Task<HttpExternalConnectionService> GetService(string configurationName)
    {
        // This method would typically load configuration by name from a configuration provider
        // For now, we'll return a not supported exception as this requires a configuration provider
        throw new NotSupportedException("Configuration name-based service creation requires a configuration provider. Use CreateCore with a configuration instance instead.");
    }

    /// <inheritdoc/>
    public override Task<HttpExternalConnectionService> GetService(int configurationId)
    {
        // This method would typically load configuration by ID from a configuration provider  
        // For now, we'll return a not supported exception as this requires a configuration provider
        throw new NotSupportedException("Configuration ID-based service creation requires a configuration provider. Use CreateCore with a configuration instance instead.");
    }
}
