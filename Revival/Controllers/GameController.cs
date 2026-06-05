using Microsoft.AspNetCore.Mvc;
using Revival.Services;

namespace Revival.Controllers;

/// <summary>
/// Controller for game-related endpoints.
/// Handles game joining and place launching for Roblox clients.
/// </summary>
public class GameController : Controller
{
    private readonly GameService _gameService;
    private readonly AuthService _authService;
    private readonly ILogger<GameController> _logger;
    private const string SessionCookieName = "RevivalSession";

    public GameController(
        GameService gameService, 
        AuthService authService, 
        ILogger<GameController> logger)
    {
        _gameService = gameService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// GET /Game/Join.ashx - Joins a player to a game server.
    /// Returns server connection information including ticket.
    /// This endpoint is called by the Roblox client when user clicks Play.
    /// </summary>
    [HttpGet("/Game/Join.ashx")]
    public async Task<IActionResult> Join(int? placeId)
    {
        if (!placeId.HasValue)
        {
            return Content("ERROR: PlaceId is required", "text/plain");
        }

        var (isAuthenticated, session, user) = await GetCurrentUserAsync();
        
        if (!isAuthenticated || user == null)
        {
            return Content("ERROR: Not authenticated", "text/plain");
        }

        var (success, error, result) = await _gameService.JoinGameAsync(placeId.Value, user.Id);

        if (!success || result == null)
        {
            _logger.LogWarning("Join failed for user {UserId}, place {PlaceId}: {Error}", 
                user.Id, placeId, error);
            return Content($"ERROR: {error ?? "Failed to join game"}", "text/plain");
        }

        // Return server information in a format the Roblox client can parse
        // Format: success|serverId|address|port|ticket
        var response = $"OK|{result.ServerId}|{result.Address}|{result.Port}|{result.Ticket}";
        
        _logger.LogInformation("User {UserId} joined game: Server={ServerId}, Address={Address}:{Port}", 
            user.Id, result.ServerId, result.Address, result.Port);

        return Content(response, "text/plain");
    }

    /// <summary>
    /// GET /Game/PlaceLauncher.ashx - Launches a place and returns server info.
    /// Used by the Roblox client to get complete place launch information.
    /// </summary>
    [HttpGet("/Game/PlaceLauncher.ashx")]
    public async Task<IActionResult> PlaceLauncher(int? placeId)
    {
        if (!placeId.HasValue)
        {
            return Content("ERROR: PlaceId is required", "text/plain");
        }

        var (isAuthenticated, session, user) = await GetCurrentUserAsync();
        
        if (!isAuthenticated || user == null)
        {
            return Content("ERROR: Not authenticated", "text/plain");
        }

        var launcherInfo = await _gameService.GetPlaceLauncherInfoAsync(placeId.Value, user.Id);

        if (launcherInfo == null)
        {
            _logger.LogWarning("PlaceLauncher failed for user {UserId}, place {PlaceId}", 
                user.Id, placeId);
            return Content("ERROR: Failed to launch place", "text/plain");
        }

        // Return detailed server information
        // Format: machineAddress|machinePort|placeId|gameId|serverId|ticket|placeVersionId
        var response = string.Join("|",
            launcherInfo.MachineAddress,
            launcherInfo.MachinePort,
            launcherInfo.PlaceId,
            launcherInfo.GameId,
            launcherInfo.ServerId,
            launcherInfo.Ticket,
            launcherInfo.PlaceVersionId);

        _logger.LogInformation("PlaceLauncher: User {UserId} launching place {PlaceId}, Server={ServerId}",
            user.Id, placeId, launcherInfo.ServerId);

        return Content(response, "text/plain");
    }

    /// <summary>
    /// GET /Game/CheckServer - Checks server status.
    /// </summary>
    [HttpGet("/Game/CheckServer")]
    public async Task<IActionResult> CheckServer(string? serverId)
    {
        if (string.IsNullOrEmpty(serverId))
        {
            return Json(new { success = false, error = "ServerId required" });
        }

        var (isAuthenticated, session, user) = await GetCurrentUserAsync();
        
        if (!isAuthenticated || user == null)
        {
            return Json(new { success = false, error = "Not authenticated" });
        }

        var (valid, userId, parsedServerId) = await _gameService.ValidateAuthTicket(Request.Headers["RBXAuthTicket"].ToString());
        
        if (!valid || userId != user.Id)
        {
            return Json(new { success = false, error = "Invalid ticket" });
        }

        // Server is available for connection
        return Json(new { success = true, serverId = serverId });
    }

    /// <summary>
    /// POST /Game/PlayerJoined - Called when a player joins a server.
    /// </summary>
    [HttpPost("/Game/PlayerJoined")]
    public async Task<IActionResult> PlayerJoined([FromForm] string serverId, [FromForm] int playerCount)
    {
        await _gameService.UpdatePlayerCountAsync(serverId, playerCount);
        return Ok();
    }

    /// <summary>
    /// GET /Game/GetGames - Returns list of public games.
    /// </summary>
    [HttpGet("/Game/GetGames")]
    public async Task<IActionResult> GetGames()
    {
        var games = await _gameService.GetPublicGamesAsync();
        
        return Json(new 
        { 
            success = true,
            games = games.Select(g => new
            {
                id = g.Id,
                name = g.Name,
                description = g.Description,
                creator = g.Creator?.Username,
                totalPlays = g.TotalPlays,
                activePlayers = g.ActivePlayers
            })
        });
    }

    /// <summary>
    /// Gets the current authenticated user from session.
    /// </summary>
    private async Task<(bool IsAuthenticated, Session? Session, User? User)> GetCurrentUserAsync()
    {
        var sessionToken = Request.Cookies[SessionCookieName];
        
        if (string.IsNullOrEmpty(sessionToken))
        {
            // Try to get ticket from header (for Roblox client)
            var ticket = Request.Headers["RBXAuthTicket"].ToString();
            if (!string.IsNullOrEmpty(ticket))
            {
                var (valid, userId, serverId) = await _gameService.ValidateAuthTicket(ticket);
                if (valid && userId.HasValue)
                {
                    var user = await _authService.GetUserByIdAsync(userId.Value);
                    if (user != null)
                    {
                        return (true, null, user);
                    }
                }
            }
            return (false, null, null);
        }

        return await _authService.ValidateSessionAsync(sessionToken);
    }
}