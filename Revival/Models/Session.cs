using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Revival.Models;

/// <summary>
/// Represents an authentication session ticket for a user.
/// Contains the session token, expiration, and tracking information.
/// </summary>
[Table("Sessions")]
public class UserSession
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(128)]
    [Column(TypeName = "varchar(128)")]
    public string SessionToken { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "int")]
    public int UserId { get; set; }

    [StringLength(45)]
    [Column(TypeName = "varchar(45)")]
    public string? IpAddress { get; set; }

    [StringLength(512)]
    [Column(TypeName = "varchar(512)")]
    public string? UserAgent { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? DeviceType { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime")]
    public DateTime ExpiresAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastAccessedAt { get; set; }

    [Column(TypeName = "tinyint(1)")]
    public bool IsActive { get; set; } = true;

    [Column(TypeName = "tinyint(1)")]
    public bool IsPersistent { get; set; } = false;

    [Column(TypeName = "varchar(50)")]
    public string? Status { get; set; } = "Active";

    // Navigation property
    [JsonIgnore]
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}