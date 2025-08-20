using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace FractalDataWorks.Services.DataProvider.Abstractions;

/// <summary>
/// Interface for data commands in the FractalDataWorks framework.
/// Represents a command that can be executed against a data provider to perform operations.
/// </summary>
/// <remarks>
/// Data commands encapsulate the details of data operations (queries, updates, etc.)
/// and provide a consistent interface for data providers to execute operations
/// regardless of the underlying data store technology.
/// </remarks>
public interface IDataCommand : ICommand
{
    
    /// <summary>
    /// Gets the type of operation this command represents.
    /// </summary>
    /// <value>The command type (e.g., "Query", "Insert", "Update", "Delete", "StoredProcedure").</value>
    /// <remarks>
    /// Command types help data providers determine how to execute the command
    /// and what type of result to expect. This enables provider-specific optimizations.
    /// </remarks>
    string CommandType { get; }
    
    /// <summary>
    /// Gets the target resource (table, collection, endpoint) for this command.
    /// </summary>
    /// <value>The target resource name, or null if not applicable.</value>
    /// <remarks>
    /// The target helps data providers route commands to the appropriate data structures
    /// and apply resource-specific configurations or security policies.
    /// </remarks>
    string? Target { get; }
    
    /// <summary>
    /// Gets the expected result type for this command.
    /// </summary>
    /// <value>The Type of object expected to be returned by command execution.</value>
    /// <remarks>
    /// This information enables data providers to prepare appropriate result handling
    /// and type conversion logic before executing the command.
    /// </remarks>
    Type ExpectedResultType { get; }
    
    /// <summary>
    /// Gets the timeout for command execution.
    /// </summary>
    /// <value>The maximum time to wait for command execution, or null for provider default.</value>
    /// <remarks>
    /// Command-specific timeouts allow fine-grained control over execution time limits.
    /// If null, the data provider should use its default timeout configuration.
    /// </remarks>
    TimeSpan? Timeout { get; }
    
    /// <summary>
    /// Gets the parameters for this command.
    /// </summary>
    /// <value>A dictionary of parameter names and values for command execution.</value>
    /// <remarks>
    /// Parameters provide input data for the command execution. The structure and
    /// types of parameters depend on the specific command type and data provider.
    /// Parameter names should use consistent naming conventions across commands.
    /// </remarks>
    IReadOnlyDictionary<string, object?> Parameters { get; }
    
    /// <summary>
    /// Gets additional metadata for this command.
    /// </summary>
    /// <value>A dictionary of metadata properties that may influence command execution.</value>
    /// <remarks>
    /// Metadata can include hints for query optimization, caching directives,
    /// security contexts, or other provider-specific configuration options.
    /// Common metadata keys include "CacheKey", "ReadPreference", "ConsistencyLevel".
    /// </remarks>
    IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Gets a value indicating whether this command modifies data.
    /// </summary>
    /// <value><c>true</c> if the command modifies data; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// This property helps data providers determine appropriate connection handling,
    /// transaction requirements, and caching behavior for the command.
    /// </remarks>
    bool IsDataModifying { get; }
    
    /// <summary>
    /// Validates the command before execution.
    /// </summary>
    /// <returns>A ValidationResult indicating whether the command is valid for execution.</returns>
    /// <remarks>
    /// This method allows commands to perform self-validation before being passed
    /// to data providers. It can check parameter completeness, value ranges,
    /// and other command-specific validation rules.
    /// </remarks>
    new ValidationResult Validate();
    
    /// <summary>
    /// Creates a copy of this command with modified parameters.
    /// </summary>
    /// <param name="newParameters">The new parameters to use in the copied command.</param>
    /// <returns>A new command instance with the specified parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newParameters"/> is null.</exception>
    /// <remarks>
    /// This method enables command reuse with different parameter sets without
    /// modifying the original command instance. Useful for batch operations
    /// or parameter optimization scenarios.
    /// </remarks>
    IDataCommand WithParameters(IReadOnlyDictionary<string, object?> newParameters);
    
    /// <summary>
    /// Creates a copy of this command with modified metadata.
    /// </summary>
    /// <param name="newMetadata">The new metadata to use in the copied command.</param>
    /// <returns>A new command instance with the specified metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newMetadata"/> is null.</exception>
    /// <remarks>
    /// This method enables command customization with different execution hints
    /// or configuration options without modifying the original command instance.
    /// </remarks>
    IDataCommand WithMetadata(IReadOnlyDictionary<string, object> newMetadata);
}

/// <summary>
/// Generic interface for data commands with typed result expectations.
/// Extends the base command interface with compile-time type safety for results.
/// </summary>
/// <typeparam name="TResult">The type of result expected from command execution.</typeparam>
/// <remarks>
/// Use this interface when the expected result type is known at compile time.
/// It provides type safety and eliminates the need for runtime type checking and casting.
/// </remarks>
public interface IDataCommand<TResult> : IDataCommand
{
    /// <summary>
    /// Creates a copy of this command with modified parameters.
    /// </summary>
    /// <param name="newParameters">The new parameters to use in the copied command.</param>
    /// <returns>A new typed command instance with the specified parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newParameters"/> is null.</exception>
    /// <remarks>
    /// This method provides type-safe command copying for generic command instances.
    /// </remarks>
    new IDataCommand<TResult> WithParameters(IReadOnlyDictionary<string, object?> newParameters);
    
    /// <summary>
    /// Creates a copy of this command with modified metadata.
    /// </summary>
    /// <param name="newMetadata">The new metadata to use in the copied command.</param>
    /// <returns>A new typed command instance with the specified metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newMetadata"/> is null.</exception>
    /// <remarks>
    /// This method provides type-safe command copying for generic command instances.
    /// </remarks>
    new IDataCommand<TResult> WithMetadata(IReadOnlyDictionary<string, object> newMetadata);
}