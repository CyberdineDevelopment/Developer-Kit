using System;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.ExternalConnections.MsSql;

/// <summary>
/// Factory for creating MsSql external connection services.
/// </summary>
public sealed class MsSqlConnectionFactory : ServiceFactoryBase<MsSqlExternalConnectionService, MsSqlConfiguration>
{
    private readonly ILoggerFactory _loggerFactory;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlConnectionFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="loggerFactory">The logger factory for creating service loggers.</param>
    public MsSqlConnectionFactory(ILogger<MsSqlConnectionFactory>? logger, ILoggerFactory loggerFactory)
        : base(logger)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }
    
    /// <summary>
    /// Parameterless constructor for Enhanced Enum creation.
    /// </summary>
    public MsSqlConnectionFactory()
        : base(null)
    {
        _loggerFactory = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
    }
    
    /// <inheritdoc/>
    protected override IFdwResult<MsSqlExternalConnectionService> CreateCore(MsSqlConfiguration configuration)
    {
        if (configuration == null)
        {
            return FdwResult<MsSqlExternalConnectionService>.Failure("Configuration cannot be null");
        }
        
        try
        {
            Logger.LogDebug("Creating MsSql external connection service");
            
            var service = new MsSqlExternalConnectionService(
                _loggerFactory.CreateLogger<MsSqlExternalConnectionService>(),
                _loggerFactory,
                configuration);
            
            Logger.LogInformation("Successfully created MsSql external connection service");
            return FdwResult<MsSqlExternalConnectionService>.Success(service);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create MsSql external connection service");
            return FdwResult<MsSqlExternalConnectionService>.Failure($"Failed to create service: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public override Task<MsSqlExternalConnectionService> GetService(string configurationName)
    {
        // This method would typically load configuration by name from a configuration provider
        // For now, we'll return a not supported exception as this requires a configuration provider
        throw new NotSupportedException("Configuration name-based service creation requires a configuration provider. Use CreateCore with a configuration instance instead.");
    }

    /// <inheritdoc/>
    public override Task<MsSqlExternalConnectionService> GetService(int configurationId)
    {
        // This method would typically load configuration by ID from a configuration provider  
        // For now, we'll return a not supported exception as this requires a configuration provider
        throw new NotSupportedException("Configuration ID-based service creation requires a configuration provider. Use CreateCore with a configuration instance instead.");
    }
}