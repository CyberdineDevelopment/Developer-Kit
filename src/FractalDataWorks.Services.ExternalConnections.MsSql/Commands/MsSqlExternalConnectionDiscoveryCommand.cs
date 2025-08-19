using System;
using System.Collections.Generic;
using FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;

using FractalDataWorks;
using System.Linq;
using FluentValidation.Results;
using FractalDataWorks.Services.ExternalConnections.Abstractions;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Commands;

/// <summary>
/// Command for discovering SQL Server connection schemas and metadata.
/// </summary>
public sealed class MsSqlExternalConnectionDiscoveryCommand : IExternalConnectionCommand, IExternalConnectionDiscoveryCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlExternalConnectionDiscoveryCommand"/> class.
    /// </summary>
    /// <param name="connectionName">The name of the connection to discover.</param>
    /// <param name="startPath">The starting path for schema discovery.</param>
    /// <param name="options">The discovery options.</param>
    public MsSqlExternalConnectionDiscoveryCommand(
        string connectionName, 
        string? startPath = null, 
        ConnectionDiscoveryOptions? options = null)
    {
        ConnectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
        StartPath = startPath;
        Options = options ?? new ConnectionDiscoveryOptions();
    }

    /// <inheritdoc/>
    public string ConnectionName { get; }

    /// <inheritdoc/>
    public string? StartPath { get; }

    /// <inheritdoc/>
    public ConnectionDiscoveryOptions Options { get; }


    /// <inheritdoc/>
    protected override IReadOnlyList<IValidationError> ValidateCore()
    {
        var errors = base.ValidateCore().ToList();

        if (string.IsNullOrWhiteSpace(ConnectionName))
        {
            errors.Add(new SimpleValidationError("Connection name cannot be null or empty.", nameof(ConnectionName)));
        }

        if (Options.MaxDepth < 0)
        {
            errors.Add(new SimpleValidationError("Max depth cannot be negative.", nameof(Options.MaxDepth)));
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