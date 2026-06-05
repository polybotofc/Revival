using Microsoft.EntityFrameworkCore;
using Revival.Data;
using Revival.Models;

namespace Revival.Services;

/// <summary>
/// Service for managing and delivering assets.
/// Handles asset metadata and file delivery.
/// </summary>
public class AssetService
{
    private readonly RevivalDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AssetService> _logger;
    private readonly string _assetsPath;

    public AssetService(
        RevivalDbContext context, 
        IWebHostEnvironment environment,
        ILogger<AssetService> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
        _assetsPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "assets");
        
        // Ensure assets directory exists
        if (!Directory.Exists(_assetsPath))
        {
            Directory.CreateDirectory(_assetsPath);
        }
    }

    /// <summary>
    /// Gets an asset by ID.
    /// </summary>
    public async Task<Asset?> GetAssetByIdAsync(int assetId)
    {
        return await _context.Assets.FindAsync(assetId);
    }

    /// <summary>
    /// Gets the file path for an asset.
    /// </summary>
    public string? GetAssetFilePath(int assetId)
    {
        var asset = _context.Assets.Find(assetId);
        if (asset == null || !File.Exists(asset.FilePath))
        {
            // Try default location
            var defaultPath = Path.Combine(_assetsPath, assetId.ToString());
            if (File.Exists(defaultPath))
            {
                return defaultPath;
            }
            return null;
        }
        return asset.FilePath;
    }

    /// <summary>
    /// Gets a user avatar by user ID.
    /// </summary>
    public async Task<(string? FilePath, string? ContentType)> GetUserAvatarAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.AvatarAssetId.HasValue)
        {
            // Return default avatar
            var defaultAvatar = Path.Combine(_assetsPath, "..", "images", "default-avatar.png");
            if (File.Exists(defaultAvatar))
            {
                return (defaultAvatar, "image/png");
            }
            return (null, null);
        }

        var asset = await GetAssetByIdAsync(user.AvatarAssetId.Value);
        if (asset == null)
        {
            return (null, null);
        }

        var filePath = File.Exists(asset.FilePath) ? asset.FilePath : 
            Path.Combine(_assetsPath, asset.Id.ToString());

        return (File.Exists(filePath) ? filePath : null, asset.ContentType);
    }

    /// <summary>
    /// Gets a game thumbnail.
    /// </summary>
    public async Task<(string? FilePath, string? ContentType)> GetGameThumbnailAsync(int gameId)
    {
        var game = await _context.Games.FindAsync(gameId);
        if (game == null || string.IsNullOrEmpty(game.ThumbnailUrl))
        {
            // Return default thumbnail
            var defaultThumb = Path.Combine(_assetsPath, "..", "images", "default-game.png");
            if (File.Exists(defaultThumb))
            {
                return (defaultThumb, "image/png");
            }
            return (null, null);
        }

        // Check if thumbnail is a file path or URL
        if (File.Exists(game.ThumbnailUrl))
        {
            return (game.ThumbnailUrl, "image/png");
        }

        var thumbPath = Path.Combine(_assetsPath, "..", "thumbs", $"game_{gameId}.png");
        if (File.Exists(thumbPath))
        {
            return (thumbPath, "image/png");
        }

        return (null, null);
    }

    /// <summary>
    /// Creates a new asset.
    /// </summary>
    public async Task<Asset?> CreateAssetAsync(
        string name, 
        AssetType type, 
        string filePath, 
        int? creatorId,
        string? description = null)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogError("Asset file not found: {FilePath}", filePath);
            return null;
        }

        var fileInfo = new FileInfo(filePath);
        var contentType = GetContentType(fileInfo.Extension);

        var asset = new Asset
        {
            Name = name,
            Type = type,
            FilePath = filePath,
            CreatorId = creatorId,
            Description = description,
            FileSize = fileInfo.Length,
            ContentType = contentType,
            Hash = ComputeFileHash(filePath),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Asset created: {AssetName} (ID: {AssetId})", name, asset.Id);
        return asset;
    }

    /// <summary>
    /// Gets content type based on file extension.
    /// </summary>
    private string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".ogg" => "audio/ogg",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".rbxl" or ".rbxlx" => "application/octet-stream",
            ".obj" => "model/obj",
            ".mtl" => "model/mtl",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Computes MD5 hash of a file.
    /// </summary>
    private string ComputeFileHash(string filePath)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Gets all assets of a specific type.
    /// </summary>
    public async Task<List<Asset>> GetAssetsByTypeAsync(AssetType type, bool publicOnly = true)
    {
        var query = _context.Assets.Where(a => a.Type == type);
        
        if (publicOnly)
        {
            query = query.Where(a => a.IsPublic);
        }

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Deletes an asset.
    /// </summary>
    public async Task<bool> DeleteAssetAsync(int assetId)
    {
        var asset = await _context.Assets.FindAsync(assetId);
        if (asset == null) return false;

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Asset deleted: {AssetId}", assetId);
        return true;
    }
}