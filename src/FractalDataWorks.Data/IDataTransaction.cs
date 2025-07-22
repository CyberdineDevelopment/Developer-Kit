using System;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data;

/// <summary>
/// Defines the contract for data transactions.
/// </summary>
public interface IDataTransaction : IDisposable
{
    /// <summary>
    /// Gets the transaction ID.
    /// </summary>
    Guid TransactionId { get; }

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    /// <returns>A task representing the commit operation.</returns>
    Task<IFdwResult<NonResult>> Commit();

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    /// <returns>A task representing the rollback operation.</returns>
    Task<IFdwResult<NonResult>> Rollback();
}