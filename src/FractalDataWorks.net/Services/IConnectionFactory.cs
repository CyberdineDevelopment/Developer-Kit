using System.Threading.Tasks;

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