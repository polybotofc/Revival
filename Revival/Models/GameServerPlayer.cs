using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Revival.Models;

/// <summary>
/// Represents a player currently connected to a game server.
/// Used for tracking player sessions and server population.
/// </summary>
[Table("GameServerPlayers")]
public class GameServerPlayer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "int")]
    public int ServerId { get; set; }

    [Required]
    [Column(TypeName = "int")]
    public int UserId { get; set; }

    [Column(TypeName = "varchar(50)")]
    public string? DisplayName { get; set; }

    [Column(TypeName = "varchar(50)")]
    public string? MembershipType { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime")]
    public DateTime? LeftAt { get; set; }

    [Column(TypeName = "int")]
    public int Ping { get; set; } = 0;

    [Column(TypeName = "varchar(50)")]
    public string? Status { get; set; } = "Playing";

    [Column(TypeName = "int")]
    public int Score { get; set; } = 0;

    [Column(TypeName = "bigint")]
    public long PlayTimeSeconds { get; set; } = 0;

    // Navigation properties
    [JsonIgnore]
    [ForeignKey("ServerId")]
    public virtual GameServer? Server { get; set; }

    [JsonIgnore]
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}