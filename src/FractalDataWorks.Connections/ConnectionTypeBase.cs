using System;
using System.Threading.Tasks;

namespace FractalDataWorks.Connections;

/// <summary>
/// Base class for connection type definitions.
/// Note: Enhanced Enum attributes temporarily removed due to compatibility issues.
/// </summary>
public abstract class ConnectionTypeBase<TConnection, TConfiguration> : ConnectionTypeFactoryBase<TConnection, TConfiguration>
    where TConnection : class, IExternalConnection
    where TConfiguration : class, IFdwConfiguration
{
    protected ConnectionTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }
}