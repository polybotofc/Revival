using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Revival.Models;

/// <summary>
/// Represents an authentication session for a user.
/// Contains the session ticket and expiration information.
/// </summary>
public class Session
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(64)]
    public string SessionToken { get; set; } = string.Empty;

    [Required]
    public int UserId { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}