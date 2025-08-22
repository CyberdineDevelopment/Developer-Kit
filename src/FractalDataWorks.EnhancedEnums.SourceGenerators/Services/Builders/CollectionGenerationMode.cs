using System;

namespace FractalDataWorks.EnhancedEnums.SourceGenerators.Services.Builders;

/// <summary>
/// Defines the different generation modes for enhanced enum collections.
/// Each mode determines the architectural pattern and instantiation strategy for the generated collection classes.
/// </summary>
public enum CollectionGenerationMode
{
    /// <summary>
    /// Generates a static collection class with all static methods and fields.
    /// This is the most efficient pattern for read-only enum collections.
    /// All enum instances are pre-created and cached as static readonly fields.
    /// </summary>
    StaticCollection,

    /// <summary>
    /// Generates an instance-based collection using the singleton pattern.
    /// The collection class has a single static instance accessed via a property or method.
    /// Enum instances are created once and reused through the singleton instance.
    /// Suitable for scenarios requiring instance methods while maintaining single instantiation.
    /// </summary>
    InstanceCollection,

    /// <summary>
    /// Generates a factory-based collection that creates new instances on demand.
    /// Each call to collection methods returns newly created enum instances.
    /// This pattern is useful when enum instances need to be mutable or when
    /// different instances are required for different contexts.
    /// </summary>
    FactoryCollection,

    /// <summary>
    /// Generates a collection designed for dependency injection scenarios.
    /// The collection class implements appropriate interfaces and can be registered
    /// with DI containers. Supports both singleton and transient lifetimes
    /// depending on registration configuration.
    /// </summary>
    ServiceCollection
}