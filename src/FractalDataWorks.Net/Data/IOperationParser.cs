

namespace FractalDataWorks.Data;

/// <summary>
/// Interface for parsing universal data operations into provider-specific commands.
/// Parsers convert LINQ expressions and operations into formats that specific providers can execute.
/// </summary>
public interface IOperationParser
{
    /// <summary>
    /// Checks if this parser can handle the given operation
    /// </summary>
    /// <param name="operation">The operation to check</param>
    /// <returns>True if this parser can handle the operation</returns>
    bool CanHandle(IDataOperation operation);
    
    /// <summary>
    /// Parses a universal operation into a provider-specific format
    /// </summary>
    /// <param name="operation">The universal operation</param>
    /// <param name="schema">The schema definition for the target container</param>
    /// <returns>A parsed operation ready for execution by the provider</returns>
    Task<IGenericResult<IParsedOperation>> Parse(IDataOperation operation, DataContainerDefinition schema);
}

/// <summary>
/// Represents a parsed operation ready for execution
/// </summary>
public interface IParsedOperation
{
    /// <summary>
    /// The original operation that was parsed
    /// </summary>
    IDataOperation OriginalOperation { get; }
    
    /// <summary>
    /// Provider-specific command/query text
    /// </summary>
    string CommandText { get; }
    
    /// <summary>
    /// Parameters for the command
    /// </summary>
    Dictionary<string, object> Parameters { get; }
    
    /// <summary>
    /// Metadata about the parsed operation
    /// </summary>
    OperationMetadata Metadata { get; }
}

/// <summary>
/// Metadata about a parsed operation
/// </summary>
public record OperationMetadata
{
    /// <summary>
    /// The type of operation
    /// </summary>
    public OperationType OperationType { get; init; }
    
    /// <summary>
    /// The target container name
    /// </summary>
    public string ContainerName { get; init; } = string.Empty;
    
    /// <summary>
    /// Expected execution time estimate
    /// </summary>
    public TimeSpan? EstimatedExecutionTime { get; init; }
    
    /// <summary>
    /// Additional provider-specific metadata
    /// </summary>
    public Dictionary<string, object> AdditionalData { get; init; } = new();
}
