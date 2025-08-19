using System;
using System.Linq;
using System.Collections.Generic;
using FluentValidation.Results;
using FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;

using FractalDataWorks;
using FractalDataWorks.Services.ExternalConnections.Abstractions;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Commands;

/// <summary>
/// Command for managing SQL Server connections (list, remove, etc.).
/// </summary>
public sealed class MsSqlExternalConnectionManagementCommand : IExternalConnectionCommand, IExternalConnectionManagementCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlExternalConnectionManagementCommand"/> class.
    /// </summary>
    /// <param name="operation">The management operation to perform.</param>
    /// <param name="connectionName">The connection name (required for some operations).</param>
    public MsSqlExternalConnectionManagementCommand(ConnectionManagementOperation operation, string? connectionName = null)
    {
        Operation = operation;
        ConnectionName = connectionName;
    }

    /// <inheritdoc/>
    public ConnectionManagementOperation Operation { get; }

    /// <inheritdoc/>
    public string? ConnectionName { get; }


    /// <inheritdoc/>
    protected override IReadOnlyList<IValidationError> ValidateCore()
    {
        var errors = base.ValidateCore().ToList();

        // Operations that require connection name
        if ((Operation == ConnectionManagementOperation.RemoveConnection ||
             Operation == ConnectionManagementOperation.GetConnectionMetadata ||
             Operation == ConnectionManagementOperation.RefreshConnectionStatus) &&
            string.IsNullOrWhiteSpace(ConnectionName))
        {
            errors.Add(new SimpleValidationError($"Connection name is required for {Operation} operation.", nameof(ConnectionName)));
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
