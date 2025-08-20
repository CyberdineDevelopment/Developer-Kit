using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Configuration for container access capabilities and constraints.
/// </summary>
public sealed class ContainerAccessConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether this access type is supported.
    /// </summary>
    public bool Supported { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of records that can be processed in a single operation.
    /// </summary>
    /// <remarks>
    /// For reads: maximum result set size before paging is required.
    /// For writes: maximum batch size for bulk operations.
    /// Set to 0 for no limit.
    /// </remarks>
    public int MaxRecords { get; set; } = 0;

    /// <summary>
    /// Gets or sets the timeout in seconds for operations of this type.
    /// </summary>
    /// <remarks>
    /// Override for the default timeout specified in DataStoreConfiguration.
    /// Set to 0 to use the default timeout.
    /// </remarks>
    public int TimeoutSeconds { get; set; } = 0;

    /// <summary>
    /// Gets or sets additional constraints or capabilities for this access type.
    /// </summary>
    /// <remarks>
    /// Provider-specific access configuration:
    /// - SQL: "AllowDirtyReads", "RequireTransaction", "SupportsCTE"
    /// - File: "AppendOnly", "RequiresLocking", "SupportsStreaming"
    /// - API: "RequiresAuth", "SupportsETag", "RateLimit"
    /// </remarks>
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>(StringComparer.Ordinal);
}