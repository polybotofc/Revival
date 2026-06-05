using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Revival.Models;

/// <summary>
/// Represents a user in the Roblox Revival platform.
/// Contains user authentication and profile information.
/// </summary>
[Table("Users")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    [Column(TypeName = "varchar(50)")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 5)]
    [EmailAddress]
    [Column(TypeName = "varchar(255)")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? DisplayName { get; set; }

    [StringLength(1000)]
    [Column(TypeName = "varchar(1000)")]
    public string? Description { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? Location { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? Website { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime")]
    public DateTime? LastLoginAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Birthdate { get; set; }

    [Column(TypeName = "tinyint(1)")]
    public bool IsBanned { get; set; } = false;

    [StringLength(500)]
    [Column(TypeName = "varchar(500)")]
    public string? BanReason { get; set; }

    [Column(TypeName = "int")]
    public int? AvatarAssetId { get; set; }

    [Column(TypeName = "int")]
    public int LoginCount { get; set; } = 0;

    [Column(TypeName = "int")]
    public int Reputation { get; set; } = 0;

    [Column(TypeName = "int")]
    public int TotalFriends { get; set; } = 0;

    [Column(TypeName = "int")]
    public int FollowerCount { get; set; } = 0;

    [Column(TypeName = "int")]
    public int FollowingCount { get; set; } = 0;

    [Column(TypeName = "int")]
    public int PostCount { get; set; } = 0;

    [Column(TypeName = "tinyint(1)")]
    public bool EmailVerified { get; set; } = false;

    [Column(TypeName = "varchar(255)")]
    public string? EmailVerificationToken { get; set; }

    [Column(TypeName = "varchar(255)")]
    public string? PasswordResetToken { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PasswordResetExpiresAt { get; set; }

    [Column(TypeName = "int")]
    public int FailedLoginAttempts { get; set; } = 0;

    [Column(TypeName = "datetime")]
    public DateTime? LockoutEnd { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    
    [JsonIgnore]
    public virtual ICollection<Game> CreatedGames { get; set; } = new List<Game>();
    
    [JsonIgnore]
    public virtual ICollection<GameServer> GameServers { get; set; } = new List<GameServer>();
    
    [JsonIgnore]
    public virtual ICollection<Asset> CreatedAssets { get; set; } = new List<Asset>();
}