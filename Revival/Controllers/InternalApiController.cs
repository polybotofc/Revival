using Microsoft.AspNetCore.Mvc;
using Revival.Models;
using Revival.Services;

namespace Revival.Controllers;

/// <summary>
/// Internal API controller for website to RCCService communication.
/// This API handles game server management requests from the website.
/// </summary>
[ApiController]
[Route("api/internal")]
public class InternalApiController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly RCCManager _rccManager;
    private readonly AuthService _authService;
    private readonly ILogger<InternalApiController> _logger;

    public InternalApiController(
        GameService gameService,
        RCCManager rccManager,
        AuthService authService,
        ILogger<InternalApiController> logger)
    {
        _gameService = gameService;
        _rccManager = rccManager;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new game server for a place.
    /// </summary>
    [HttpPost("server/create")]
    public async Task<IActionResult> CreateServer([FromBody] CreateServerRequest request)
    {
        try
        {
            var (isAuthenticated, session, user) = await GetAuthenticatedUserAsync();
            if (!isAuthenticated || user == null)
            {
                return Unauthorized(new { error = "Not authenticated" });
            }

            var place = await _gameService.GetPlaceByIdAsync(request.PlaceId);
            if (place == null)
            {
                return NotFound(new { error = "Place not found" });
            }

            var server = await _gameService.CreateGameServerAsync(request.PlaceId, user.Id);
            
            if (server == null)
            {
                return StatusCode(500, new { error = "Failed to create server" });
            }

            return Ok(new
            {
                success = true,
                serverId = server.ServerId,
                jobId = server.RCCJobId,
                address = server.Host,
                port = server.Port,
                placeId = place.Id,
                gameId = place.GameId,
                maxPlayers = server.MaxPlayers
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating server via internal API");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Stops a game server.
    /// </summary>
    [HttpPost("server/stop")]
    public async Task<IActionResult> StopServer([FromBody] StopServerRequest request)
    {
        try
        {
            var (isAuthenticated, session, user) = await GetAuthenticatedUserAsync();
            if (!isAuthenticated || user == null)
            {
                return Unauthorized(new { error = "Not authenticated" });
            }

            var success = await _gameService.StopGameServerAsync(request.ServerId);

            return Ok(new { success });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping server via internal API");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets server status.
    /// </summary>
    [HttpGet("server/status/{serverId}")]
    public async Task<IActionResult> GetServerStatus(string serverId)
    {
        try
        {
            var (success, status, error) = await _rccManager.GetServerStatusAsync(serverId);

            if (!success)
            {
                return BadRequest(new { error });
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting server status via internal API");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Joins a player to a game.
    /// </summary>
    [HttpPost("game/join")]
    public async Task<IActionResult> JoinGame([FromBody] JoinGameRequest request)
    {
        try
        {
            var (isAuthenticated, session, user) = await GetAuthenticatedUserAsync();
            if (!isAuthenticated || user == null)
            {
                return Unauthorized(new { error = "Not authenticated" });
            }

            var (success, error, result) = await _gameService.JoinGameAsync(request.PlaceId, user.Id);

            if (!success || result == null)
            {
                return BadRequest(new { error });
            }

            return Ok(new
            {
                success = true,
                serverId = result.ServerId,
                address = result.Address,
                port = result.Port,
                ticket = result.Ticket,
                placeId = result.PlaceId,
                gameId = result.GameId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining game via internal API");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Executes a Lua script on a game server.
    /// </summary>
    [HttpPost("script/execute")]
    public async Task<IActionResult> ExecuteScript([FromBody] ExecuteScriptRequest request)
    {
        try
        {
            var (isAuthenticated, session, user) = await GetAuthenticatedUserAsync();
            if (!isAuthenticated || user == null)
            {
                return Unauthorized(new { error = "Not authenticated" });
            }

            var (success, result, error) = await _rccManager.ExecuteScriptAsync(request.ServerId, request.Script);

            return Ok(new
            {
                success,
                result,
                error
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing script via internal API");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets the current authenticated user from session.
    /// </summary>
    private async Task<(bool IsAuthenticated, UserSession? Session, User? User)> GetAuthenticatedUserAsync()
    {
        var sessionToken = Request.Cookies["RevivalAuth"];
        
        if (string.IsNullOrEmpty(sessionToken))
        {
            return (false, null, null);
        }

        return await _authService.ValidateSessionAsync(sessionToken);
    }
}

// Request DTOs

public class CreateServerRequest
{
    public int PlaceId { get; set; }
}

public class StopServerRequest
{
    public string ServerId { get; set; } = string.Empty;
}

public class JoinGameRequest
{
    public int PlaceId { get; set; }
}

public class ExecuteScriptRequest
{
    public string ServerId { get; set; } = string.Empty;
    public string Script { get; set; } = string.Empty;
}