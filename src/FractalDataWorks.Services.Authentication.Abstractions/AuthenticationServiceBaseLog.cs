using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Authentication.Abstractions;

internal static partial class AuthenticationServiceBaseLog
{
    [LoggerMessage(1, LogLevel.Information, "Executing Authentication command {CommandType}")]
    public static partial void ExecutingCommand(ILogger logger, string commandType);
    
    [LoggerMessage(2, LogLevel.Warning, "Invalid Authentication command: {Reason}")]
    public static partial void InvalidCommand(ILogger logger, string reason);
    
    [LoggerMessage(3, LogLevel.Error, "Authentication command failed: {Error}")]
    public static partial void CommandFailed(ILogger logger, string error);
}