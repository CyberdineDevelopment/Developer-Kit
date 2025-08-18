namespace FractalDataWorks.Samples.DataProvider.DataModels;

// Common data models that work across all providers
public record Customer(
    int Id,
    string Name,
    string Email,
    string City,
    string Country,
    DateTime CreatedDate
);

public record Product(
    int Id,
    string Name,
    string Category,
    decimal Price,
    int StockQuantity,
    bool IsActive
);

public record Order(
    int Id,
    int CustomerId,
    DateTime OrderDate,
    decimal TotalAmount,
    string Status,
    DateTime? ProcessedDate
);

public record OrderItem(
    int Id,
    int OrderId,
    int ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);

// Query/Command models
public record CustomerQuery(
    string? NameFilter = null,
    string? Country = null,
    DateTime? CreatedAfter = null
);

public record ProductQuery(
    string? Category = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? ActiveOnly = null
);

public record OrderQuery(
    int? CustomerId = null,
    string? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);

// Statistics model
public record DataStatistics(
    int TotalCustomers,
    int TotalProducts,
    int TotalOrders,
    decimal TotalRevenue,
    Dictionary<string, int> OrdersByStatus,
    Dictionary<string, int> ProductsByCategory
);