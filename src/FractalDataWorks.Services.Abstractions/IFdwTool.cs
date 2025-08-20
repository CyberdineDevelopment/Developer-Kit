namespace FractalDataWorks;

/// <summary>
/// Marker interface for all FDW tools.
/// </summary>
public interface IFdwTool
{
    /// <summary>
    /// Gets the unique identifier for this tool.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the name of this tool.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the version of this tool.
    /// </summary>
    string Version { get; }
}