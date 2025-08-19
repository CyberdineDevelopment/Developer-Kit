using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace FractalDataWorks.Services.ExternalConnections.MsSql;

/// <summary>
/// Represents the result of translating a DataCommandBase to SQL.
/// </summary>
internal sealed class SqlTranslationResult
{
    public SqlTranslationResult(string sql, IReadOnlyList<SqlParameter> parameters)
    {
        Sql = sql ?? throw new ArgumentNullException(nameof(sql));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    /// <summary>
    /// Gets the generated SQL statement.
    /// </summary>
    public string Sql { get; }

    /// <summary>
    /// Gets the SQL parameters.
    /// </summary>
    public IReadOnlyList<SqlParameter> Parameters { get; }
}