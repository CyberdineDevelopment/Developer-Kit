using System;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions;

/// <summary>
/// High-performance logging methods for ExternalConnectionProviderBase using source generators.
/// </summary>
public static partial class ExternalConnectionProviderBaseLog
{
    /// <summary>
    /// Logs when an empty data store name is provided.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Data store name cannot be empty or whitespace")]
    public static partial void EmptyDataStoreName(ILogger logger);

    /// <summary>
    /// Logs when a provider doesn't support a data store.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="providerName">The provider name.</param>
    /// <param name="dataStore">The unsupported data store.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Provider '{ProviderName}' does not support data store '{DataStore}'")]
    public static partial void UnsupportedDataStore(ILogger logger, string providerName, string dataStore);

    /// <summary>
    /// Logs when a provider doesn't support a connection mode.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="providerName">The provider name.</param>
    /// <param name="connectionMode">The unsupported connection mode.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Provider '{ProviderName}' does not support connection mode '{ConnectionMode}'")]
    public static partial void UnsupportedConnectionMode(ILogger logger, string providerName, string connectionMode);
}