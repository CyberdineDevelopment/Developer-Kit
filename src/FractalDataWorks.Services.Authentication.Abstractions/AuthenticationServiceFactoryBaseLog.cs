using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Authentication.Abstractions;

internal static partial class AuthenticationServiceFactoryBaseLog
{
    [LoggerMessage(1, LogLevel.Information, "Creating Authentication service instance")]
    public static partial void CreatingService(ILogger logger);
    
    [LoggerMessage(2, LogLevel.Warning, "Authentication service creation failed: {Reason}")]
    public static partial void ServiceCreationFailed(ILogger logger, string reason);
    
    [LoggerMessage(3, LogLevel.Error, "Authentication service factory error: {Error}")]
    public static partial void FactoryError(ILogger logger, string error);
}