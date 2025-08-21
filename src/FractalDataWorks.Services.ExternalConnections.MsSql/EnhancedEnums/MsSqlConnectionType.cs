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

namespace FractalDataWorks.Services.ExternalConnections.MsSql.EnhancedEnums;
/// <summary>
/// Represents a Microsoft SQL Server external connection type.
/// </summary>

[EnumOption]
public sealed class MsSqlConnectionType : ExternalConnectionServiceTypeBase<MsSqlExternalConnectionService, MsSqlConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlConnectionType"/> class.
    /// </summary>
    public MsSqlConnectionType() : base(1, "MsSql", "Microsoft SQL Server external connection service")
    {
    }
    
    /// <inheritdoc/>
    public override string[] SupportedDataStores => new[] { "SqlServer", "MSSQL", "Microsoft SQL Server" };
    
    /// <inheritdoc/>
    public override string ProviderName => "Microsoft.Data.SqlClient";
    
    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedConnectionModes => new[]
    {
        "ReadWrite", 
        "ReadOnly", 
        "Bulk", 
        "Streaming"
    };
    
    /// <inheritdoc/>
    public override int Priority => 100;

    /// <inheritdoc/>
    public override IServiceFactory<MsSqlExternalConnectionService, MsSqlConfiguration> CreateTypedFactory()
    {
        return new MsSqlConnectionFactory();
    }
}
