using System;
using System.Collections.Generic;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;

using FractalDataWorks;
using System.Linq;
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


    /// <inheritdoc/>
    protected override IReadOnlyList<IValidationError> ValidateCore()
    {
        var errors = base.ValidateCore().ToList();

        if (string.IsNullOrWhiteSpace(ConnectionName))
        {
            errors.Add(new SimpleValidationError("Connection name cannot be null or empty.", nameof(ConnectionName)));
        }

        if (ConnectionConfiguration is not MsSqlConfiguration)
        {
            errors.Add(new SimpleValidationError("Connection configuration must be MsSqlConfiguration.", nameof(ConnectionConfiguration)));
        }

        return errors;
    }

    #region Implementation of ICommand

    /// <summary>
    /// Gets the unique identifier for this command instance.
    /// </summary>
    public Guid CommandId { get; }

    /// <summary>
    /// Gets the correlation identifier for tracking related operations.
    /// </summary>
    public Guid CorrelationId { get; }

    /// <summary>
    /// Gets the timestamp when this command was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the configuration associated with this command.
    /// </summary>
    public IFdwConfiguration? Configuration { get; }

    /// <summary>
    /// Validates this command.
    /// </summary>
    /// <returns>A task containing the validation result.</returns>
    public ValidationResult Validate()
    {
        throw new NotImplementedException();
    }

    #endregion
}