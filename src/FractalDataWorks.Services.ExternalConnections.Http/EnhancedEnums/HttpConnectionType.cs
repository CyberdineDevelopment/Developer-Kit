using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.ExternalConnections.Http.EnhancedEnums;

/// <summary>
/// Enhanced enum type for HTTP external connection service.
/// </summary>
[EnumOption]
public sealed class HttpConnectionType : ExternalConnectionServiceTypeBase<HttpExternalConnectionService, HttpConnectionConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpConnectionType"/> class.
    /// </summary>
    public HttpConnectionType() : base(2, "Http", "HTTP external connection service supporting REST, SOAP, and GraphQL protocols")
    {
    }
    
    /// <inheritdoc/>
    public override string[] SupportedDataStores => new[] { "REST", "HTTP", "Web API", "SOAP", "GraphQL" };
    
    /// <inheritdoc/>
    public override string ProviderName => "System.Net.Http";
    
    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedConnectionModes => new[]
    {
        "REST", 
        "SOAP", 
        "GraphQL",
        "WebAPI"
    };
    
    /// <inheritdoc/>
    public override int Priority => 90;

    /// <inheritdoc/>
    public override IServiceFactory<HttpExternalConnectionService, HttpConnectionConfiguration> CreateTypedFactory()
    {
        return new HttpConnectionFactory();
    }
}