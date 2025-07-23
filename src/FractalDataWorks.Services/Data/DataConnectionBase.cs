using System;
using System.Collections.Generic;
using System.Text;

namespace FractalDataWorks.Services.Data;
public abstract class DataConnectionBase<TCommand, TConnection, TConfiguration> : ServiceBase<TCommand>
where TCommand : IDataCommand
{
}
