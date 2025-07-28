using System;
using FractalDataWorks.Data;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Connections.Data;

/// <summary>
/// Abstract base class for command translators
/// </summary>
/// <typeparam name="TNative">The native command type (SqlCommand, HttpRequest, etc.)</typeparam>
public abstract class CommandTranslatorBase<TNative> : ICommandTranslator<IDataCommand, TNative>
{
    protected readonly ILogger _logger;
    
    protected CommandTranslatorBase(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <inheritdoc/>
    public virtual TNative Translate(IDataCommand command)
    {
        if (!CanTranslate(command))
            throw new InvalidOperationException($"Cannot translate command of type {command.GetType().Name}");
        
        _logger.LogDebug("Translating {CommandType} to {NativeType}", 
            command.GetType().Name, typeof(TNative).Name);
        
        return command switch
        {
            IQueryCommand<object> queryCmd => TranslateQuery(queryCmd),
            IInsertCommand<object> insertCmd => TranslateInsert(insertCmd),
            IUpdateCommand<object> updateCmd => TranslateUpdate(updateCmd),
            IDeleteCommand<object> deleteCmd => TranslateDelete(deleteCmd),
            _ => throw new NotSupportedException($"Command type {command.GetType().Name} not supported")
        };
    }
    
    /// <inheritdoc/>
    public abstract IDataCommand Parse(TNative native);
    
    /// <inheritdoc/>
    public virtual bool CanTranslate(IDataCommand command)
    {
        return command != null && 
               !string.IsNullOrEmpty(command.DataStore) && 
               !string.IsNullOrEmpty(command.Record);
    }
    
    /// <summary>
    /// Translates a query command to native format
    /// </summary>
    protected abstract TNative TranslateQuery(IQueryCommand<object> command);
    
    /// <summary>
    /// Translates an insert command to native format
    /// </summary>
    protected abstract TNative TranslateInsert(IInsertCommand<object> command);
    
    /// <summary>
    /// Translates an update command to native format
    /// </summary>
    protected abstract TNative TranslateUpdate(IUpdateCommand<object> command);
    
    /// <summary>
    /// Translates a delete command to native format
    /// </summary>
    protected abstract TNative TranslateDelete(IDeleteCommand<object> command);
}