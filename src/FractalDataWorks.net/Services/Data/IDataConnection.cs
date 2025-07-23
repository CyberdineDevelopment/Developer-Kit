using System;
using System.Collections.Generic;
using System.Text;

namespace FractalDataWorks.Services.Data;

/// <summary>
/// A Service that provides a common interface for accessing data from external connections
/// </summary>
public interface IDataConnection : IFdwService<IDataCommand> 
{

}

/// <summary>
/// A Service that provides a common interface for accessing data from external connections with a command type
/// </summary>
/// <typeparam name="TCommand">The type of data command expected</typeparam>
public interface IDataConnection<TCommand> : IDataConnection
where TCommand : IDataCommand
{

}