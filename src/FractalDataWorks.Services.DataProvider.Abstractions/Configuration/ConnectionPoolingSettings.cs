namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Connection pooling configuration for data store connections.
/// </summary>
public sealed class ConnectionPoolingSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether connection pooling is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum number of connections to maintain in the pool.
    /// </summary>
    public int MinPoolSize { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum number of connections allowed in the pool.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum time in seconds a connection can be idle before being removed.
    /// </summary>
    public int IdleTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the maximum lifetime in seconds for a connection in the pool.
    /// </summary>
    public int MaxLifetimeSeconds { get; set; } = 3600;
}