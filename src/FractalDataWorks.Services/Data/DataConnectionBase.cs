using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using FractalDataWorks.Configuration;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Data;

/// <summary>
/// Base class for data connection implementations.
/// </summary>
/// <ExcludeFromTest>Abstract base class for data connections with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage(Justification = "Abstract base class for data connections with no business logic")]
public abstract class DataConnectionBase<TCommand, TConnection, TConfiguration> : ServiceBase<TCommand, TConfiguration, DataConnectionBase<TCommand, TConnection, TConfiguration>>
where TCommand : IDataCommand
where TConfiguration : ConfigurationBase<TConfiguration>, new()
where TConnection : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataConnectionBase{TCommand, TConnection, TConfiguration}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configurations">The configuration registry.</param>
    protected DataConnectionBase(
        ILogger<DataConnectionBase<TCommand, TConnection, TConfiguration>>? logger,
        TConfiguration configurations)
        : base(logger, configurations)
    {
    }
}
