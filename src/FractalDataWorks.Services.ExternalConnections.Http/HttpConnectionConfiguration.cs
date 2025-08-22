using System.Collections.Generic;
using FractalDataWorks.Configuration;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks.Services.ExternalConnections.Http.EnhancedEnums;

namespace FractalDataWorks.Services.ExternalConnections.Http;

/// <summary>
/// Configuration class for HTTP external connections.
/// </summary>
public sealed class HttpConnectionConfiguration : ConfigurationBase<HttpConnectionConfiguration>, IExternalConnectionConfiguration
{
    /// <inheritdoc/>
    public override string SectionName => "ExternalConnections:Http";

    /// <summary>
    /// Gets or sets the base URL for the HTTP connection.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP protocol type (REST, SOAP, GraphQL).
    /// </summary>
    public HttpProtocolType Protocol { get; set; } = HttpProtocolType.Rest;

    /// <summary>
    /// Gets or sets the timeout for HTTP requests in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the authentication type.
    /// </summary>
    public string AuthenticationType { get; set; } = "None";

    /// <summary>
    /// Gets or sets additional headers to include with requests.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>(System.StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the content type for requests.
    /// </summary>
    public string ContentType { get; set; } = "application/json";
}
