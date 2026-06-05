using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Revival.Models;

/// <summary>
/// Represents an asset (image, model, audio, etc.) in the Roblox Revival platform.
/// Assets are delivered via the asset delivery system.
/// </summary>
public class Asset
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public AssetType Type { get; set; }

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public long FileSize { get; set; }

    [StringLength(100)]
    public string? ContentType { get; set; }

    [StringLength(255)]
    public string? Hash { get; set; }

    public int? CreatorId { get; set; }

    public bool IsPublic { get; set; } = true;

    public bool IsApproved { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("CreatorId")]
    public virtual User? Creator { get; set; }
}

/// <summary>
/// Enum representing different types of assets.
/// </summary>
public enum AssetType
{
    Image = 1,
    Model = 2,
    Audio = 3,
    Video = 4,
    Mesh = 5,
    Decal = 6,
    Avatar = 7,
    Place = 8,
    Package = 9,
    Plugin = 10,
    Script = 11,
    Other = 99
}