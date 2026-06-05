using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Revival.Models;

/// <summary>
/// Represents an asset (image, model, audio, etc.) in the Roblox Revival platform.
/// Assets are delivered via the asset delivery system.
/// </summary>
[Table("Assets")]
public class Asset
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
    public AssetType Type { get; set; } = AssetType.Other;

    [Required]
    [StringLength(500)]
    [Column(TypeName = "varchar(500)")]
    public string FilePath { get; set; } = string.Empty;

    [StringLength(2000)]
    [Column(TypeName = "varchar(2000)")]
    public string? Description { get; set; }

    [Column(TypeName = "bigint")]
    public long FileSize { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? ContentType { get; set; }

    [StringLength(64)]
    [Column(TypeName = "varchar(64)")]
    public string? Hash { get; set; }

    [Column(TypeName = "int")]
    public int? CreatorId { get; set; }

    [Column(TypeName = "tinyint(1)")]
    public bool IsPublic { get; set; } = true;

    [Column(TypeName = "tinyint(1)")]
    public bool IsApproved { get; set; } = true;

    [Column(TypeName = "tinyint(1)")]
    public bool IsForSale { get; set; } = false;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; } = 0;

    [Column(TypeName = "int")]
    public int SalesCount { get; set; } = 0;

    [Column(TypeName = "int")]
    public int Version { get; set; } = 1;

    [Column(TypeName = "int")]
    public int DownloadCount { get; set; } = 0;

    [Column(TypeName = "varchar(50)")]
    public string? Category { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string? Tags { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [JsonIgnore]
    [ForeignKey("CreatorId")]
    public virtual User? Creator { get; set; }
}

/// <summary>
/// Enum representing different types of assets.
/// Compatible with Roblox asset types for version 0.338.0.202976
/// </summary>
public enum AssetType
{
    None = 0,
    Image = 1,
    TShirt = 2,  // T-Shirt in Roblox
    Audio = 3,
    Mesh = 4,
    Lua = 5,
    Text = 6,
    HTML = 7,
    Animation = 8,
    Model = 9,
    Blueprint = 10,
    Plugin = 11,
    Mashup = 12,
    Accessory = 17,
    App = 18,
    GamePass = 34,
    Product = 38,
    Badge = 41,
    Place = 38, // Same as Product in older versions
    GroupBadge = 44,
    PluginAccessory = 64,
    Decal = 65,
    Video = 68,
    AudioStreaming = 69,
    Texture = 70,
    VideoTexture = 71,
    Other = 99
}