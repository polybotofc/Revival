using Microsoft.EntityFrameworkCore;
using Revival.Data;
using Revival.Models;

namespace Revival.Services;

/// <summary>
/// Service for managing games, places, and game servers.
/// Coordinates between the website and RCCService for server management.
/// </summary>
public class GameService
{
    private readonly RevivalDbContext _context;
    private readonly RCCManager _rccManager;
    private readonly AuthService _authService;
    private readonly ILogger<GameService> _logger;

    public GameService(
        RevivalDbContext context, 
        RCCManager rccManager,
        AuthService authService,
        ILogger<GameService> logger)
    {
        _context = context;
        _rccManager = rccManager;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all public games.
    /// </summary>
    public async Task<List<Game>> GetPublicGamesAsync()
    {
        return await _context.Games
            .Where(g => g.IsPublic && g.IsActive)
            .Include(g => g.Creator)
            .OrderByDescending(g => g.TotalPlays)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a game by ID.
    /// </summary>
    public async Task<Game?> GetGameByIdAsync(int gameId)
    {
        return await _context.Games
            .Include(g => g.Creator)
            .Include(g => g.Places)
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    /// <summary>
    /// Gets a place by ID.
    /// </summary>
    public async Task<Place?> GetPlaceByIdAsync(int placeId)
    {
        return await _context.Places
            .Include(p => p.Game)
            .FirstOrDefaultAsync(p => p.Id == placeId);
    }

    /// <summary>
    /// Creates a new game.
    /// </summary>
    public async Task<Game?> CreateGameAsync(int creatorId, string name, string? description)
    {
        var game = new Game
        {
            Name = name,
            Description = description,
            CreatorId = creatorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Game created: {GameName} by user {UserId}", name, creatorId);
        return game;
    }

    /// <summary>
    /// Creates a new place within a game.
    /// </summary>
    public async Task<Place?> CreatePlaceAsync(int gameId, string name, string filePath, int maxPlayers = 50)
    {
        var place = new Place
        {
            GameId = gameId,
            Name = name,
            FilePath = filePath,
            MaxPlayers = maxPlayers,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Places.Add(place);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Place created: {PlaceName} in game {GameId}", name, gameId);
        return place;
    }

    /// <summary>
    /// Joins a player to a game. Creates a new game server if needed.
    /// </summary>
    public async Task<(bool Success, string? Error, JoinGameResult? Result)> JoinGameAsync(int placeId, int userId)
    {
        try
        {
            var place = await GetPlaceByIdAsync(placeId);
            if (place == null || !place.IsActive)
            {
                return (false, "Place not found or inactive", null);
            }

            var game = place.Game;
            if (game == null || !game.IsActive)
            {
                return (false, "Game not found or inactive", null);
            }

            // Find an available server or create a new one
            var server = await FindAvailableServerAsync(placeId);
            
            if (server == null)
            {
                // Create new server
                server = await CreateGameServerAsync(placeId, game.CreatorId);
                if (server == null)
                {
                    return (false, "Failed to create game server", null);
                }
            }

            // Generate authentication ticket
            var ticket = GenerateAuthTicket(userId, server.ServerId);

            var result = new JoinGameResult
            {
                ServerId = server.ServerId,
                Address = server.Host,
                Port = server.Port,
                Ticket = ticket,
                PlaceId = placeId,
                GameId = game.Id
            };

            _logger.LogInformation("User {UserId} joining game: Place={PlaceId}, Server={ServerId}", 
                userId, placeId, server.ServerId);

            return (true, null, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining game for user {UserId}, place {PlaceId}", userId, placeId);
            return (false, "An error occurred while joining the game", null);
        }
    }

    /// <summary>
    /// Creates a new game server for a place.
    /// </summary>
    public async Task<GameServer?> CreateGameServerAsync(int placeId, int creatorId)
    {
        var place = await GetPlaceByIdAsync(placeId);
        if (place == null) return null;

        // Call RCCService to create server
        var (success, serverId, jobId, error) = await _rccManager.CreateGameServerAsync(
            placeId, 
            place.FilePath, 
            place.MaxPlayers);

        if (!success || serverId == null)
        {
            _logger.LogError("Failed to create server: {Error}", error);
            return null;
        }

        // Get connection info from RCCService
        var (connSuccess, connInfo, connError) = await _rccManager.GetConnectionInfoAsync(serverId);
        
        var gameServer = new GameServer
        {
            ServerId = serverId,
            PlaceId = placeId,
            GameId = place.GameId,
            CreatorId = creatorId,
            Host = connInfo?.Address ?? "localhost",
            Port = connInfo?.Port ?? 53640,
            RCCJobId = jobId,
            RCCServerUrl = connInfo?.Address,
            Status = "Running",
            MaxPlayers = place.MaxPlayers,
            ExpiresAt = DateTime.UtcNow.AddHours(2)
        };

        _context.GameServers.Add(gameServer);
        await _context.SaveChangesAsync();

        // Update active players count
        var game = await _context.Games.FindAsync(place.GameId);
        if (game != null)
        {
            game.ActivePlayers++;
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Game server created: {ServerId} for place {PlaceId}", serverId, placeId);
        return gameServer;
    }

    /// <summary>
    /// Finds an available game server for a place.
    /// </summary>
    private async Task<GameServer?> FindAvailableServerAsync(int placeId)
    {
        return await _context.GameServers
            .Where(s => s.PlaceId == placeId && 
                        s.Status == "Running" && 
                        s.CurrentPlayers < s.MaxPlayers &&
                        s.ExpiresAt > DateTime.UtcNow)
            .OrderBy(s => s.CurrentPlayers)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Stops a game server.
    /// </summary>
    public async Task<bool> StopGameServerAsync(string serverId)
    {
        var server = await _context.GameServers.FirstOrDefaultAsync(s => s.ServerId == serverId);
        if (server == null) return false;

        var (success, error) = await _rccManager.StopGameServerAsync(server.RCCJobId ?? serverId);
        
        if (success)
        {
            server.Status = "Stopped";
            server.ExpiresAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var game = await _context.Games.FindAsync(server.GameId);
            if (game != null && game.ActivePlayers > 0)
            {
                game.ActivePlayers--;
            }
            await _context.SaveChangesAsync();

            _logger.LogInformation("Game server stopped: {ServerId}", serverId);
        }

        return success;
    }

    /// <summary>
    /// Updates the player count on a server.
    /// </summary>
    public async Task UpdatePlayerCountAsync(string serverId, int playerCount)
    {
        var server = await _context.GameServers.FirstOrDefaultAsync(s => s.ServerId == serverId);
        if (server == null) return;

        server.CurrentPlayers = playerCount;
        server.UpdatedAt = DateTime.UtcNow;
        
        var game = await _context.Games.FindAsync(server.GameId);
        if (game != null)
        {
            // Recalculate total active players
            var totalPlayers = await _context.GameServers
                .Where(s => s.GameId == game.Id && s.Status == "Running")
                .SumAsync(s => s.CurrentPlayers);
            game.ActivePlayers = totalPlayers;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Generates an authentication ticket for a player.
    /// </summary>
    private string GenerateAuthTicket(int userId, string serverId)
    {
        var ticketData = $"{userId}:{serverId}:{DateTime.UtcNow.Ticks}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(ticketData);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Validates an authentication ticket.
    /// </summary>
    public async Task<(bool Valid, int? UserId, string? ServerId)> ValidateAuthTicket(string ticket)
    {
        try
        {
            var bytes = Convert.FromBase64String(ticket);
            var ticketData = System.Text.Encoding.UTF8.GetString(bytes);
            var parts = ticketData.Split(':');

            if (parts.Length >= 2 && int.TryParse(parts[0], out int userId))
            {
                return (true, userId, parts[1]);
            }

            return (false, null, null);
        }
        catch
        {
            return (false, null, null);
        }
    }

    /// <summary>
    /// Gets server information for PlaceLauncher.
    /// </summary>
    public async Task<PlaceLauncherResult?> GetPlaceLauncherInfoAsync(int placeId, int userId)
    {
        var place = await GetPlaceByIdAsync(placeId);
        if (place == null || !place.IsActive) return null;

        var game = place.Game;
        if (game == null || !game.IsActive) return null;

        // Create or find server
        var (success, error, joinResult) = await JoinGameAsync(placeId, userId);
        
        if (!success || joinResult == null)
        {
            _logger.LogError("Failed to get place launcher info: {Error}", error);
            return null;
        }

        return new PlaceLauncherResult
        {
            GameId = game.Id,
            PlaceId = placeId,
            ServerId = joinResult.ServerId,
            Address = joinResult.Address,
            Port = joinResult.Port,
            Ticket = joinResult.Ticket,
            MachineAddress = joinResult.Address,
            MachinePort = joinResult.Port,
            PlaceVersionId = placeId
        };
    }
}

/// <summary>
/// Result object for joining a game.
/// </summary>
public class JoinGameResult
{
    public string ServerId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Ticket { get; set; } = string.Empty;
    public int PlaceId { get; set; }
    public int GameId { get; set; }
}

/// <summary>
/// Result object for PlaceLauncher endpoint.
/// </summary>
public class PlaceLauncherResult
{
    public int GameId { get; set; }
    public int PlaceId { get; set; }
    public string ServerId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Ticket { get; set; } = string.Empty;
    public string MachineAddress { get; set; } = string.Empty;
    public int MachinePort { get; set; }
    public int PlaceVersionId { get; set; }
}