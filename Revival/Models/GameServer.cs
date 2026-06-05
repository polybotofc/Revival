using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Revival.Models;

/// <summary>
/// Represents a running game server instance managed by RCCService.
/// Contains server connection information for clients.
/// </summary>
[Table("GameServers")]
public class GameServer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(36)]
    [Column(TypeName = "varchar(36)")]
    public string ServerId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column(TypeName = "int")]
    public int PlaceId { get; set; }

    [Required]
    [Column(TypeName = "int")]
    public int GameId { get; set; }

    [Required]
    [Column(TypeName = "int")]
    public int CreatorId { get; set; }

    [Required]
    [StringLength(45)]
    [Column(TypeName = "varchar(45)")]
    public string Host { get; set; } = string.Empty;

    [Column(TypeName = "int")]
    public int Port { get; set; } = 53640;

    [StringLength(64)]
    [Column(TypeName = "varchar(64)")]
    public string? TicketToken { get; set; }

    [Column(TypeName = "int")]
    public int CurrentPlayers { get; set; } = 0;

    [Column(TypeName = "int")]
    public int MaxPlayers { get; set; } = 50;

    [Required]
    [StringLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string Status { get; set; } = "Starting"; // Starting, Running, Stopping, Stopped, Crashed

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? RCCJobId { get; set; }

    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string? RCCServerUrl { get; set; }

    [Column(TypeName = "int")]
    public int CPUUsage { get; set; } = 0;

    [Column(TypeName = "bigint")]
    public long MemoryUsage { get; set; } = 0;

    [Column(TypeName = "varchar(50)")]
    public string? Region { get; set; }

    [Column(TypeName = "varchar(50)")]
    public string? MachineId { get; set; }

    [Column(TypeName = "tinyint(1)")]
    public bool IsReserved { get; set; } = false;

    [Column(TypeName = "int")]
    public int ReservedPlayerCount { get; set; } = 0;

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime")]
    public DateTime? StartedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExpiresAt { get; set; }

    [Column(TypeName = "int")]
    public int Ping { get; set; } = 0;

    // Navigation properties
    [JsonIgnore]
    [ForeignKey("PlaceId")]
    public virtual Place? Place { get; set; }

    [JsonIgnore]
    [ForeignKey("GameId")]
    public virtual Game? Game { get; set; }

    [JsonIgnore]
    [ForeignKey("CreatorId")]
    public virtual User? Creator { get; set; }
}