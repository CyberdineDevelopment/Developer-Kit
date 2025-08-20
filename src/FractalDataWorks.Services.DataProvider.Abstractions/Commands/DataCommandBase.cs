using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;
using FluentValidation.Results;


namespace FractalDataWorks.Services.DataProvider.Abstractions.Commands;

/// <summary>
/// Base class for all data commands providing provider-agnostic data operations.
/// </summary>
/// <remarks>
/// DataCommandBase represents universal data operations that can be translated to provider-specific
/// implementations. This enables the same LINQ expressions and commands to work across SQL databases,
/// file systems, REST APIs, and other data sources.
/// </remarks>
public abstract class DataCommandBase : IDataCommand
{
    private readonly Dictionary<string, object?> _parameters;
    private readonly Dictionary<string, object> _metadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandBase"/> class.
    /// </summary>
    /// <param name="commandType">The type of command (Query, Insert, Update, Delete, Upsert).</param>
    /// <param name="connectionName">The named connection to execute this command against.</param>
    /// <param name="targetContainer">The target data container path.</param>
    /// <param name="expectedResultType">The expected result type.</param>
    /// <param name="parameters">Command parameters.</param>
    /// <param name="metadata">Additional command metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    protected DataCommandBase(
        string commandType,
        string connectionName,
        DataPath? targetContainer,
        Type expectedResultType,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
    {
        if (string.IsNullOrWhiteSpace(commandType))
            throw new ArgumentException("Command type cannot be null or empty.", nameof(commandType));
        
        if (string.IsNullOrWhiteSpace(connectionName))
            throw new ArgumentException("Connection name cannot be null or empty.", nameof(connectionName));
        
        CommandType = commandType;
        ConnectionName = connectionName;
        TargetContainer = targetContainer;
        ExpectedResultType = expectedResultType ?? throw new ArgumentNullException(nameof(expectedResultType));
        Timeout = timeout;
        
        _parameters = parameters != null 
            ? new Dictionary<string, object?>(parameters, StringComparer.Ordinal)
            : new Dictionary<string, object?>(StringComparer.Ordinal);
        
        _metadata = metadata != null 
            ? new Dictionary<string, object>(metadata, StringComparer.Ordinal)
            : new Dictionary<string, object>(StringComparer.Ordinal);

        CorrelationId = Guid.NewGuid();
        CommandId = Guid.NewGuid();
    }

    /// <inheritdoc/>
    public string CommandType { get; }

    /// <summary>
    /// Gets the named connection this command should execute against.
    /// </summary>
    public string ConnectionName { get; }

    /// <summary>
    /// Gets the target data container path for this command.
    /// </summary>
    public DataPath? TargetContainer { get; }

    /// <inheritdoc/>
    public string? Target => TargetContainer?.ToString();

    /// <inheritdoc/>
    public Type ExpectedResultType { get; }

    /// <inheritdoc/>
    public TimeSpan? Timeout { get; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> Parameters => _parameters;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;

    /// <inheritdoc/>
    public abstract bool IsDataModifying { get; }

    /// <inheritdoc/>
    public Guid CorrelationId { get; }

    /// <inheritdoc/>
    public Guid CommandId { get; }

    /// <inheritdoc/>
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

    /// <inheritdoc/>
    public IFdwConfiguration? Configuration => null;

    /// <summary>
    /// Creates a new command with the specified connection name.
    /// </summary>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>A new command instance with the specified connection name.</returns>
    public DataCommandBase WithConnection(string connectionName)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
            throw new ArgumentException("Connection name cannot be null or empty.", nameof(connectionName));
        
        return CreateCopy(connectionName, TargetContainer, Parameters, Metadata, Timeout);
    }

    /// <summary>
    /// Creates a new command with the specified target container.
    /// </summary>
    /// <param name="targetContainer">The target container path.</param>
    /// <returns>A new command instance with the specified target container.</returns>
    public DataCommandBase WithTarget(DataPath targetContainer)
    {
        if (targetContainer == null)
            throw new ArgumentNullException(nameof(targetContainer));
        
        return CreateCopy(ConnectionName, targetContainer, Parameters, Metadata, Timeout);
    }

    /// <summary>
    /// Creates a new command with the specified timeout.
    /// </summary>
    /// <param name="timeout">The timeout to use.</param>
    /// <returns>A new command instance with the specified timeout.</returns>
    public DataCommandBase WithTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentException("Timeout must be positive.", nameof(timeout));
        
        return CreateCopy(ConnectionName, TargetContainer, Parameters, Metadata, timeout);
    }

    /// <inheritdoc/>
    public virtual ValidationResult Validate()
    {
        var errors = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(CommandType))
        {
            errors.Add(new ValidationFailure(nameof(CommandType), "Command type cannot be null or empty."));
        }

        if (string.IsNullOrWhiteSpace(ConnectionName))
        {
            errors.Add(new ValidationFailure(nameof(ConnectionName), "Connection name cannot be null or empty."));
        }

        if (ExpectedResultType == null)
        {
            errors.Add(new ValidationFailure(nameof(ExpectedResultType), "Expected result type cannot be null."));
        }

        if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
        {
            errors.Add(new ValidationFailure(nameof(Timeout), "Timeout must be positive if specified."));
        }

        // Validate parameters don't contain null keys
        foreach (var parameter in Parameters)
        {
            if (string.IsNullOrWhiteSpace(parameter.Key))
            {
                errors.Add(new ValidationFailure(nameof(Parameters), "Parameter keys cannot be null or empty."));
                break;
            }
        }

        return new ValidationResult(errors);
    }
    
    /// <inheritdoc/>
    public virtual IDataCommand WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        if (newParameters == null)
            throw new ArgumentNullException(nameof(newParameters));
        
        return CreateCopy(ConnectionName, TargetContainer, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    public virtual IDataCommand WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        if (newMetadata == null)
            throw new ArgumentNullException(nameof(newMetadata));
        
        return CreateCopy(ConnectionName, TargetContainer, Parameters, newMetadata, Timeout);
    }

    /// <summary>
    /// Creates a copy of this command with new values.
    /// </summary>
    /// <param name="connectionName">The connection name.</param>
    /// <param name="targetContainer">The target container.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="metadata">The metadata.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>A new command instance.</returns>
    protected abstract DataCommandBase CreateCopy(
        string connectionName,
        DataPath? targetContainer,
        IReadOnlyDictionary<string, object?> parameters,
        IReadOnlyDictionary<string, object> metadata,
        TimeSpan? timeout);

    /// <summary>
    /// Gets a parameter value by name.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="name">The parameter name.</param>
    /// <returns>The parameter value converted to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the parameter is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the specified type.</exception>
    protected T? GetParameter<T>(string name)
    {
        if (!_parameters.TryGetValue(name, out var value))
            throw new KeyNotFoundException($"Parameter '{name}' not found.");
        
        if (value == null)
            return default(T);
        
        if (value is T directValue)
            return directValue;
        
        try
        {
            return (T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            throw new InvalidCastException($"Cannot convert parameter '{name}' value from {value.GetType().Name} to {typeof(T).Name}.", ex);
        }
    }

    /// <summary>
    /// Tries to get a parameter value by name.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value if found and converted successfully.</param>
    /// <returns>True if the parameter was found and converted successfully; otherwise, false.</returns>
    protected bool TryGetParameter<T>(string name, out T? value)
    {
        try
        {
            value = GetParameter<T>(name);
            return true;
        }
        catch
        {
            value = default(T);
            return false;
        }
    }

    /// <summary>
    /// Gets a metadata value by name.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="name">The metadata name.</param>
    /// <returns>The metadata value converted to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the metadata is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the specified type.</exception>
    protected T GetMetadata<T>(string name)
    {
        if (!_metadata.TryGetValue(name, out var value))
            throw new KeyNotFoundException($"Metadata '{name}' not found.");
        
        if (value is T directValue)
            return directValue;
        
        try
        {
            return (T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            throw new InvalidCastException($"Cannot convert metadata '{name}' value from {value?.GetType().Name ?? "null"} to {typeof(T).Name}.", ex);
        }
    }

    /// <summary>
    /// Tries to get a metadata value by name.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="name">The metadata name.</param>
    /// <param name="value">The metadata value if found and converted successfully.</param>
    /// <returns>True if the metadata was found and converted successfully; otherwise, false.</returns>
    protected bool TryGetMetadata<T>(string name, out T? value)
    {
        try
        {
            value = GetMetadata<T>(name);
            return true;
        }
        catch
        {
            value = default(T);
            return false;
        }
    }

    /// <summary>
    /// Returns a string representation of the command.
    /// </summary>
    /// <returns>A string describing the command.</returns>
    public override string ToString()
    {
        var target = TargetContainer != null ? $" -> {TargetContainer}" : "";
        return $"{CommandType}({ConnectionName}){target}";
    }
}

/// <summary>
/// Generic base class for data commands with typed result expectations.
/// </summary>
/// <typeparam name="TResult">The type of result expected from command execution.</typeparam>
public abstract class DataCommandBase<TResult> : DataCommandBase, IDataCommand<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandBase{TResult}"/> class.
    /// </summary>
    /// <param name="commandType">The type of command.</param>
    /// <param name="connectionName">The named connection.</param>
    /// <param name="targetContainer">The target container.</param>
    /// <param name="parameters">Command parameters.</param>
    /// <param name="metadata">Command metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    protected DataCommandBase(
        string commandType,
        string connectionName,
        DataPath? targetContainer = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base(commandType, connectionName, targetContainer, typeof(TResult), parameters, metadata, timeout)
    {
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        if (newParameters == null)
            throw new ArgumentNullException(nameof(newParameters));
        
        return (IDataCommand<TResult>)CreateCopy(ConnectionName, TargetContainer, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        if (newMetadata == null)
            throw new ArgumentNullException(nameof(newMetadata));
        
        return (IDataCommand<TResult>)CreateCopy(ConnectionName, TargetContainer, Parameters, newMetadata, Timeout);
    }

    /// <summary>
    /// Creates a new typed command with the specified connection name.
    /// </summary>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>A new typed command instance with the specified connection name.</returns>
    public new DataCommandBase<TResult> WithConnection(string connectionName)
    {
        return (DataCommandBase<TResult>)base.WithConnection(connectionName);
    }

    /// <summary>
    /// Creates a new typed command with the specified target container.
    /// </summary>
    /// <param name="targetContainer">The target container path.</param>
    /// <returns>A new typed command instance with the specified target container.</returns>
    public new DataCommandBase<TResult> WithTarget(DataPath targetContainer)
    {
        return (DataCommandBase<TResult>)base.WithTarget(targetContainer);
    }

    /// <summary>
    /// Creates a new typed command with the specified timeout.
    /// </summary>
    /// <param name="timeout">The timeout to use.</param>
    /// <returns>A new typed command instance with the specified timeout.</returns>
    public new DataCommandBase<TResult> WithTimeout(TimeSpan timeout)
    {
        return (DataCommandBase<TResult>)base.WithTimeout(timeout);
    }
}