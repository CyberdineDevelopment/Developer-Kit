using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FractalDataWorks.Samples.MsSqlProvider.Models;

/// <summary>
/// Represents a product in the inventory system.
/// Maps to inventory.Products table in the database.
/// </summary>
[Table("Products", Schema = "inventory")]
public sealed class Product
{
    /// <summary>
    /// Unique identifier for the product
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product price
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
    public decimal Price { get; set; }

    /// <summary>
    /// Category ID this product belongs to
    /// </summary>
    [Required]
    public int CategoryId { get; set; }

    /// <summary>
    /// Indicates if the product is currently in stock
    /// </summary>
    [Required]
    public bool InStock { get; set; } = true;

    /// <summary>
    /// Date when the product was created
    /// </summary>
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Product description
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Stock Keeping Unit (unique identifier)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property for the product category
    /// </summary>
    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;
}