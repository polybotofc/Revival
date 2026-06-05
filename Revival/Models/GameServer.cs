using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Revival.Models;

/// <summary>
/// Represents a running game server instance managed by RCCService.
/// Contains server connection information for clients.
/// </summary>
public class GameServer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(36)]
    public string ServerId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public int PlaceId { get; set; }

    [Required]
    public int GameId { get; set; }

    [Required]
    public int CreatorId { get; set; }

    [Required]
    [StringLength(45)]
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    [StringLength(64)]
    public string? TicketToken { get; set; }

    public int CurrentPlayers { get; set; } = 0;

    public int MaxPlayers { get; set; } = 50;

    [StringLength(20)]
    public string Status { get; set; } = "Starting"; // Starting, Running, Stopping, Stopped

    [StringLength(500)]
    public string? RCCJobId { get; set; }

    [StringLength(500)]
    public string? RCCServerUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    [ForeignKey("PlaceId")]
    public virtual Place? Place { get; set; }

    [ForeignKey("GameId")]
    public virtual Game? Game { get; set; }

    [ForeignKey("CreatorId")]
    public virtual User? Creator { get; set; }
}