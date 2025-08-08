using System.IO;
using FluentValidation;
using FractalDataWorks.Configuration;

namespace FractalDataWorks.Connections.Stream;

/// <summary>
/// Configuration for stream connections.
/// </summary>
public class StreamConnectionConfiguration : ConfigurationBase<StreamConnectionConfiguration>
{
    /// <summary>
    /// Gets or sets the type of stream to create.
    /// </summary>
    public StreamType StreamType { get; set; } = StreamType.File;

    /// <summary>
    /// Gets or sets the path for file-based streams.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the file mode for file streams.
    /// </summary>
    public FileMode FileMode { get; set; } = FileMode.OpenOrCreate;

    /// <summary>
    /// Gets or sets the file access for file streams.
    /// </summary>
    public FileAccess FileAccess { get; set; } = FileAccess.ReadWrite;

    /// <summary>
    /// Gets or sets the file share mode for file streams.
    /// </summary>
    public FileShare FileShare { get; set; } = FileShare.Read;

    /// <summary>
    /// Gets or sets the buffer size for stream operations.
    /// </summary>
    public int BufferSize { get; set; } = 4096;

    /// <summary>
    /// Gets or sets whether to use async I/O for file streams.
    /// </summary>
    public bool UseAsync { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to automatically flush after write operations.
    /// </summary>
    public bool AutoFlush { get; set; }

    /// <summary>
    /// Gets or sets the initial capacity for memory streams.
    /// </summary>
    public int InitialCapacity { get; set; }

    /// <inheritdoc/>
    public override string SectionName => "StreamConnection";

    /// <inheritdoc/>
    protected override IValidator<StreamConnectionConfiguration> GetValidator()
    {
        return new StreamConnectionConfigurationValidator();
    }
}