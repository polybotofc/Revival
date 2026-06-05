using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Revival.Models;

/// <summary>
/// Represents a place (level/game world) within a game.
/// Each place has its own .rbxl file and can be launched independently.
/// </summary>
[Table("Places")]
public class Place
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    [Column(TypeName = "varchar(100)")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "int")]
    public int GameId { get; set; }

    [StringLength(2000)]
    [Column(TypeName = "varchar(2000)")]
    public string? Description { get; set; }

    [Required]
    [StringLength(500)]
    [Column(TypeName = "varchar(500)")]
    public string FilePath { get; set; } = string.Empty;

    [Column(TypeName = "int")]
    public int MaxPlayers { get; set; } = 50;

    [Column(TypeName = "int")]
    public int MinPlayers { get; set; } = 1;

    [Column(TypeName = "int")]
    public int CurrentPlayers { get; set; } = 0;

    [Column(TypeName = "tinyint(1)")]
    public bool IsActive { get; set; } = true;

    [Column(TypeName = "tinyint(1)")]
    public bool IsPrimary { get; set; } = false;

    [Column(TypeName = "tinyint(1)")]
    public bool IsCopyable { get; set; } = true;

    [Column(TypeName = "int")]
    public int Version { get; set; } = 1;

    [Column(TypeName = "bigint")]
    public long TotalPlays { get; set; } = 0;

    [Column(TypeName = "varchar(50)")]
    public string? PlayingMode { get; set; }

    [Column(TypeName = "int")]
    public int Score { get; set; } = 0;

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [JsonIgnore]
    [ForeignKey("GameId")]
    public virtual Game? Game { get; set; }

    [JsonIgnore]
    public virtual ICollection<GameServer> GameServers { get; set; } = new List<GameServer>();
}