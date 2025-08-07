using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Authentication.Abstractions;

internal static partial class AuthenticationServiceTypeBaseLog
{
    [LoggerMessage(1, LogLevel.Information, "Creating Authentication service type {ServiceTypeName}")]
    public static partial void CreatingServiceType(ILogger logger, string serviceTypeName);
    
    [LoggerMessage(2, LogLevel.Warning, "Authentication service type creation failed: {Reason}")]
    public static partial void ServiceTypeCreationFailed(ILogger logger, string reason);
    
    [LoggerMessage(3, LogLevel.Error, "Authentication service type error: {Error}")]
    public static partial void ServiceTypeError(ILogger logger, string error);
}