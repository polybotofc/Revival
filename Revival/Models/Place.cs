using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Revival.Models;

/// <summary>
/// Represents a place (level/game world) within a game.
/// Each place has its own .rbxl file and can be launched independently.
/// </summary>
public class Place
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int GameId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public int MaxPlayers { get; set; } = 50;

    public int CurrentPlayers { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("GameId")]
    public virtual Game? Game { get; set; }

    public virtual ICollection<GameServer> GameServers { get; set; } = new List<GameServer>();
}