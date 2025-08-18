using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FractalDataWorks.Samples.MsSqlProvider.Models;

/// <summary>
/// Represents user activity logging for analytics and audit purposes.
/// Maps to users.UserActivity table in the database.
/// </summary>
[Table("UserActivity", Schema = "users")]
public sealed class UserActivity
{
    /// <summary>
    /// Unique identifier for the activity record
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// User identifier who performed the activity
    /// </summary>
    [Required]
    [StringLength(50)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Type of activity performed
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ActivityType { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the activity occurred
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if the activity was successful
    /// </summary>
    [Required]
    public bool IsSuccessful { get; set; } = true;

    /// <summary>
    /// Additional details about the activity (JSON or text)
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Details { get; set; }

    /// <summary>
    /// Session identifier for grouping related activities
    /// </summary>
    [StringLength(100)]
    public string? SessionId { get; set; }

    /// <summary>
    /// IP address of the user
    /// </summary>
    [StringLength(45)] // IPv6 support
    public string? IPAddress { get; set; }

    /// <summary>
    /// User agent string from the client
    /// </summary>
    [StringLength(500)]
    public string? UserAgent { get; set; }
}