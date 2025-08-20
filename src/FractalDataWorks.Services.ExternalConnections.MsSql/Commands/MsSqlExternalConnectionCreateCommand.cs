using System;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;
using FractalDataWorks;
using FluentValidation.Results;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Commands;

/// <summary>
/// Command for creating new SQL Server connections.
/// </summary>
public sealed class MsSqlExternalConnectionCreateCommand : IExternalConnectionCommand, IExternalConnectionCreateCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlExternalConnectionCreateCommand"/> class.
    /// </summary>
    /// <param name="connectionName">The name for the new connection.</param>
    /// <param name="connectionConfiguration">The SQL Server configuration for the connection.</param>
    public MsSqlExternalConnectionCreateCommand(string connectionName, MsSqlConfiguration connectionConfiguration)
    {
        ConnectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
        ConnectionConfiguration = connectionConfiguration ?? throw new ArgumentNullException(nameof(connectionConfiguration));
    }

    /// <inheritdoc/>
    public string ConnectionName { get; }

    /// <inheritdoc/>
    public string ProviderType => "MsSql";

    /// <inheritdoc/>
    public IExternalConnectionConfiguration ConnectionConfiguration { get; }


    /// <summary>
    /// Validates this command using FluentValidation.
    /// </summary>
    /// <returns>The validation result.</returns>
    public ValidationResult Validate()
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(ConnectionName))
        {
            result.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(ConnectionName), "Connection name cannot be null or empty."));
        }

        if (ConnectionConfiguration is not MsSqlConfiguration)
        {
            result.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(ConnectionConfiguration), "Connection configuration must be MsSqlConfiguration."));
        }

        return result;
    }

    /// <summary>
    /// Gets the unique identifier for this command instance.
    /// </summary>
    public Guid CommandId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the correlation identifier for tracking related operations.
    /// </summary>
    public Guid CorrelationId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when this command was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the configuration associated with this command.
    /// </summary>
    public IFdwConfiguration? Configuration => ConnectionConfiguration as IFdwConfiguration;
}