using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Revival.Models;

/// <summary>
/// Represents a game/experience in the Roblox Revival platform.
/// A game can contain multiple places (levels).
/// </summary>
[Table("Games")]
public class Game
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    [Column(TypeName = "varchar(100)")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    [Column(TypeName = "varchar(2000)")]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "int")]
    public int CreatorId { get; set; }

    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string? ThumbnailUrl { get; set; }

    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string? IconUrl { get; set; }

    [Column(TypeName = "bigint")]
    public long TotalPlays { get; set; } = 0;

    [Column(TypeName = "bigint")]
    public long MonthlyPlays { get; set; } = 0;

    [Column(TypeName = "bigint")]
    public long WeeklyPlays { get; set; } = 0;

    [Column(TypeName = "int")]
    public int ActivePlayers { get; set; } = 0;

    [Column(TypeName = "int")]
    public int MaxPlayers { get; set; } = 50;

    [Column(TypeName = "tinyint(1)")]
    public bool IsPublic { get; set; } = true;

    [Column(TypeName = "tinyint(1)")]
    public bool IsActive { get; set; } = true;

    [Column(TypeName = "tinyint(1)")]
    public bool IsFeatured { get; set; } = false;

    [Column(TypeName = "int")]
    public int Votes { get; set; } = 0;

    [Column(TypeName = "int")]
    public int Likes { get; set; } = 0;

    [Column(TypeName = "int")]
    public int Dislikes { get; set; } = 0;

    [Column(TypeName = "decimal(3,2)")]
    public decimal Rating { get; set; } = 0.0m;

    [Column(TypeName = "varchar(50)")]
    public string? Genre { get; set; }

    [Column(TypeName = "varchar(50)")]
    public string? PlayingMode { get; set; }

    [Column(TypeName = "int")]
    public int MinPlayers { get; set; } = 1;

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime")]
    public DateTime? PublishedAt { get; set; }

    [Column(TypeName = "int")]
    public int Version { get; set; } = 1;

    [Column(TypeName = "bigint")]
    public long VisitCount { get; set; } = 0;

    // Navigation properties
    [JsonIgnore]
    [ForeignKey("CreatorId")]
    public virtual User? Creator { get; set; }

    [JsonIgnore]
    public virtual ICollection<Place> Places { get; set; } = new List<Place>();
    
    [JsonIgnore]
    public virtual ICollection<GameServer> GameServers { get; set; } = new List<GameServer>();
}