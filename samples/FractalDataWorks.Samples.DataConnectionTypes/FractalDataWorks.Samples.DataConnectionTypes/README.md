# Data Connection Types Sample

This sample demonstrates the new Data Connection Types pattern in FractalDataWorks, showing how different data providers can be used interchangeably through a unified interface.

## Key Features Demonstrated

1. **Connection Type Pattern**: ExternalDataConnectionTypeBase following the same pattern as ServiceTypeBase
2. **Enhanced Enum Support**: Using `[EnumOption]` attributes for type collection generation
3. **Configuration-Driven Initialization**: Connections created from configuration automatically
4. **Interchangeable Providers**: Same commands work across SQL, NoSQL, file systems, APIs
5. **Type-Safe Factories**: Compile-time type checking with generated collections

## Architecture

```
ExternalDataConnectionTypeBase
├── MsSqlConnectionType (SQL Server)
├── MongoDbConnectionType (Future)
├── FileSystemConnectionType (Future)
└── ConfigurationConnectionType (Future)
```

## Configuration

The sample uses `appsettings.json` to configure data stores:

```json
{
  "DataStores": [
    {
      "StoreName": "SampleDatabase",
      "ProviderType": "MsSql",
      "IsEnabled": true,
      "ConnectionProperties": {
        "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=SampleDB;Trusted_Connection=true;"
      }
    }
  ]
}
```

## Usage

The same command pattern works across all connection types:

```csharp
// Query works the same regardless of underlying provider
var customers = await dataProvider.Execute(
    DataCommands.Query<Customer>(c => c.IsActive)
        .WithConnection("SampleDatabase")
);

// Schema discovery works across all providers
var schema = await connectionProvider.DiscoverConnectionSchema("SampleDatabase");
```

## Running the Sample

1. Ensure SQL Server LocalDB is installed
2. Update the connection string in `appsettings.json` if needed
3. Run: `dotnet run`

The sample will:
- Show available connection types and their capabilities
- Test connection availability
- Retrieve connection metadata
- Demonstrate schema discovery

## Future Enhancements

- **Enhanced Enum Source Generation**: Auto-generate DataConnectionTypes collection
- **Additional Providers**: MongoDB, FileSystem, REST API connections
- **Configuration as Data**: Query configuration itself as a data source
- **Cross-Provider Queries**: Single query spanning multiple connection types