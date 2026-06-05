using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Revival.Models;

/// <summary>
/// Represents a user in the Roblox Revival platform.
/// Contains user authentication and profile information.
/// </summary>
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(255)]
    public string? DisplayName { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public bool IsBanned { get; set; } = false;

    public int? AvatarAssetId { get; set; }

    // Navigation properties
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<Game> CreatedGames { get; set; } = new List<Game>();
    public virtual ICollection<GameServer> GameServers { get; set; } = new List<GameServer>();
}