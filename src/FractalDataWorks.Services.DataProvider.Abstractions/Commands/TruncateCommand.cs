using System;
using System.Collections.Generic;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Commands;

/// <summary>
/// Represents a provider-agnostic truncate command for removing all records from a container.
/// </summary>
/// <typeparam name="TEntity">The type of entity to truncate.</typeparam>
public sealed class TruncateCommand<TEntity> : DataCommandBase<int>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TruncateCommand{TEntity}"/> class.
    /// </summary>
    /// <param name="connectionName">The named connection to execute against.</param>
    /// <param name="targetContainer">The target container path.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    public TruncateCommand(
        string connectionName,
        DataPath? targetContainer = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Truncate", connectionName, targetContainer, parameters, metadata, timeout)
    {
    }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <summary>
    /// Creates a new TruncateCommand with confirmation required.
    /// </summary>
    /// <param name="confirmed">Whether the truncation is confirmed.</param>
    /// <returns>A new TruncateCommand instance with confirmation.</returns>
    public TruncateCommand<TEntity> Confirm(bool confirmed = true)
    {
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["Confirmed"] = confirmed
        };
        
        return new TruncateCommand<TEntity>(
            ConnectionName, 
            TargetContainer, 
            Parameters, 
            newMetadata, 
            Timeout);
    }

    /// <inheritdoc/>
    protected override DataCommandBase CreateCopy(
        string connectionName,
        DataPath? targetContainer,
        IReadOnlyDictionary<string, object?> parameters,
        IReadOnlyDictionary<string, object> metadata,
        TimeSpan? timeout)
    {
        return new TruncateCommand<TEntity>(
            connectionName,
            targetContainer,
            parameters,
            metadata,
            timeout);
    }

    /// <summary>
    /// Returns a string representation of the truncate command.
    /// </summary>
    /// <returns>A string describing the truncate command.</returns>
    public override string ToString()
    {
        var entityName = typeof(TEntity).Name;
        var target = TargetContainer != null ? $" {TargetContainer}" : $" {entityName}";
        var confirmed = TryGetMetadata<bool>("Confirmed", out var isConfirmed) && isConfirmed ? " (confirmed)" : " (unconfirmed)";
        
        return $"Truncate<{entityName}>({ConnectionName}){target}{confirmed}";
    }
}