namespace FractalDataWorks.Services.DataProvider.Abstractions.Models;

/// <summary>
/// Defines the types of data containers across different storage systems.
/// </summary>
public enum ContainerType
{
    /// <summary>
    /// A database table that stores data records.
    /// </summary>
    Table = 1,

    /// <summary>
    /// A database view that provides a virtual table.
    /// </summary>
    View = 2,

    /// <summary>
    /// A database schema that contains tables and views.
    /// </summary>
    Schema = 3,

    /// <summary>
    /// A database that contains schemas.
    /// </summary>
    Database = 4,

    /// <summary>
    /// A file system folder that contains files.
    /// </summary>
    Folder = 5,

    /// <summary>
    /// A file that contains data records.
    /// </summary>
    File = 6,

    /// <summary>
    /// An API resource endpoint.
    /// </summary>
    Resource = 7,

    /// <summary>
    /// A namespace that groups related resources.
    /// </summary>
    Namespace = 8,

    /// <summary>
    /// A NoSQL collection or document store.
    /// </summary>
    Collection = 9,

    /// <summary>
    /// A data stream for real-time data.
    /// </summary>
    Stream = 10,

    /// <summary>
    /// A stored procedure that can return data.
    /// </summary>
    StoredProcedure = 11,

    /// <summary>
    /// A function that returns data.
    /// </summary>
    Function = 12
}