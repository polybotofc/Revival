using Microsoft.AspNetCore.Mvc;
using Revival.Services;

namespace Revival.Controllers;

/// <summary>
/// Controller for asset delivery system.
/// Handles serving assets, thumbnails, and user avatars.
/// </summary>
public class AssetController : Controller
{
    private readonly AssetService _assetService;
    private readonly ILogger<AssetController> _logger;

    public AssetController(AssetService assetService, ILogger<AssetController> logger)
    {
        _assetService = assetService;
        _logger = logger;
    }

    /// <summary>
    /// GET /asset/?id={assetId} - Delivers an asset file.
    /// Returns the raw asset data with appropriate content type.
    /// </summary>
    [HttpGet("/asset/")]
    public async Task<IActionResult> GetAsset(int? id)
    {
        if (!id.HasValue)
        {
            return BadRequest("Asset ID is required");
        }

        var asset = await _assetService.GetAssetByIdAsync(id.Value);
        if (asset == null)
        {
            return NotFound("Asset not found");
        }

        var filePath = _assetService.GetAssetFilePath(id.Value);
        if (filePath == null || !System.IO.File.Exists(filePath))
        {
            _logger.LogWarning("Asset file not found: ID={AssetId}", id);
            return NotFound("Asset file not found");
        }

        var contentType = asset.ContentType ?? "application/octet-stream";
        var fileStream = System.IO.File.OpenRead(filePath);

        return File(fileStream, contentType, $"{asset.Name}{System.IO.Path.GetExtension(filePath)}");
    }

    /// <summary>
    /// GET /thumbs/avatar - Returns user avatar thumbnail.
    /// Query params: userId
    /// </summary>
    [HttpGet("/thumbs/avatar")]
    public async Task<IActionResult> GetAvatarThumbnail(int? userId)
    {
        if (!userId.HasValue)
        {
            return BadRequest("User ID is required");
        }

        var (filePath, contentType) = await _assetService.GetUserAvatarAsync(userId.Value);
        
        if (filePath == null)
        {
            // Return 1x1 transparent pixel as default
            return File(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01,
                0x00, 0x00, 0x00, 0x01, 0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
                0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63,
                0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00,
                0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 }, "image/png");
        }

        var fileStream = System.IO.File.OpenRead(filePath);
        return File(fileStream, contentType ?? "image/png");
    }

    /// <summary>
    /// GET /thumbs/game - Returns game thumbnail.
    /// Query params: gameId
    /// </summary>
    [HttpGet("/thumbs/game")]
    public async Task<IActionResult> GetGameThumbnail(int? gameId)
    {
        if (!gameId.HasValue)
        {
            return BadRequest("Game ID is required");
        }

        var (filePath, contentType) = await _assetService.GetGameThumbnailAsync(gameId.Value);
        
        if (filePath == null)
        {
            // Return default game thumbnail
            return File(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01,
                0x00, 0x00, 0x00, 0x01, 0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
                0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63,
                0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00,
                0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 }, "image/png");
        }

        var fileStream = System.IO.File.OpenRead(filePath);
        return File(fileStream, contentType ?? "image/png");
    }

    /// <summary>
    /// GET /thumbs/place - Returns place thumbnail.
    /// Query params: placeId
    /// </summary>
    [HttpGet("/thumbs/place")]
    public async Task<IActionResult> GetPlaceThumbnail(int? placeId)
    {
        if (!placeId.HasValue)
        {
            return BadRequest("Place ID is required");
        }

        // Place thumbnails use game thumbnails
        var (filePath, contentType) = await _assetService.GetGameThumbnailAsync(placeId.Value);
        
        if (filePath == null)
        {
            return File(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01,
                0x00, 0x00, 0x00, 0x01, 0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
                0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63,
                0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00,
                0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 }, "image/png");
        }

        var fileStream = System.IO.File.OpenRead(filePath);
        return File(fileStream, contentType ?? "image/png");
    }

    /// <summary>
    /// GET /api/assets/{id} - API endpoint for asset metadata.
    /// </summary>
    [HttpGet("/api/assets/{id}")]
    public async Task<IActionResult> GetAssetApi(int id)
    {
        var asset = await _assetService.GetAssetByIdAsync(id);
        if (asset == null)
        {
            return NotFound();
        }

        return Json(new
        {
            id = asset.Id,
            name = asset.Name,
            type = asset.Type.ToString(),
            description = asset.Description,
            fileSize = asset.FileSize,
            contentType = asset.ContentType,
            hash = asset.Hash,
            createdAt = asset.CreatedAt
        });
    }
}