using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FractalDataWorks.Samples.MsSqlProvider.Models;

/// <summary>
/// Represents a customer in the sales system.
/// Maps to sales.Customers table in the database.
/// </summary>
[Table("Customers", Schema = "sales")]
public sealed class Customer
{
    /// <summary>
    /// Unique identifier for the customer
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Customer's name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Customer's email address (must be unique)
    /// </summary>
    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Date when the customer record was created
    /// </summary>
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if the customer account is active
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Customer's credit limit
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Credit limit must be non-negative")]
    public decimal CreditLimit { get; set; } = 0.00m;

    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    [Timestamp]
    public byte[] Version { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Navigation property for orders placed by this customer
    /// </summary>
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}