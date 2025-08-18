using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FractalDataWorks.Samples.MsSqlProvider.Models;

/// <summary>
/// Represents an order in the sales system.
/// Maps to sales.Orders table in the database.
/// </summary>
[Table("Orders", Schema = "sales")]
public sealed class Order
{
    /// <summary>
    /// Unique identifier for the order
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// ID of the customer who placed the order
    /// </summary>
    [Required]
    public int CustomerId { get; set; }

    /// <summary>
    /// Date when the order was placed
    /// </summary>
    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Current status of the order
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = nameof(OrderStatus.Pending);

    /// <summary>
    /// Total amount of the order
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Total amount must be non-negative")]
    public decimal TotalAmount { get; set; } = 0.00m;

    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    [Timestamp]
    public byte[] Version { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Navigation property for the customer who placed the order
    /// </summary>
    [ForeignKey(nameof(CustomerId))]
    public Customer Customer { get; set; } = null!;

    /// <summary>
    /// Gets the order status as an enum
    /// </summary>
    [NotMapped]
    public OrderStatus StatusEnum
    {
        get => Enum.TryParse<OrderStatus>(Status, out var status) ? status : OrderStatus.Pending;
        set => Status = value.ToString();
    }
}