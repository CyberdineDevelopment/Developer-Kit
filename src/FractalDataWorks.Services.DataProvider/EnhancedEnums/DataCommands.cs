using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;

namespace FractalDataWorks.Services.DataProvider.EnhancedEnums;

/// <summary>
/// Enhanced Enum factory for creating provider-agnostic data commands with fluent syntax.
/// </summary>
/// <remarks>
/// DataCommands provides a unified factory for creating data operations that work across
/// all data providers (SQL, NoSQL, File, API, etc.). The fluent syntax enables natural
/// command composition: DataCommands.Query&lt;Customer&gt;(c => c.IsActive).WithConnection("DB").
/// </remarks>
public static class DataCommands
{
    /// <summary>
    /// Creates a query command for retrieving data records.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to query.</typeparam>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>A new QueryCommand instance.</returns>
    /// <example>
    /// <code>
    /// var customers = await dataProvider.Execute(
    ///     DataCommands.Query&lt;Customer&gt;(c => c.IsActive, "ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static QueryCommand<TEntity> Query<TEntity>(
        Expression<Func<TEntity, bool>>? predicate = null,
        string connectionName = "")
    {
        return new QueryCommand<TEntity>(connectionName, predicate);
    }

    /// <summary>
    /// Creates a query command for retrieving all records.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to query.</typeparam>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>A new QueryCommand instance.</returns>
    /// <example>
    /// <code>
    /// var allCustomers = await dataProvider.Execute(
    ///     DataCommands.QueryAll&lt;Customer&gt;("ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static QueryCommand<TEntity> QueryAll<TEntity>(string connectionName = "")
    {
        return new QueryCommand<TEntity>(connectionName);
    }

    /// <summary>
    /// Creates a query command that finds entities by ID.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to query.</typeparam>
    /// <typeparam name="TId">The type of the ID field.</typeparam>
    /// <param name="id">The ID value to search for.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <param name="idFieldName">The name of the ID field (default: "Id").</param>
    /// <returns>A new QueryCommand instance.</returns>
    /// <example>
    /// <code>
    /// var customer = await dataProvider.Execute(
    ///     DataCommands.QueryById&lt;Customer, int&gt;(123, "ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static QueryCommand<TEntity> QueryById<TEntity, TId>(
        TId id,
        string connectionName = "",
        string idFieldName = "Id")
    {
        // Create a predicate that compares the ID field to the provided value
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var property = Expression.Property(parameter, idFieldName);
        var constant = Expression.Constant(id, typeof(TId));
        var equals = Expression.Equal(property, constant);
        var predicate = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

        return new QueryCommand<TEntity>(connectionName, predicate);
    }

    /// <summary>
    /// Creates a count command for counting records.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to count.</typeparam>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>A new CountCommand instance.</returns>
    /// <example>
    /// <code>
    /// var activeCount = await dataProvider.Execute(
    ///     DataCommands.Count&lt;Customer&gt;(c => c.IsActive, "ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static CountCommand<TEntity> Count<TEntity>(
        Expression<Func<TEntity, bool>>? predicate = null,
        string connectionName = "")
    {
        return new CountCommand<TEntity>(connectionName, predicate);
    }

    /// <summary>
    /// Creates an exists command for checking if records exist.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to check.</typeparam>
    /// <param name="predicate">The filter predicate.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>A new ExistsCommand instance.</returns>
    /// <example>
    /// <code>
    /// var hasActiveCustomers = await dataProvider.Execute(
    ///     DataCommands.Exists&lt;Customer&gt;(c => c.IsActive, "ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static ExistsCommand<TEntity> Exists<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        string connectionName = "")
    {
        return new ExistsCommand<TEntity>(connectionName, predicate);
    }

    /// <summary>
    /// Creates an insert command for adding a new record.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to insert.</typeparam>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>A new InsertCommand instance.</returns>
    /// <example>
    /// <code>
    /// var newCustomer = new Customer { Name = "John Doe", Email = "john@example.com" };
    /// var result = await dataProvider.Execute(
    ///     DataCommands.Insert(newCustomer, "ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static InsertCommand<TEntity> Insert<TEntity>(
        TEntity entity,
        string connectionName = "")
        where TEntity : class
    {
        return new InsertCommand<TEntity>(connectionName, entity);
    }

    /// <summary>
    /// Creates a bulk insert command for adding multiple records efficiently.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to insert.</typeparam>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <param name="batchSize">The batch size for bulk operations.</param>
    /// <returns>A new BulkInsertCommand instance.</returns>
    /// <example>
    /// <code>
    /// var customers = new List&lt;Customer&gt; { customer1, customer2, customer3 };
    /// var rowsInserted = await dataProvider.Execute(
    ///     DataCommands.BulkInsert(customers, "ProductionDB", 500)
    /// );
    /// </code>
    /// </example>
    public static BulkInsertCommand<TEntity> BulkInsert<TEntity>(
        IEnumerable<TEntity> entities,
        string connectionName = "",
        int batchSize = 1000)
        where TEntity : class
    {
        return new BulkInsertCommand<TEntity>(connectionName, entities, batchSize: batchSize);
    }

    /// <summary>
    /// Creates an update command for modifying existing records.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to update.</typeparam>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="predicate">The predicate to identify records to update.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>A new UpdateCommand instance.</returns>
    /// <example>
    /// <code>
    /// var updatedCustomer = new Customer { Name = "Jane Doe", Email = "jane@example.com" };
    /// var rowsUpdated = await dataProvider.Execute(
    ///     DataCommands.Update(updatedCustomer, c => c.Id == 123, "ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static UpdateCommand<TEntity> Update<TEntity>(
        TEntity entity,
        Expression<Func<TEntity, bool>> predicate,
        string connectionName = "")
        where TEntity : class
    {
        return new UpdateCommand<TEntity>(connectionName, entity, predicate);
    }

    /// <summary>
    /// Creates an update command for updating entities by ID.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to update.</typeparam>
    /// <typeparam name="TId">The type of the ID field.</typeparam>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <param name="idFieldName">The name of the ID field (default: "Id").</param>
    /// <returns>A new UpdateCommand instance.</returns>
    /// <example>
    /// <code>
    /// var updatedCustomer = new Customer { Name = "Jane Doe", Email = "jane@example.com" };
    /// var rowsUpdated = await dataProvider.Execute(
    ///     DataCommands.UpdateById(updatedCustomer, 123, "ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static UpdateCommand<TEntity> UpdateById<TEntity, TId>(
        TEntity entity,
        TId id,
        string connectionName = "",
        string idFieldName = "Id")
        where TEntity : class
    {
        // Create a predicate that compares the ID field to the provided value
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var property = Expression.Property(parameter, idFieldName);
        var constant = Expression.Constant(id, typeof(TId));
        var equals = Expression.Equal(property, constant);
        var predicate = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

        return new UpdateCommand<TEntity>(connectionName, entity, predicate);
    }

    /// <summary>
    /// Creates a partial update command for modifying specific fields.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to update.</typeparam>
    /// <param name="updates">The field updates to apply.</param>
    /// <param name="predicate">The predicate to identify records to update.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>A new PartialUpdateCommand instance.</returns>
    /// <example>
    /// <code>
    /// var updates = new Dictionary&lt;string, object?&gt; { ["Name"] = "Jane Doe", ["Email"] = "jane@example.com" };
    /// var rowsUpdated = await dataProvider.Execute(
    ///     DataCommands.PartialUpdate&lt;Customer&gt;(updates, c => c.Id == 123, "ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static PartialUpdateCommand<TEntity> PartialUpdate<TEntity>(
        IDictionary<string, object?> updates,
        Expression<Func<TEntity, bool>> predicate,
        string connectionName = "")
        where TEntity : class
    {
        return new PartialUpdateCommand<TEntity>(connectionName, updates, predicate);
    }

    /// <summary>
    /// Creates a delete command for removing records.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to delete.</typeparam>
    /// <param name="predicate">The predicate to identify records to delete.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>A new DeleteCommand instance.</returns>
    /// <example>
    /// <code>
    /// var rowsDeleted = await dataProvider.Execute(
    ///     DataCommands.Delete&lt;Customer&gt;(c => !c.IsActive, "ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static DeleteCommand<TEntity> Delete<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        string connectionName = "")
        where TEntity : class
    {
        return new DeleteCommand<TEntity>(connectionName, predicate);
    }

    /// <summary>
    /// Creates a delete command for removing entities by ID.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to delete.</typeparam>
    /// <typeparam name="TId">The type of the ID field.</typeparam>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <param name="idFieldName">The name of the ID field (default: "Id").</param>
    /// <returns>A new DeleteCommand instance.</returns>
    /// <example>
    /// <code>
    /// var rowsDeleted = await dataProvider.Execute(
    ///     DataCommands.DeleteById&lt;Customer, int&gt;(123, "ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static DeleteCommand<TEntity> DeleteById<TEntity, TId>(
        TId id,
        string connectionName = "",
        string idFieldName = "Id")
        where TEntity : class
    {
        // Create a predicate that compares the ID field to the provided value
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var property = Expression.Property(parameter, idFieldName);
        var constant = Expression.Constant(id, typeof(TId));
        var equals = Expression.Equal(property, constant);
        var predicate = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

        return new DeleteCommand<TEntity>(connectionName, predicate);
    }

    /// <summary>
    /// Creates a truncate command for removing all records from a container.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to truncate.</typeparam>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>A new TruncateCommand instance.</returns>
    /// <example>
    /// <code>
    /// var rowsDeleted = await dataProvider.Execute(
    ///     DataCommands.Truncate&lt;Customer&gt;("ProductionDB").Confirm()
    /// );
    /// </code>
    /// </example>
    public static TruncateCommand<TEntity> Truncate<TEntity>(string connectionName = "")
        where TEntity : class
    {
        return new TruncateCommand<TEntity>(connectionName);
    }

    /// <summary>
    /// Creates an upsert command for inserting or updating records.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to upsert.</typeparam>
    /// <param name="entity">The entity to upsert.</param>
    /// <param name="conflictFields">The fields used to detect conflicts.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <returns>A new UpsertCommand instance.</returns>
    /// <example>
    /// <code>
    /// var customer = new Customer { Id = 123, Name = "John Doe", Email = "john@example.com" };
    /// var result = await dataProvider.Execute(
    ///     DataCommands.Upsert(customer, new[] { "Id" }, "ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static UpsertCommand<TEntity> Upsert<TEntity>(
        TEntity entity,
        IEnumerable<string> conflictFields,
        string connectionName = "")
        where TEntity : class
    {
        return new UpsertCommand<TEntity>(connectionName, entity, conflictFields);
    }

    /// <summary>
    /// Creates an upsert command using the primary key for conflict detection.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to upsert.</typeparam>
    /// <param name="entity">The entity to upsert.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <param name="primaryKeyField">The primary key field name (default: "Id").</param>
    /// <returns>A new UpsertCommand instance.</returns>
    /// <example>
    /// <code>
    /// var customer = new Customer { Id = 123, Name = "John Doe", Email = "john@example.com" };
    /// var result = await dataProvider.Execute(
    ///     DataCommands.UpsertByPrimaryKey(customer, "ProductionDB")
    /// );
    /// </code>
    /// </example>
    public static UpsertCommand<TEntity> UpsertByPrimaryKey<TEntity>(
        TEntity entity,
        string connectionName = "",
        string primaryKeyField = "Id")
        where TEntity : class
    {
        return new UpsertCommand<TEntity>(connectionName, entity, new[] { primaryKeyField });
    }

    /// <summary>
    /// Creates a bulk upsert command for efficiently upserting multiple records.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to upsert.</typeparam>
    /// <param name="entities">The entities to upsert.</param>
    /// <param name="conflictFields">The fields used to detect conflicts.</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <param name="batchSize">The batch size for bulk operations.</param>
    /// <returns>A new BulkUpsertCommand instance.</returns>
    /// <example>
    /// <code>
    /// var customers = new List&lt;Customer&gt; { customer1, customer2, customer3 };
    /// var rowsAffected = await dataProvider.Execute(
    ///     DataCommands.BulkUpsert(customers, new[] { "Id" }, "ProductionDB", 500)
    /// );
    /// </code>
    /// </example>
    public static BulkUpsertCommand<TEntity> BulkUpsert<TEntity>(
        IEnumerable<TEntity> entities,
        IEnumerable<string> conflictFields,
        string connectionName = "",
        int batchSize = 1000)
        where TEntity : class
    {
        return new BulkUpsertCommand<TEntity>(connectionName, entities, conflictFields, batchSize: batchSize);
    }
}