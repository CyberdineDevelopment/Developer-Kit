using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.ExternalConnections.Http.EnhancedEnums;

/// <summary>
/// Enhanced enum for HTTP protocol types.
/// </summary>
[EnumCollection]
public sealed class HttpProtocolType : EnumOptionBase<HttpProtocolType>
{
    /// <summary>
    /// REST (Representational State Transfer) protocol - uses standard HTTP methods and status codes.
    /// </summary>
    public static readonly HttpProtocolType Rest = new(1, "REST", "Representational State Transfer protocol using standard HTTP methods and status codes");

    /// <summary>
    /// SOAP (Simple Object Access Protocol) - XML-based messaging protocol.
    /// </summary>
    public static readonly HttpProtocolType Soap = new(2, "SOAP", "Simple Object Access Protocol - XML-based messaging protocol");

    /// <summary>
    /// GraphQL - Query language and runtime for APIs with a single endpoint.
    /// </summary>
    public static readonly HttpProtocolType GraphQL = new(3, "GraphQL", "Query language and runtime for APIs with a single endpoint");

    /// <summary>
    /// Gets the description of the protocol type.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpProtocolType"/> class.
    /// </summary>
    /// <param name="value">The numeric value of the protocol type.</param>
    /// <param name="name">The name of the protocol type.</param>
    /// <param name="description">The description of the protocol type.</param>
    private HttpProtocolType(int value, string name, string description) : base(value, name)
    {
        Description = description;
    }
}