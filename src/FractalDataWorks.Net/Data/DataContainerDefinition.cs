namespace FractalDataWorks.Data;

/// <summary>
/// Defines the structure and layout of a data container
/// </summary>
public class DataContainerDefinition
{
    /// <summary>
    /// The container name (table, collection, etc.)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The data fields/columns in the container
    /// </summary>
    public List<DataFieldDefinition> Fields { get; set; } = new();
    
    /// <summary>
    /// Primary key fields
    /// </summary>
    public List<string> PrimaryKeys { get; set; } = new();
    
    /// <summary>
    /// Indexes defined on the container
    /// </summary>
    public List<DataIndexDefinition> Indexes { get; set; } = new();
}

/// <summary>
/// Defines a data field within a container
/// </summary>
public class DataFieldDefinition
{
    /// <summary>
    /// Field name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Field data type
    /// </summary>
    public Type DataType { get; set; } = typeof(object);
    
    /// <summary>
    /// Whether the field is nullable
    /// </summary>
    public bool IsNullable { get; set; }
    
    /// <summary>
    /// Maximum length for string fields
    /// </summary>
    public int? MaxLength { get; set; }
    
    /// <summary>
    /// Default value for the field
    /// </summary>
    public object? DefaultValue { get; set; }
}

/// <summary>
/// Defines an index on a data container
/// </summary>
public class DataIndexDefinition
{
    /// <summary>
    /// Index name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Fields included in the index
    /// </summary>
    public List<string> Fields { get; set; } = new();
    
    /// <summary>
    /// Whether the index is unique
    /// </summary>
    public bool IsUnique { get; set; }
}