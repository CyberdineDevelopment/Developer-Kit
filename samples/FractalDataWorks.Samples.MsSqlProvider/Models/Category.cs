using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FractalDataWorks.Samples.MsSqlProvider.Models;

/// <summary>
/// Represents a product category with hierarchical structure support.
/// Maps to inventory.Categories table in the database.
/// </summary>
[Table("Categories", Schema = "inventory")]
public sealed class Category
{
    /// <summary>
    /// Unique identifier for the category
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Parent category ID for hierarchical structure (null for root categories)
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Category description
    /// </summary>
    [StringLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// Date when the category was created
    /// </summary>
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if the category is active
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for parent category
    /// </summary>
    [ForeignKey(nameof(ParentId))]
    public Category? Parent { get; set; }

    /// <summary>
    /// Navigation property for child categories
    /// </summary>
    public ICollection<Category> Children { get; set; } = new List<Category>();

    /// <summary>
    /// Navigation property for products in this category
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}