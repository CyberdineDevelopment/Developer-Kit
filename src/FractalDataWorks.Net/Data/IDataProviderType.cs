using System;
using FractalDataWorks.Connections;

namespace FractalDataWorks.Data;

/// <summary>
/// Enhanced enum interface for auto-discovery of data providers
/// Each provider package will implement this to self-register
/// </summary>
public interface IDataProviderType : IEnhancedEnum
{
    /// <summary>
    /// The display name of the provider
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// The connection type that implements IDataConnection
    /// </summary>
    Type ConnectionType { get; }
    
    /// <summary>
    /// The translator type for bidirectional command translation
    /// </summary>
    Type TranslatorType { get; }
    
    /// <summary>
    /// The configuration type for this provider
    /// </summary>
    Type ConfigurationType { get; }
    
    /// <summary>
    /// Provider capabilities
    /// </summary>
    ProviderCapabilities Capabilities { get; }
}