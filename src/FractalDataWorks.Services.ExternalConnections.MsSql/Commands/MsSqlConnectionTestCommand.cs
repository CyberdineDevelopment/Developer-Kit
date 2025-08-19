using System;
using System.Collections.Generic;
using FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;

using FractalDataWorks;
using System.Linq;
using FluentValidation.Results;
using FractalDataWorks.Services.ExternalConnections.Abstractions;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Commands;

/// <summary>
/// Command for testing SQL Server connection availability.
/// </summary>
public sealed class MsSqlConnectionTestCommand : IExternalConnectionCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlConnectionTestCommand"/> class.
    /// </summary>
    /// <param name="connectionName">The name of the connection to test.</param>
    /// <param name="timeout">The timeout for the connection test.</param>
    public MsSqlConnectionTestCommand(string connectionName, TimeSpan? timeout = null)
    {
        ConnectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
        Timeout = timeout;
    }

    /// <inheritdoc/>
    public string ConnectionName { get; }

    /// <inheritdoc/>
    public TimeSpan? Timeout { get; }
    
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