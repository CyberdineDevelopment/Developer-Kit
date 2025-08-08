using FractalDataWorks.Data;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.DataProviders.Abstractions;

/// <summary>
/// Non-generic marker interface for data services.
/// </summary>
public interface IDataService : IFdwService
{
}

/// <summary>
/// Interface for data services in the FractalDataWorks framework.
/// Provides a unified interface for executing data commands against various data sources.
/// </summary>
/// <typeparam name="TDataCommand">The data command type.</typeparam>
/// <remarks>
/// Data services abstract the complexity of different data access technologies
/// and provide a consistent command execution interface. They handle connection management,
/// transaction coordination, and result processing for data operations.
/// </remarks>
public interface IDataService<TDataCommand> : IDataService, IFdwService<TDataCommand>
    where TDataCommand : IDataCommand
{
}

