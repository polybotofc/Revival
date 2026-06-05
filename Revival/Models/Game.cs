using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Revival.Models;

/// <summary>
/// Represents a game/experience in the Roblox Revival platform.
/// A game can contain multiple places (levels).
/// </summary>
public class Game
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public int CreatorId { get; set; }

    [StringLength(255)]
    public string? ThumbnailUrl { get; set; }

    public int TotalPlays { get; set; } = 0;

    public int MonthlyPlays { get; set; } = 0;

    public int ActivePlayers { get; set; } = 0;

    public bool IsPublic { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("CreatorId")]
    public virtual User? Creator { get; set; }

    public virtual ICollection<Place> Places { get; set; } = new List<Place>();
    public virtual ICollection<GameServer> GameServers { get; set; } = new List<GameServer>();
}