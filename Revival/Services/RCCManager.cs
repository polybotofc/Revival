using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Revival.Models;

namespace Revival.Services;

/// <summary>
/// Service for communicating with RCCService to manage game servers.
/// Handles creating, stopping game servers and executing Lua scripts.
/// </summary>
public class RCCManager
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RCCManager> _logger;
    private readonly string _rccServiceUrl;

    public RCCManager(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<RCCManager> logger)
    {
        _httpClient = httpClientFactory.CreateClient("RCCService");
        _rccServiceUrl = configuration["RCCService:Url"] ?? "http://localhost:3000";
        _logger = logger;
    }

    /// <summary>
    /// Creates a new game server for a specific place.
    /// </summary>
    public async Task<(bool Success, string? ServerId, string? JobId, string? Error)> CreateGameServerAsync(
        int placeId, 
        string placeFilePath, 
        int maxPlayers,
        string? customMachineAddress = null)
    {
        try
        {
            var request = new CreateServerRequest
            {
                PlaceId = placeId,
                PlacePath = placeFilePath,
                MaxPlayers = maxPlayers,
                MachineAddress = customMachineAddress ?? _rccServiceUrl
            };

            var response = await _httpClient.PostAsJsonAsync($"{_rccServiceUrl}/api/server/create", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CreateServerResponse>();
                if (result != null && result.Success)
                {
                    _logger.LogInformation("Game server created: {ServerId}, Job: {JobId}", result.ServerId, result.JobId);
                    return (true, result.ServerId, result.JobId, null);
                }
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create game server: {Error}", error);
            return (false, null, null, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game server for place {PlaceId}", placeId);
            return (false, null, null, ex.Message);
        }
    }

    /// <summary>
    /// Stops a running game server.
    /// </summary>
    public async Task<(bool Success, string? Error)> StopGameServerAsync(string jobId)
    {
        try
        {
            var request = new StopServerRequest { JobId = jobId };
            var response = await _httpClient.PostAsJsonAsync($"{_rccServiceUrl}/api/server/stop", request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Game server stopped: {JobId}", jobId);
                return (true, null);
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to stop game server {JobId}: {Error}", jobId, error);
            return (false, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping game server {JobId}", jobId);
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Gets the status of a game server.
    /// </summary>
    public async Task<(bool Success, ServerStatusResponse? Status, string? Error)> GetServerStatusAsync(string serverId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_rccServiceUrl}/api/server/status/{serverId}");

            if (response.IsSuccessStatusCode)
            {
                var status = await response.Content.ReadFromJsonAsync<ServerStatusResponse>();
                return (true, status, null);
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, null, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting server status for {ServerId}", serverId);
            return (false, null, ex.Message);
        }
    }

    /// <summary>
    /// Executes a Lua script on a specific game server.
    /// </summary>
    public async Task<(bool Success, string? Result, string? Error)> ExecuteScriptAsync(string serverId, string luaScript)
    {
        try
        {
            var request = new ExecuteScriptRequest
            {
                ServerId = serverId,
                Script = luaScript
            };

            var response = await _httpClient.PostAsJsonAsync($"{_rccServiceUrl}/api/script/execute", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ExecuteScriptResponse>();
                if (result != null)
                {
                    return (result.Success, result.Result, null);
                }
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to execute script on server {ServerId}: {Error}", serverId, error);
            return (false, null, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing script on server {ServerId}", serverId);
            return (false, null, ex.Message);
        }
    }

    /// <summary>
    /// Gets server connection information for a player.
    /// </summary>
    public async Task<(bool Success, ServerConnectionInfo? Info, string? Error)> GetConnectionInfoAsync(string serverId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_rccServiceUrl}/api/server/connect/{serverId}");

            if (response.IsSuccessStatusCode)
            {
                var info = await response.Content.ReadFromJsonAsync<ServerConnectionInfo>();
                return (true, info, null);
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, null, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection info for server {ServerId}", serverId);
            return (false, null, ex.Message);
        }
    }
}

// Request/Response DTOs

public class CreateServerRequest
{
    public int PlaceId { get; set; }
    public string PlacePath { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
    public string MachineAddress { get; set; } = string.Empty;
}

public class CreateServerResponse
{
    public bool Success { get; set; }
    public string? ServerId { get; set; }
    public string? JobId { get; set; }
    public string? Address { get; set; }
    public int Port { get; set; }
}

public class StopServerRequest
{
    public string JobId { get; set; } = string.Empty;
}

public class ServerStatusResponse
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CurrentPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public string? Address { get; set; }
    public int Port { get; set; }
}

public class ExecuteScriptRequest
{
    public string ServerId { get; set; } = string.Empty;
    public string Script { get; set; } = string.Empty;
}

public class ExecuteScriptResponse
{
    public bool Success { get; set; }
    public string? Result { get; set; }
}

public class ServerConnectionInfo
{
    public string ServerId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Port { get; set; }
    public string? Ticket { get; set; }
}