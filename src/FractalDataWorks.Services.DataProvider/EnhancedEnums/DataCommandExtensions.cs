using System;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.EnhancedEnums;

/// <summary>
/// Extension methods for enhancing DataCommands fluent syntax.
/// </summary>
public static class DataCommandExtensions
{
    /// <summary>
    /// Sets the connection name for a command using fluent syntax.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="command">The command instance.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>The command with the specified connection name.</returns>
    /// <example>
    /// <code>
    /// var customers = await dataProvider.Execute(
    ///     DataCommands.Query&lt;Customer&gt;(c => c.IsActive).WithConnection("ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static TCommand WithConnection<TCommand>(this TCommand command, string connectionName)
        where TCommand : DataCommandBase
    {
        return (TCommand)command.WithConnection(connectionName);
    }

    /// <summary>
    /// Sets the target container for a command using fluent syntax.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="command">The command instance.</param>
    /// <param name="containerPath">The container path to target.</param>
    /// <returns>The command with the specified target container.</returns>
    /// <example>
    /// <code>
    /// var customers = await dataProvider.Execute(
    ///     DataCommands.Query&lt;Customer&gt;(c => c.IsActive)
    ///         .WithConnection("ProductionDB")
    ///         .WithTarget(DataPath.Create(".", "sales", "customers"))
    /// );
    /// </code>
    /// </example>
    public static TCommand WithTarget<TCommand>(this TCommand command, DataPath containerPath)
        where TCommand : DataCommandBase
    {
        return (TCommand)command.WithTarget(containerPath);
    }

    /// <summary>
    /// Sets the timeout for a command using fluent syntax.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="command">The command instance.</param>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>The command with the specified timeout.</returns>
    /// <example>
    /// <code>
    /// var customers = await dataProvider.Execute(
    ///     DataCommands.Query&lt;Customer&gt;(c => c.IsActive)
    ///         .WithConnection("ProductionDB")
    ///         .WithTimeout(TimeSpan.FromMinutes(5))
    /// );
    /// </code>
    /// </example>
    public static TCommand WithTimeout<TCommand>(this TCommand command, TimeSpan timeout)
        where TCommand : DataCommandBase
    {
        return (TCommand)command.WithTimeout(timeout);
    }
}