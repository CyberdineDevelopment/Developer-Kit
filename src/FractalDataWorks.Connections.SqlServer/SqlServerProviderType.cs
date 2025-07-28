using System;
using FractalDataWorks.Connections;
using FractalDataWorks.Data;

namespace FractalDataWorks.Connections.SqlServer;

/// <summary>
/// Enhanced enum registration for SQL Server provider
/// </summary>
public class SqlServerProviderType : IDataProviderType
{
    public string Name => "SqlServer";
    public int Order => 1;
    public string ProviderName => "Microsoft SQL Server";
    
    public Type ConnectionType => typeof(SqlServerConnection);
    public Type TranslatorType => typeof(SqlCommandTranslator);
    public Type ConfigurationType => typeof(SqlServerConfiguration);
    
    public ProviderCapabilities Capabilities => 
        ProviderCapabilities.BasicCrud |
        ProviderCapabilities.Transactions |
        ProviderCapabilities.BulkOperations |
        ProviderCapabilities.StoredProcedures |
        ProviderCapabilities.ComplexQueries |
        ProviderCapabilities.JsonColumns |
        ProviderCapabilities.Streaming;
}