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