using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Samples.DataConnectionTypes;

/// <summary>
/// Sample demonstrating the FractalDataWorks data connection types pattern.
/// This simplified version shows how different data sources can be abstracted
/// through connection type patterns without requiring the full framework.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("FractalDataWorks Data Connection Types Sample");
        Console.WriteLine("=============================================");
        Console.WriteLine("This sample demonstrates the connection type pattern for abstracting");
        Console.WriteLine("different data sources and their capabilities.");
        Console.WriteLine();

        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register our sample services
                services.AddSingleton<IConnectionTypeRegistry, ConnectionTypeRegistry>();
                services.AddSingleton<IDataConnectionManager, DataConnectionManager>();
                
                // Add logging
                services.AddLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
            });

        var host = builder.Build();
        
        // Demonstrate the connection type pattern
        await RunSample(host.Services);
    }

    private static async Task RunSample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var registry = services.GetRequiredService<IConnectionTypeRegistry>();
        var connectionManager = services.GetRequiredService<IDataConnectionManager>();
        
        logger.LogInformation("=== Data Connection Types Sample ===");
        
        // Show available connection types and their capabilities
        logger.LogInformation("Available Connection Types:");
        
        var connectionTypes = registry.GetAllConnectionTypes();
        foreach (var connectionType in connectionTypes)
        {
            logger.LogInformation("- {Name}:", connectionType.Name);
            logger.LogInformation("  Transactions: {SupportsTransactions}", connectionType.SupportsTransactions);
            logger.LogInformation("  Batch Operations: {SupportsBatch}", connectionType.SupportsBatchOperations);
            logger.LogInformation("  Schema Discovery: {SupportsSchema}", connectionType.SupportsSchemaDiscovery);
            logger.LogInformation("  Connection String Template: {Template}", connectionType.ConnectionStringTemplate);
            logger.LogInformation("");
        }
        
        // Test connection availability
        logger.LogInformation("Testing connection availability:");
        
        var connections = new[] { "SampleSqlServer", "SampleMongoDB", "SamplePostgreSQL", "NonexistentDB" };
        
        foreach (var connectionName in connections)
        {
            var isAvailable = await connectionManager.IsConnectionAvailable(connectionName);
            logger.LogInformation("{ConnectionName}: {Status}", 
                connectionName, 
                isAvailable.IsSuccess ? (isAvailable.Value ? "Available" : "Not Available") : $"Error - {isAvailable.Message}");
        }
        
        // Get connection metadata
        logger.LogInformation("");
        logger.LogInformation("Connection metadata:");
        
        var metadataResult = await connectionManager.GetConnectionsMetadata();
        if (metadataResult.IsSuccess)
        {
            foreach (var kvp in metadataResult.Value)
            {
                logger.LogInformation("{ConnectionName}: {Metadata}", kvp.Key, string.Join(", ", kvp.Value.Select(m => $"{m.Key}={m.Value}")));
            }
        }
        
        // Test schema discovery capabilities
        logger.LogInformation("");
        logger.LogInformation("Testing schema discovery:");
        
        foreach (var connectionName in connections.Take(3)) // Skip the nonexistent one
        {
            var connectionType = registry.GetConnectionTypeForConnection(connectionName);
            if (connectionType?.SupportsSchemaDiscovery == true)
            {
                var schemaResult = await connectionManager.DiscoverConnectionSchema(connectionName);
                if (schemaResult.IsSuccess)
                {
                    logger.LogInformation("{ConnectionName}: Discovered {Count} schema objects", 
                        connectionName, schemaResult.Value.Count());
                    
                    foreach (var schemaObject in schemaResult.Value.Take(3)) // Show first 3
                    {
                        logger.LogInformation("  - {Name} ({Type})", schemaObject.Name, schemaObject.Type);
                    }
                }
                else
                {
                    logger.LogWarning("{ConnectionName}: Schema discovery failed - {Error}", 
                        connectionName, schemaResult.Message);
                }
            }
            else
            {
                logger.LogInformation("{ConnectionName}: Schema discovery not supported", connectionName);
            }
        }
        
        // Demonstrate connection type selection
        logger.LogInformation("");
        logger.LogInformation("Connection type recommendations:");
        
        var requirements = new[]
        {
            new ConnectionRequirements { RequiresTransactions = true, RequiresBatchOperations = false },
            new ConnectionRequirements { RequiresTransactions = false, RequiresBatchOperations = true },
            new ConnectionRequirements { RequiresTransactions = true, RequiresBatchOperations = true, RequiresSchemaDiscovery = true }
        };
        
        foreach (var requirement in requirements)
        {
            var recommendedTypes = registry.GetCompatibleConnectionTypes(requirement);
            logger.LogInformation("For requirements (Trans: {Trans}, Batch: {Batch}, Schema: {Schema}):", 
                requirement.RequiresTransactions, 
                requirement.RequiresBatchOperations, 
                requirement.RequiresSchemaDiscovery);
            
            foreach (var type in recommendedTypes)
            {
                logger.LogInformation("  - {TypeName} is compatible", type.Name);
            }
            
            if (!recommendedTypes.Any())
            {
                logger.LogWarning("  - No compatible connection types found");
            }
        }
        
        logger.LogInformation("");
        logger.LogInformation("=== Sample Complete ===");
        logger.LogInformation("This sample demonstrated:");
        logger.LogInformation("- Connection type abstraction pattern");
        logger.LogInformation("- Capability discovery and matching");
        logger.LogInformation("- Connection availability testing");
        logger.LogInformation("- Schema discovery for supported types");
        logger.LogInformation("- Connection type selection based on requirements");
    }
}