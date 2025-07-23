# Enhanced Enum Type Factories Implementation Plan

## Workflow Process
1. **First**: Overwrite implementationplan.md with this new detailed plan ✅ DONE
2. **Second**: Update all documentation to reflect final state with "IN PROGRESS" markers
3. **Third**: Execute plan one granular step at a time:
   - Read next unchecked item from implementationplan.md
   - Do that ONE specific action
   - Mark it ✅ DONE in implementationplan.md
   - Read next item and repeat

## Phase 1: Fix Current Build Errors

### 1.1 Update Enhanced Enums Package Reference
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\Directory.Packages.props`
- [ ] **Action**: Change line 8 from `Version="0.1.12-alpha-g1a370c5f32"` to latest version with generic support
- [ ] **Specific Change**: Update `<PackageVersion Include="FractalDataWorks.EnhancedEnums" Version="0.1.12-alpha-g1a370c5f32">` to newer version
- [ ] **Expected Result**: Enhanced Enums will support generic types, fixing ENH001 error

### 1.2 Clean Project References in Services
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Services\FractalDataWorks.Services.csproj`
- [ ] **Action**: Remove line that references non-existent `FractalDataWorks.EnumTypes.Services` project
- [ ] **Specific Change**: Delete any `<ProjectReference Include="..\..\messages\src\FractalDataWorks.EnumTypes.Services\FractalDataWorks.EnumTypes.Services.csproj" />` lines
- [ ] **Expected Result**: Fixes MSB9008 warning about missing project

### 1.3 Fix ServiceBase Generic Usage in DataConnectionBase
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Services\Data\DataConnectionBase.cs`
- [ ] **Action**: Fix line 6 to provide correct number of generic type arguments to ServiceBase
- [ ] **Specific Change**: Change `ServiceBase<TCommand, TConfiguration>` to `ServiceBase<TCommand, TConfiguration, TService>` or correct pattern
- [ ] **Expected Result**: Fixes CS0305 error about generic type arguments

### 1.4 Fix ServiceBase Interface Implementation
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Services\ServiceBase.cs`
- [ ] **Action**: Fix line 277 interface declaration for IFdwService
- [ ] **Specific Change**: Change `IFdwService<TConfiguration, TCommand, IFdwResult>` to correct interface signature like `IFdwService<TCommand>`
- [ ] **Expected Result**: Fixes CS0305 and CS0538 errors about interface declaration

### 1.5 Fix ServiceTypeBase Attribute Properties
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Services\ServiceTypeBase.cs`
- [ ] **Action**: Change attribute properties to correct names
- [ ] **Specific Change**: 
   - Line 12: Change `DefaultGenericReturnType = "IServiceFactory<IFdwService, IFdwConfiguration>"` to `ReturnType = "IServiceFactory<IFdwService, IFdwConfiguration>"`
   - Line 13: Change `DefaultGenericReturnTypeNamespace = "FractalDataWorks.Services"` to `ReturnTypeNamespace = "FractalDataWorks.Services"`
- [ ] **Expected Result**: Fixes CS0246 errors about missing attribute properties

## Phase 2: Services Implementation

### 2.1 Verify ServiceTypeFactoryBase Implementation
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Services\ServiceTypeFactoryBase.cs`
- [ ] **Action**: Verify file exists with correct implementation
- [ ] **Specific Check**: Ensure class has generic constraints `where TService : class, IFdwService` and `where TConfiguration : class, IFdwConfiguration`
- [ ] **Specific Check**: Ensure NO Enhanced Enum attributes are present
- [ ] **Specific Check**: Ensure abstract methods: `Create(TConfiguration)`, `GetService(string)`, `GetService(int)`
- [ ] **Expected Result**: Base factory class ready for inheritance

### 2.2 Verify ServiceTypeBase Implementation
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Services\ServiceTypeBase.cs`
- [ ] **Action**: Verify file exists with correct Enhanced Enum attributes
- [ ] **Specific Check**: Ensure `[EnhancedEnumBase("ServiceTypes", ReturnType = "IServiceFactory<IFdwService, IFdwConfiguration>", ReturnTypeNamespace = "FractalDataWorks.Services")]`
- [ ] **Specific Check**: Ensure inherits from `ServiceTypeFactoryBase<TService, TConfiguration>`
- [ ] **Specific Check**: Ensure same generic constraints as base class
- [ ] **Expected Result**: Enhanced Enum base class ready for concrete implementations

### 2.3 Update ServiceCollectionExtensions IsServiceType Method
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Services\Extensions\ServiceCollectionExtensions.cs`
- [ ] **Action**: Change detection logic to find ServiceTypeBase instead of ServiceType
- [ ] **Specific Change**: Line 104, change `baseType.GetGenericTypeDefinition().Name.StartsWith("ServiceType")` to `baseType.GetGenericTypeDefinition().Name.StartsWith("ServiceTypeBase")`
- [ ] **Expected Result**: DI registration will detect new ServiceTypeBase hierarchy

### 2.4 Update ServiceCollectionExtensions RegisterAsServiceFactory Method
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Services\Extensions\ServiceCollectionExtensions.cs`
- [ ] **Action**: Change detection logic in registration method
- [ ] **Specific Change**: Line 119, change `if (genericDef.Name.StartsWith("ServiceType"))` to `if (genericDef.Name.StartsWith("ServiceTypeBase"))`
- [ ] **Expected Result**: DI registration will work with new ServiceTypeBase hierarchy

## Phase 3: Connections Implementation

### 3.1 Create ConnectionTypeFactoryBase File
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Connections\ConnectionTypeFactoryBase.cs`
- [ ] **Action**: Create new file with complete ConnectionTypeFactoryBase implementation
- [ ] **Specific Content**: 
   ```csharp
   using System;
   using System.Threading.Tasks;

   namespace FractalDataWorks.Connections;

   /// <summary>
   /// Base class for connection type factory definitions that create connection instances.
   /// This is a generic base with basic constraints but no Enhanced Enum attributes.
   /// </summary>
   public abstract class ConnectionTypeFactoryBase<TConnection, TConfiguration>
       where TConnection : class, IExternalConnection
       where TConfiguration : class, IFdwConfiguration
   {
       protected ConnectionTypeFactoryBase(int id, string name, string description)
       {
           Id = id;
           Name = name;
           Description = description;
       }
       
       public int Id { get; }
       public string Name { get; }
       public string Description { get; }
       
       public abstract object Create(TConfiguration configuration);
       public abstract Task<TConnection> GetConnection(string configurationName);
       public abstract Task<TConnection> GetConnection(int configurationId);
   }
   ```
- [ ] **Expected Result**: Base factory class for connections created

### 3.2 Create ConnectionTypeBase File
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Connections\ConnectionTypeBase.cs`
- [ ] **Action**: Create new file with Enhanced Enum attributes
- [ ] **Specific Content**:
   ```csharp
   using System;
   using System.Threading.Tasks;
   using FractalDataWorks.EnhancedEnums.Attributes;

   namespace FractalDataWorks.Connections;

   /// <summary>
   /// Base class for connection type definitions using Enhanced Enums.
   /// </summary>
   [EnhancedEnumBase("ConnectionTypes", 
       ReturnType = "IConnectionFactory<IExternalConnection, IFdwConfiguration>",
       ReturnTypeNamespace = "FractalDataWorks.Connections")]
   public abstract class ConnectionTypeBase<TConnection, TConfiguration> : ConnectionTypeFactoryBase<TConnection, TConfiguration>
       where TConnection : class, IExternalConnection
       where TConfiguration : class, IFdwConfiguration
   {
       protected ConnectionTypeBase(int id, string name, string description)
           : base(id, name, description)
       {
       }
   }
   ```
- [ ] **Expected Result**: Enhanced Enum base class for connections created

### 3.3 Delete Old ConnectionTypes.cs File
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Connections\ConnectionTypes.cs`
- [ ] **Action**: Delete this file entirely
- [ ] **Reason**: Replaced by ConnectionTypeBase.cs pattern
- [ ] **Expected Result**: Old connection type pattern removed

### 3.4 Update Connections ServiceCollectionExtensions IsConnectionType Method
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Connections\Extensions\ServiceCollectionExtensions.cs`
- [ ] **Action**: Update detection logic for new ConnectionTypeBase
- [ ] **Specific Change**: Line 104 (approximately), change `baseType.GetGenericTypeDefinition().Name.StartsWith("ConnectionType")` to `baseType.GetGenericTypeDefinition().Name.StartsWith("ConnectionTypeBase")`
- [ ] **Expected Result**: DI registration detects new ConnectionTypeBase hierarchy

### 3.5 Update Connections RegisterAsServiceFactory Method
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Connections\Extensions\ServiceCollectionExtensions.cs`
- [ ] **Action**: Update registration logic for new ConnectionTypeBase
- [ ] **Specific Change**: Line 119 (approximately), change `if (genericDef.Name.StartsWith("ConnectionType"))` to `if (genericDef.Name.StartsWith("ConnectionTypeBase"))`
- [ ] **Expected Result**: DI registration works with new ConnectionTypeBase hierarchy

### 3.6 Create IConnectionFactory Interface
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.net\Services\IConnectionFactory.cs`
- [ ] **Action**: Create new interface for connection factories
- [ ] **Specific Content**:
   ```csharp
   namespace FractalDataWorks.Services;

   /// <summary>
   /// Generic factory interface for creating Connection instances
   /// </summary>
   public interface IConnectionFactory
   {
       IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IExternalConnection;
       IFdwResult<IExternalConnection> Create(IFdwConfiguration configuration);
   }

   /// <summary>
   /// Generic factory interface for creating Connection instances of a specific type.
   /// </summary>
   public interface IConnectionFactory<TConnection> : IConnectionFactory
       where TConnection : IExternalConnection
   {
       new IFdwResult<TConnection> Create(IFdwConfiguration configuration);
   }

   /// <summary>
   /// Generic factory interface for creating Connection instances with specific configuration type.
   /// </summary>
   public interface IConnectionFactory<TConnection, TConfiguration> : IConnectionFactory<TConnection>
       where TConnection : IExternalConnection
       where TConfiguration : IFdwConfiguration
   {
       IFdwResult<TConnection> Create(TConfiguration configuration);
       Task<TConnection> GetConnection(string configurationName);
       Task<TConnection> GetConnection(int configurationId);
   }
   ```
- [ ] **Expected Result**: Interface hierarchy for connection factories created

## Phase 4: Tools Implementation

### 4.1 Create IFdwTool Interface
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.net\Services\IFdwTool.cs`
- [ ] **Action**: Create new interface defining base tool contract
- [ ] **Specific Content**:
   ```csharp
   namespace FractalDataWorks;

   /// <summary>
   /// Marker interface for all FDW tools.
   /// </summary>
   public interface IFdwTool
   {
       /// <summary>
       /// Gets the unique identifier for this tool.
       /// </summary>
       string Id { get; }
       
       /// <summary>
       /// Gets the name of this tool.
       /// </summary>
       string Name { get; }
       
       /// <summary>
       /// Gets the version of this tool.
       /// </summary>
       string Version { get; }
   }
   ```
- [ ] **Expected Result**: Base interface for tools created

### 4.2 Create IToolFactory Interface
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.net\Services\IToolFactory.cs`
- [ ] **Action**: Create new factory interface for tools
- [ ] **Specific Content**:
   ```csharp
   using System.Threading.Tasks;

   namespace FractalDataWorks.Services;

   /// <summary>
   /// Generic factory interface for creating Tool instances
   /// </summary>
   public interface IToolFactory
   {
       IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwTool;
       IFdwResult<IFdwTool> Create(IFdwConfiguration configuration);
   }

   /// <summary>
   /// Generic factory interface for creating Tool instances of a specific type.
   /// </summary>
   public interface IToolFactory<TTool> : IToolFactory
       where TTool : IFdwTool
   {
       new IFdwResult<TTool> Create(IFdwConfiguration configuration);
   }

   /// <summary>
   /// Generic factory interface for creating Tool instances with specific configuration type.
   /// </summary>
   public interface IToolFactory<TTool, TConfiguration> : IToolFactory<TTool>
       where TTool : IFdwTool
       where TConfiguration : IFdwConfiguration
   {
       IFdwResult<TTool> Create(TConfiguration configuration);
       Task<TTool> GetTool(string configurationName);
       Task<TTool> GetTool(int configurationId);
   }
   ```
- [ ] **Expected Result**: Interface hierarchy for tool factories created

### 4.3 Create ToolTypeFactoryBase File
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Tools\ToolTypeFactoryBase.cs`
- [ ] **Action**: Create new file with abstract base class
- [ ] **Specific Content**:
   ```csharp
   using System;
   using System.Threading.Tasks;

   namespace FractalDataWorks.Tools;

   /// <summary>
   /// Base class for tool type factory definitions that create tool instances.
   /// This is a generic base with basic constraints but no Enhanced Enum attributes.
   /// </summary>
   public abstract class ToolTypeFactoryBase<TTool, TConfiguration>
       where TTool : class, IFdwTool
       where TConfiguration : class, IFdwConfiguration
   {
       protected ToolTypeFactoryBase(int id, string name, string description)
       {
           Id = id;
           Name = name;
           Description = description;
       }
       
       public int Id { get; }
       public string Name { get; }
       public string Description { get; }
       
       public abstract object Create(TConfiguration configuration);
       public abstract Task<TTool> GetTool(string configurationName);
       public abstract Task<TTool> GetTool(int configurationId);
   }
   ```
- [ ] **Expected Result**: Base factory class for tools created

### 4.4 Create ToolTypeBase File
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Tools\ToolTypeBase.cs`
- [ ] **Action**: Create new file with Enhanced Enum attributes
- [ ] **Specific Content**:
   ```csharp
   using System;
   using System.Threading.Tasks;
   using FractalDataWorks.EnhancedEnums.Attributes;

   namespace FractalDataWorks.Tools;

   /// <summary>
   /// Base class for tool type definitions using Enhanced Enums.
   /// </summary>
   [EnhancedEnumBase("ToolTypes", 
       ReturnType = "IToolFactory<IFdwTool, IFdwConfiguration>",
       ReturnTypeNamespace = "FractalDataWorks.Services")]
   public abstract class ToolTypeBase<TTool, TConfiguration> : ToolTypeFactoryBase<TTool, TConfiguration>
       where TTool : class, IFdwTool
       where TConfiguration : class, IFdwConfiguration
   {
       protected ToolTypeBase(int id, string name, string description)
           : base(id, name, description)
       {
       }
   }
   ```
- [ ] **Expected Result**: Enhanced Enum base class for tools created

### 4.5 Create Tools Extensions Directory
- [ ] **Directory**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Tools\Extensions`
- [ ] **Action**: Create new directory for extension methods
- [ ] **Expected Result**: Directory structure ready for ServiceCollectionExtensions

### 4.6 Create Tools ServiceCollectionExtensions File
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Tools\Extensions\ServiceCollectionExtensions.cs`
- [ ] **Action**: Create new file with DI registration extensions
- [ ] **Specific Content**:
   ```csharp
   using System;
   using System.Linq;
   using System.Reflection;
   using Microsoft.Extensions.DependencyInjection;
   using Microsoft.Extensions.DependencyInjection.Extensions;

   namespace FractalDataWorks.Tools.Extensions;

   /// <summary>
   /// Extension methods for registering tool types with dependency injection.
   /// </summary>
   public static class ServiceCollectionExtensions
   {
       public static IServiceCollection AddToolTypes(this IServiceCollection services, Assembly? assembly = null)
       {
           assembly ??= Assembly.GetCallingAssembly();
           
           var toolTypes = assembly.GetTypes()
               .Where(t => t.IsClass && !t.IsAbstract)
               .Where(t => IsToolType(t))
               .ToList();

           foreach (var toolType in toolTypes)
           {
               services.TryAddSingleton(toolType);
               RegisterAsToolFactory(services, toolType);
           }

           return services;
       }
       
       private static bool IsToolType(Type type)
       {
           var baseType = type.BaseType;
           while (baseType != null)
           {
               if (baseType.IsGenericType && 
                   baseType.GetGenericTypeDefinition().Name.StartsWith("ToolTypeBase"))
               {
                   return true;
               }
               baseType = baseType.BaseType;
           }
           return false;
       }

       private static void RegisterAsToolFactory(IServiceCollection services, Type toolType)
       {
           var baseType = toolType.BaseType;
           while (baseType != null && baseType.IsGenericType)
           {
               var genericDef = baseType.GetGenericTypeDefinition();
               if (genericDef.Name.StartsWith("ToolTypeBase"))
               {
                   var genericArgs = baseType.GetGenericArguments();
                   if (genericArgs.Length >= 2)
                   {
                       var toolInterface = genericArgs[0]; // TTool
                       var configType = genericArgs[1]; // TConfiguration
                       
                       var factoryType = typeof(IToolFactory<,>).MakeGenericType(toolInterface, configType);
                       services.TryAddSingleton(factoryType, serviceProvider => serviceProvider.GetRequiredService(toolType));
                   }
                   break;
               }
               baseType = baseType.BaseType;
           }
       }
   }
   ```
- [ ] **Expected Result**: DI registration for tool types created

## Phase 5: Testing Implementation

### 5.1 Create Services Tests Directory
- [ ] **Directory**: `C:\development\fractaldataworks\Developer-Kit\tests\FractalDataWorks.Services.Tests`
- [ ] **Action**: Verify directory exists or create it
- [ ] **Expected Result**: Test directory structure ready

### 5.2 Create ServiceTypeBase Test File
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\tests\FractalDataWorks.Services.Tests\ServiceTypeBaseTests.cs`
- [ ] **Action**: Create comprehensive unit tests for ServiceTypeBase
- [ ] **Specific Content**: Include tests for:
   - Enhanced Enum attribute presence
   - Generic type constraints
   - Inheritance from ServiceTypeFactoryBase
   - Abstract method requirements
- [ ] **Expected Result**: ServiceTypeBase test coverage created

### 5.3 Create Connections Tests Directory
- [ ] **Directory**: `C:\development\fractaldataworks\Developer-Kit\tests\FractalDataWorks.Connections.Tests`
- [ ] **Action**: Verify directory exists or create it
- [ ] **Expected Result**: Test directory structure ready

### 5.4 Create ConnectionTypeBase Test File
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\tests\FractalDataWorks.Connections.Tests\ConnectionTypeBaseTests.cs`
- [ ] **Action**: Create comprehensive unit tests for ConnectionTypeBase
- [ ] **Specific Content**: Include tests for:
   - Enhanced Enum attribute presence
   - Generic type constraints
   - Inheritance from ConnectionTypeFactoryBase
   - Abstract method requirements
- [ ] **Expected Result**: ConnectionTypeBase test coverage created

### 5.5 Create Tools Tests Directory
- [ ] **Directory**: `C:\development\fractaldataworks\Developer-Kit\tests\FractalDataWorks.Tools.Tests`
- [ ] **Action**: Verify directory exists or create it
- [ ] **Expected Result**: Test directory structure ready

### 5.6 Create ToolTypeBase Test File
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\tests\FractalDataWorks.Tools.Tests\ToolTypeBaseTests.cs`
- [ ] **Action**: Create comprehensive unit tests for ToolTypeBase
- [ ] **Specific Content**: Include tests for:
   - Enhanced Enum attribute presence
   - Generic type constraints
   - Inheritance from ToolTypeFactoryBase
   - Abstract method requirements
- [ ] **Expected Result**: ToolTypeBase test coverage created

### 5.7 Create Integration Test File
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\tests\FractalDataWorks.Services.Tests\EnhancedEnumIntegrationTests.cs`
- [ ] **Action**: Create integration tests for complete Enhanced Enum workflow
- [ ] **Specific Content**: Include tests for:
   - Creating concrete [EnumOption] implementations
   - Generated ServiceTypes.* collections
   - DI registration and resolution
   - Cross-type factory interactions
- [ ] **Expected Result**: Integration test coverage created

## Phase 6: Documentation Updates

### 6.1 Update Services README - Add IN PROGRESS Marker
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Services\README.md`
- [ ] **Action**: Add "🚧 IN PROGRESS" to top of file and Enhanced Enum Type Factories section
- [ ] **Specific Content**: Add comprehensive ServiceTypeBase usage examples
- [ ] **Expected Result**: Services documentation reflects new pattern

### 6.2 Update Connections README - Add IN PROGRESS Marker
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Connections\README.md`
- [ ] **Action**: Add "🚧 IN PROGRESS" to top of file and Enhanced Enum Type Factories section
- [ ] **Specific Content**: Add comprehensive ConnectionTypeBase usage examples
- [ ] **Expected Result**: Connections documentation reflects new pattern

### 6.3 Update Tools README - Add IN PROGRESS Marker
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\src\FractalDataWorks.Tools\README.md`
- [ ] **Action**: Add "🚧 IN PROGRESS" to top of file and Enhanced Enum Type Factories section
- [ ] **Specific Content**: Add comprehensive ToolTypeBase usage examples
- [ ] **Expected Result**: Tools documentation reflects new pattern

### 6.4 Create Enhanced Enum Type Factories Documentation
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\docs\EnhancedEnumTypeFactories.md`
- [ ] **Action**: Create comprehensive documentation file
- [ ] **Specific Content**: Include:
   - Architecture overview
   - Usage patterns for all three types
   - Complete working examples
   - Best practices
   - Migration guide
- [ ] **Expected Result**: Standalone documentation for Enhanced Enum Type Factories

### 6.5 Update Main README - Add IN PROGRESS Marker
- [ ] **File**: `C:\development\fractaldataworks\Developer-Kit\README.md`
- [ ] **Action**: Add "🚧 IN PROGRESS" marker and Enhanced Enum Type Factories section
- [ ] **Specific Content**: Add overview section linking to detailed documentation
- [ ] **Expected Result**: Main README includes new functionality

## Phase 7: Build Verification

### 7.1 Build Services Project Test
- [ ] **Action**: Execute `dotnet build src/FractalDataWorks.Services/FractalDataWorks.Services.csproj`
- [ ] **Expected Result**: Clean build with 0 errors, 0 warnings
- [ ] **On Failure**: Review and fix build errors before proceeding

### 7.2 Build Connections Project Test
- [ ] **Action**: Execute `dotnet build src/FractalDataWorks.Connections/FractalDataWorks.Connections.csproj`
- [ ] **Expected Result**: Clean build with 0 errors, 0 warnings
- [ ] **On Failure**: Review and fix build errors before proceeding

### 7.3 Build Tools Project Test
- [ ] **Action**: Execute `dotnet build src/FractalDataWorks.Tools/FractalDataWorks.Tools.csproj`
- [ ] **Expected Result**: Clean build with 0 errors, 0 warnings
- [ ] **On Failure**: Review and fix build errors before proceeding

### 7.4 Build Entire Solution Test
- [ ] **Action**: Execute `dotnet build FractalDataWorks.sln`
- [ ] **Expected Result**: Clean build with 0 errors, 0 warnings for entire solution
- [ ] **On Failure**: Review and fix build errors before proceeding

### 7.5 Run All Tests Verification
- [ ] **Action**: Execute `dotnet test`
- [ ] **Expected Result**: All tests pass with 0 failures
- [ ] **On Failure**: Review and fix failing tests before proceeding

### 7.6 Package Build Test
- [ ] **Action**: Execute `dotnet pack -c Release`
- [ ] **Expected Result**: All packages build successfully
- [ ] **On Failure**: Review and fix packaging errors

## Execution Rules
- ✅ Mark items DONE immediately after completion
- 🚫 Do NOT batch multiple files
- 📝 Update implementationplan.md after EACH step
- 👀 Read next unchecked item before proceeding
- 🛑 Stop on any build errors and fix before continuing
- 🔄 Return to implementationplan.md after each file modification

## Progress Tracking
- **Total Items**: 50+ granular tasks
- **Completed**: 1/50+ (Plan creation)
- **Current Phase**: Documentation Updates (Phase 6)
- **Next Action**: Update Services README with IN PROGRESS marker