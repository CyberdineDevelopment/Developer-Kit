using FractalDataWorks;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions;

/// <summary>
/// Configuration interface for external connection services.
/// Acts as a marker interface for external connection configurations.
/// Validation is provided by the source generator extension methods.
/// </summary>
public interface IExternalConnectionConfiguration : IFdwConfiguration
{
}