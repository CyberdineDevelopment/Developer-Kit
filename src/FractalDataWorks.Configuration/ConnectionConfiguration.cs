using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FractalDataWorks.Configuration.Validators;

namespace FractalDataWorks.Configuration
{
    /// <summary>
    /// Base configuration for connections
    /// </summary>
    public abstract class ConnectionConfiguration : ServiceConfiguration
    {
        /// <summary>
        /// The provider name (e.g., "SqlServer", "JsonFile", "RestApi")
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;
        
        /// <summary>
        /// Provider-specific configuration data
        /// </summary>
        public Dictionary<string, object> Datum { get; set; } = new();
        
        /// <summary>
        /// Schema information for data containers
        /// </summary>
        public List<DataContainerDefinition> Containers { get; set; } = new();
        
        /// <summary>
        /// Connection-specific timeout (overrides base timeout)
        /// </summary>
        public TimeSpan? ConnectionTimeout { get; set; }
        
        /// <summary>
        /// Maximum number of concurrent connections
        /// </summary>
        public int MaxConnections { get; set; } = 10;
        
        protected override IValidator<ConnectionConfiguration>? CreateValidator()
            => new ConnectionConfigurationValidator();
    }
    
    /// <summary>
    /// Generic connection configuration
    /// </summary>
    /// <typeparam name="T">The concrete configuration type</typeparam>
    public abstract class ConnectionConfiguration<T> : ConnectionConfiguration
        where T : ConnectionConfiguration<T>, new()
    {
    }
    
    /// <summary>
    /// Data connection specific configuration
    /// </summary>
    public abstract class DataConnectionConfiguration : ConnectionConfiguration
    {
        /// <summary>
        /// Default container name for operations that don't specify one
        /// </summary>
        public string? DefaultContainer { get; set; }
        
        /// <summary>
        /// Whether to use connection pooling
        /// </summary>
        public bool UseConnectionPooling { get; set; } = true;
        
        /// <summary>
        /// Command timeout for data operations
        /// </summary>
        public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);
        
        protected override IValidator<DataConnectionConfiguration>? CreateValidator()
            => new DataConnectionConfigurationValidator();
    }
    
    /// <summary>
    /// Generic data connection configuration
    /// </summary>
    /// <typeparam name="T">The concrete configuration type</typeparam>
    public abstract class DataConnectionConfiguration<T> : DataConnectionConfiguration
        where T : DataConnectionConfiguration<T>, new()
    {
    }
    
    /// <summary>
    /// Validator for connection configuration
    /// </summary>
    public class ConnectionConfigurationValidator : AbstractValidator<ConnectionConfiguration>
    {
        public ConnectionConfigurationValidator()
        {
            Include(new ServiceConfigurationValidator());
            
            RuleFor(x => x.ProviderName)
                .NotEmpty()
                .WithMessage("Provider name is required");
                
            RuleFor(x => x.MaxConnections)
                .InclusiveBetween(1, 1000)
                .WithMessage("Max connections must be between 1 and 1000");
                
            When(x => x.ConnectionTimeout.HasValue, () =>
            {
                RuleFor(x => x.ConnectionTimeout!.Value)
                    .GreaterThan(TimeSpan.Zero)
                    .LessThanOrEqualTo(TimeSpan.FromMinutes(30))
                    .WithMessage("Connection timeout must be between 0 and 30 minutes");
            });
            
            RuleForEach(x => x.Containers)
                .SetValidator(new DataContainerDefinitionValidator());
        }
    }
    
    /// <summary>
    /// Validator for data connection configuration
    /// </summary>
    public class DataConnectionConfigurationValidator : AbstractValidator<DataConnectionConfiguration>
    {
        public DataConnectionConfigurationValidator()
        {
            Include(new ConnectionConfigurationValidator());
            
            RuleFor(x => x.CommandTimeout)
                .GreaterThan(TimeSpan.Zero)
                .LessThanOrEqualTo(TimeSpan.FromMinutes(30))
                .WithMessage("Command timeout must be between 0 and 30 minutes");
                
            When(x => !string.IsNullOrEmpty(x.DefaultContainer), () =>
            {
                RuleFor(x => x.DefaultContainer)
                    .Must((config, container) => 
                        config.Containers.Any(c => c.Name == container))
                    .WithMessage("Default container must exist in the containers list");
            });
        }
    }
    
    /// <summary>
    /// Validator for data container definition
    /// </summary>
    public class DataContainerDefinitionValidator : AbstractValidator<DataContainerDefinition>
    {
        public DataContainerDefinitionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Container name is required");
                
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid container type");
                
            RuleForEach(x => x.Properties)
                .SetValidator(new DataPropertyDefinitionValidator());
                
            RuleFor(x => x.IdentityFields)
                .Must((container, identityFields) =>
                    identityFields.All(field => 
                        container.Properties.Any(p => p.Name == field)))
                .WithMessage("All identity fields must exist in properties");
        }
    }
    
    /// <summary>
    /// Validator for data property definition
    /// </summary>
    public class DataPropertyDefinitionValidator : AbstractValidator<DataPropertyDefinition>
    {
        public DataPropertyDefinitionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Property name is required");
                
            RuleFor(x => x.DataType)
                .NotNull()
                .WithMessage("Data type is required");
                
            RuleFor(x => x.Classification)
                .IsInEnum()
                .WithMessage("Invalid property classification");
                
            When(x => x.MaxLength.HasValue, () =>
            {
                RuleFor(x => x.MaxLength!.Value)
                    .GreaterThan(0)
                    .WithMessage("Max length must be greater than 0");
            });
            
            RuleFor(x => x)
                .Must(x => !(x.IsRequired && x.IsNullable))
                .WithMessage("Required properties cannot be nullable");
        }
    }
    
    /// <summary>
    /// Definition of a data container (table, file, etc.)
    /// </summary>
    public record DataContainerDefinition
    {
        /// <summary>
        /// The container name
        /// </summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>
        /// The container type (Table, File, Collection, etc.)
        /// </summary>
        public DataContainerType Type { get; init; }
        
        /// <summary>
        /// Properties/columns in this container
        /// </summary>
        public List<DataPropertyDefinition> Properties { get; init; } = new();
        
        /// <summary>
        /// Fields that form the identity/primary key
        /// </summary>
        public List<string> IdentityFields { get; init; } = new();
        
        /// <summary>
        /// Schema name (for databases)
        /// </summary>
        public string? SchemaName { get; init; }
        
        /// <summary>
        /// Additional container-specific metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    /// <summary>
    /// Definition of a data property/column
    /// </summary>
    public record DataPropertyDefinition
    {
        /// <summary>
        /// The property name
        /// </summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>
        /// The data type
        /// </summary>
        public Type DataType { get; init; } = typeof(object);
        
        /// <summary>
        /// Whether this property is required
        /// </summary>
        public bool IsRequired { get; init; }
        
        /// <summary>
        /// Whether this property can be null
        /// </summary>
        public bool IsNullable { get; init; } = true;
        
        /// <summary>
        /// The classification of this property
        /// </summary>
        public PropertyClassification Classification { get; init; }
        
        /// <summary>
        /// Maximum length (for strings)
        /// </summary>
        public int? MaxLength { get; init; }
        
        /// <summary>
        /// Additional property-specific metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
    
    /// <summary>
    /// Types of data containers
    /// </summary>
    public enum DataContainerType
    {
        /// <summary>
        /// Database table
        /// </summary>
        Table,
        
        /// <summary>
        /// File (JSON, XML, CSV, etc.)
        /// </summary>
        File,
        
        /// <summary>
        /// NoSQL collection
        /// </summary>
        Collection,
        
        /// <summary>
        /// Partitioned table/file
        /// </summary>
        Partition,
        
        /// <summary>
        /// API endpoint
        /// </summary>
        Endpoint,
        
        /// <summary>
        /// Stream/queue
        /// </summary>
        Stream
    }
    
    /// <summary>
    /// Property classification for data modeling
    /// </summary>
    public enum PropertyClassification
    {
        /// <summary>
        /// Identity/primary key field
        /// </summary>
        Identity,
        
        /// <summary>
        /// Descriptive attribute
        /// </summary>
        Attribute,
        
        /// <summary>
        /// Measurable/numeric value
        /// </summary>
        Measure,
        
        /// <summary>
        /// Foreign key reference
        /// </summary>
        Reference,
        
        /// <summary>
        /// Calculated/derived field
        /// </summary>
        Calculated
    }
}