# FractalDataWorks.Data

Data access abstractions and common data types for the FractalDataWorks framework.

## Overview

FractalDataWorks.Data provides:
- Repository pattern abstractions
- Unit of Work pattern
- Query specification pattern
- Common data types and entities
- Data access result patterns

## Planned Components

### IRepository<T>

Generic repository interface:
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(ISpecification<T> specification);
    Task<FractalResult<T>> AddAsync(T entity);
    Task<FractalResult<T>> UpdateAsync(T entity);
    Task<FractalResult> DeleteAsync(T entity);
    Task<bool> ExistsAsync(object id);
}
```

### IUnitOfWork

Unit of Work pattern for transactional operations:
```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    Task<FractalResult<int>> SaveChangesAsync();
    Task<FractalResult> BeginTransactionAsync();
    Task<FractalResult> CommitAsync();
    Task<FractalResult> RollbackAsync();
}
```

### ISpecification<T>

Specification pattern for complex queries:
```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int? Take { get; }
    int? Skip { get; }
    bool IsPagingEnabled { get; }
}
```

### Base Entity Types

```csharp
public interface IEntity
{
    object Id { get; }
}

public interface IEntity<TId> : IEntity
{
    new TId Id { get; set; }
}

public abstract class EntityBase<TId> : IEntity<TId>
{
    public TId Id { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    string CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
```

## Planned Features

### Specification Builder
```csharp
public class SpecificationBuilder<T> : ISpecification<T>
{
    public SpecificationBuilder<T> Where(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
        return this;
    }
    
    public SpecificationBuilder<T> Include(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
        return this;
    }
    
    public SpecificationBuilder<T> OrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
        return this;
    }
    
    public SpecificationBuilder<T> WithPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
        return this;
    }
}
```

### Paged Results
```csharp
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    
    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items.ToList().AsReadOnly();
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
```

### Query Extensions
```csharp
public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source, 
        int pageNumber, 
        int pageSize)
    {
        var totalCount = await source.CountAsync();
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}
```

## Usage Examples (Planned)

### Repository Implementation
```csharp
public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;
    
    public EfRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public async Task<IEnumerable<T>> FindAsync(ISpecification<T> specification)
    {
        var query = ApplySpecification(specification);
        return await query.ToListAsync();
    }
    
    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        var query = _dbSet.AsQueryable();
        
        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);
            
        query = spec.Includes.Aggregate(query, 
            (current, include) => current.Include(include));
            
        if (spec.OrderBy != null)
            query = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending != null)
            query = query.OrderByDescending(spec.OrderByDescending);
            
        if (spec.IsPagingEnabled)
            query = query.Skip(spec.Skip ?? 0).Take(spec.Take ?? 10);
            
        return query;
    }
}
```

### Using Specifications
```csharp
public class ActiveCustomersSpecification : SpecificationBuilder<Customer>
{
    public ActiveCustomersSpecification()
    {
        Where(c => c.IsActive && !c.IsDeleted);
        Include(c => c.Orders);
        OrderByDescending(c => c.CreatedAt);
    }
}

// Usage
var activeCustomers = await repository.FindAsync(new ActiveCustomersSpecification());
```

### Unit of Work Usage
```csharp
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<FractalResult> CreateOrderAsync(Order order)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            var customerRepo = _unitOfWork.Repository<Customer>();
            var orderRepo = _unitOfWork.Repository<Order>();
            
            var customer = await customerRepo.GetByIdAsync(order.CustomerId);
            if (customer == null)
                return FractalResult.Failure("Customer not found");
                
            await orderRepo.AddAsync(order);
            customer.TotalOrders++;
            await customerRepo.UpdateAsync(customer);
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            
            return FractalResult.Success();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
```

## Installation

```xml
<PackageReference Include="FractalDataWorks.Data" Version="*" />
```

## Dependencies

- FractalDataWorks.net (core abstractions)
- System.Linq.Expressions

## Status

This package is currently in planning phase. The interfaces and implementations described above represent the intended design and may change during development.

## Contributing

This package is accepting contributions for:
- Repository pattern implementations
- Specification pattern enhancements
- Common entity base classes
- Data access utilities
- Unit and integration tests