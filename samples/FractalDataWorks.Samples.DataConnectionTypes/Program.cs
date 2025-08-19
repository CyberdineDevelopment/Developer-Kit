using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Services.DataProvider.Services;
using FractalDataWorks.Services.DataProvider.Configuration;
using FractalDataWorks.Services.DataProvider.Abstractions.Configuration;
using FractalDataWorks.Services.DataProvider.EnhancedEnums;
using FractalDataWorks.Services.ExternalConnections.MsSql.EnhancedEnums;

namespace FractalDataWorks.Samples.DataConnectionTypes;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register configuration
                services.Configure<DataStoreConfiguration[]>(
                    context.Configuration.GetSection("DataStores"));
                
                // Register DataProvider services
                services.AddSingleton<DataStoreConfigurationRegistry>();
                services.AddSingleton<IExternalDataConnectionProvider, ExternalDataConnectionProvider>();
                services.AddSingleton<IDataProvider, DataProviderService>();
                
                // Add logging
                services.AddLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                });
            });

        var host = builder.Build();
        
        // Demonstrate the connection type pattern
        await RunSample(host.Services);
    }

    private static async Task RunSample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var connectionProvider = services.GetRequiredService<IExternalDataConnectionProvider>();
        
        logger.LogInformation("=== Data Connection Types Sample ===");
        
        // Show connection type capabilities
        logger.LogInformation("Available Connection Types:");
        
        var msSqlType = new MsSqlConnectionType();
        logger.LogInformation("- {Name}: Transactions={SupportsTransactions}, Batch={SupportsBatch}, Schema={SupportsSchema}", 
            msSqlType.Name, msSqlType.SupportsTransactions, msSqlType.SupportsBatchOperations, msSqlType.SupportsSchemaDiscovery);
        
        // Test connection availability
        logger.LogInformation("\\nTesting connection availability:");
        
        var isAvailable = await connectionProvider.IsConnectionAvailable("SampleDatabase");
        logger.LogInformation("SampleDatabase connection available: {IsAvailable}", isAvailable.IsSuccess && isAvailable.Value);
        
        // Get connection metadata
        logger.LogInformation("\\nRetrieving connection metadata:");
        
        var metadataResult = await connectionProvider.GetConnectionsMetadata();
        if (metadataResult.IsSuccess)
        {
            foreach (var kvp in metadataResult.Value)
            {
                logger.LogInformation("Connection {Name}: {Metadata}", kvp.Key, kvp.Value);
            }
        }
        
        // Test schema discovery
        logger.LogInformation("\\nTesting schema discovery:");
        
        var schemaResult = await connectionProvider.DiscoverConnectionSchema("SampleDatabase");
        if (schemaResult.IsSuccess)
        {
            logger.LogInformation("Discovered {Count} schema containers", schemaResult.Value.Count());
            foreach (var container in schemaResult.Value.Take(5)) // Show first 5
            {
                logger.LogInformation("- {ContainerName} ({ContainerType})", container.Name, container.Type);
            }
        }
        else
        {
            logger.LogWarning("Schema discovery failed: {ErrorMessage}", schemaResult.Message);
        }
        
        logger.LogInformation("\\n=== Sample Complete ===");
    }
}