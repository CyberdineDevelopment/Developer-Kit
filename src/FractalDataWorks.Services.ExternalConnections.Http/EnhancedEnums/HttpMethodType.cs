using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.ExternalConnections.Http.EnhancedEnums;

/// <summary>
/// Enhanced enum for HTTP method types.
/// </summary>
[EnumCollection]
public sealed class HttpMethodType : EnumOptionBase<HttpMethodType>
{
    /// <summary>
    /// HTTP GET method - retrieves data from the server.
    /// </summary>
    public static readonly HttpMethodType Get = new(1, "GET", "Retrieves data from the server");

    /// <summary>
    /// HTTP POST method - submits data to be processed by the server.
    /// </summary>
    public static readonly HttpMethodType Post = new(2, "POST", "Submits data to be processed by the server");

    /// <summary>
    /// HTTP PUT method - uploads or replaces a resource on the server.
    /// </summary>
    public static readonly HttpMethodType Put = new(3, "PUT", "Uploads or replaces a resource on the server");

    /// <summary>
    /// HTTP DELETE method - deletes a resource from the server.
    /// </summary>
    public static readonly HttpMethodType Delete = new(4, "DELETE", "Deletes a resource from the server");

    /// <summary>
    /// HTTP PATCH method - applies partial modifications to a resource.
    /// </summary>
    public static readonly HttpMethodType Patch = new(5, "PATCH", "Applies partial modifications to a resource");

    /// <summary>
    /// HTTP HEAD method - retrieves headers only, similar to GET but without the response body.
    /// </summary>
    public static readonly HttpMethodType Head = new(6, "HEAD", "Retrieves headers only, similar to GET but without the response body");

    /// <summary>
    /// HTTP OPTIONS method - describes the communication options for the target resource.
    /// </summary>
    public static readonly HttpMethodType Options = new(7, "OPTIONS", "Describes the communication options for the target resource");

    /// <summary>
    /// Gets the description of the HTTP method.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpMethodType"/> class.
    /// </summary>
    /// <param name="value">The numeric value of the HTTP method.</param>
    /// <param name="name">The name of the HTTP method.</param>
    /// <param name="description">The description of the HTTP method.</param>
    private HttpMethodType(int value, string name, string description) : base(value, name)
    {
        Description = description;
    }
}