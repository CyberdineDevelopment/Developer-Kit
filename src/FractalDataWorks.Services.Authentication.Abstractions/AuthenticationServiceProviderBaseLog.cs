using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Authentication.Abstractions;

internal static partial class AuthenticationServiceProviderBaseLog
{
    [LoggerMessage(1, LogLevel.Information, "Providing Authentication service {ServiceName}")]
    public static partial void ProvidingService(ILogger logger, string serviceName);
    
    [LoggerMessage(2, LogLevel.Warning, "Authentication service provider warning: {Warning}")]
    public static partial void ProviderWarning(ILogger logger, string warning);
    
    [LoggerMessage(3, LogLevel.Error, "Authentication service provider error: {Error}")]
    public static partial void ProviderError(ILogger logger, string error);
}