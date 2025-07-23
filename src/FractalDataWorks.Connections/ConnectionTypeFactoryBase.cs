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