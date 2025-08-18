using FractalDataWorks.Samples.DataProvider.DataModels;

namespace FractalDataWorks.Samples.DataProvider.Abstractions;

/// <summary>
/// Common repository interface that all data providers implement
/// </summary>
public interface IDataRepository
{
    string ProviderName { get; }
    string ConnectionInfo { get; }
    
    // Customer operations
    Task<IEnumerable<Customer>> GetCustomersAsync(CustomerQuery? query = null);
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<bool> UpdateCustomerAsync(Customer customer);
    Task<bool> DeleteCustomerAsync(int id);
    
    // Product operations
    Task<IEnumerable<Product>> GetProductsAsync(ProductQuery? query = null);
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(int id);
    
    // Order operations
    Task<IEnumerable<Order>> GetOrdersAsync(OrderQuery? query = null);
    Task<Order?> GetOrderByIdAsync(int id);
    Task<Order> CreateOrderAsync(Order order, List<OrderItem> items);
    Task<bool> UpdateOrderStatusAsync(int orderId, string status);
    
    // Analytics
    Task<DataStatistics> GetStatisticsAsync();
    Task<decimal> GetCustomerTotalPurchasesAsync(int customerId);
    
    // Transaction support
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    
    // Health check
    Task<bool> IsHealthyAsync();
}