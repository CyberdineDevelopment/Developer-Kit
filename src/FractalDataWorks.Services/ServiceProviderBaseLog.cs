using System;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services;

/// <summary>
/// High-performance logging methods for ServiceTypeProviderBase using source generators.
/// </summary>
public static partial class ServiceProviderBaseLog
{
    /// <summary>
    /// Logs when a service type name is invalid.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Service type name cannot be null or empty")]
    public static partial void InvalidServiceTypeName(ILogger logger);

    /// <summary>
    /// Logs when a service type is not found.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceTypeName">The name of the service type that was not found.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Service type '{ServiceTypeName}' not found")]
    public static partial void ServiceTypeNotFound(ILogger logger, string serviceTypeName);

    /// <summary>
    /// Logs when configuration retrieval fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceTypeName">The name of the service type.</param>
    /// <param name="error">The error message.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Failed to get configuration for service type '{ServiceTypeName}': {Error}")]
    public static partial void ConfigurationRetrievalFailed(ILogger logger, string serviceTypeName, string error);

    /// <summary>
    /// Logs when service creation fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceTypeName">The name of the service type.</param>
    /// <param name="error">The error message.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Failed to create service for type '{ServiceTypeName}': {Error}")]
    public static partial void ServiceCreationFailed(ILogger logger, string serviceTypeName, string error);

    /// <summary>
    /// Logs when a service is successfully created.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceTypeName">The name of the service type.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Successfully created service for type '{ServiceTypeName}'")]
    public static partial void ServiceCreated(ILogger logger, string serviceTypeName);

    /// <summary>
    /// Logs when a null service type is provided.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Error,
        Message = "Service type cannot be null")]
    public static partial void NullServiceType(ILogger logger);

    /// <summary>
    /// Logs when provider validation fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="reason">The reason for validation failure.</param>
    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Error,
        Message = "Provider validation failed: {Reason}")]
    public static partial void ProviderValidationFailed(ILogger logger, string reason);

    /// <summary>
    /// Logs when starting to retrieve all services.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="count">The number of service types to retrieve.</param>
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Debug,
        Message = "Starting to retrieve {Count} services")]
    public static partial void RetrievingAllServices(ILogger logger, int count);

    /// <summary>
    /// Logs when all services have been retrieved.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="successCount">The number of successfully created services.</param>
    /// <param name="failureCount">The number of failed service creations.</param>
    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Information,
        Message = "Retrieved all services: {SuccessCount} successful, {FailureCount} failed")]
    public static partial void AllServicesRetrieved(ILogger logger, int successCount, int failureCount);

    /// <summary>
    /// Logs when configuration is not found for a service type.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceTypeName">The name of the service type.</param>
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Warning,
        Message = "Configuration not found for service type '{ServiceTypeName}'")]
    public static partial void ConfigurationNotFound(ILogger logger, string serviceTypeName);
}