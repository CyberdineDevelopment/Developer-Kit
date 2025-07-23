using System;
using System.Collections.Generic;
using System.Text;
using FractalDataWorks.Configuration;

namespace FractalDataWorks.Services.Data;
public abstract class DataConnectionBase<TCommand, TConnection, TConfiguration> : ServiceBase<TCommand, TConfiguration, DataConnectionBase<TCommand, TConnection, TConfiguration>>
where TCommand : IDataCommand
where TConfiguration : ConfigurationBase<TConfiguration>, new()
where TConnection : class
{
}
