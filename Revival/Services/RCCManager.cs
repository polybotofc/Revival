using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Revival.Models;

namespace Revival.Services;

/// <summary>
/// Service for communicating with RCCService to manage game servers.
/// Handles creating, stopping game servers and executing Lua scripts.
/// Uses HttpClientFactory for resilient HTTP communication.
/// </summary>
public class RCCManager
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RCCManager> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly bool _enabled;
    private readonly int _timeoutSeconds;

    public RCCManager(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration, 
        ILogger<RCCManager> logger)
    {
        _httpClient = httpClientFactory.CreateClient("RCCService");
        _configuration = configuration;
        _logger = logger;
        
        var rccSettings = _configuration.GetSection("RCCService");
        _enabled = rccSettings.GetValue<bool>("Enabled", true);
        _baseUrl = rccSettings.GetValue<string>("BaseUrl", "http://localhost:3000")!;
        _timeoutSeconds = rccSettings.GetValue<int>("TimeoutSeconds", 30);
        
        // Configure default request headers
        var apiKey = rccSettings.GetValue<string>("ApiKey");
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        }
    }

    #region Server Management

    /// <summary>
    /// Creates a new game server for a specific place.
    /// </summary>
    public async Task<(bool Success, string? ServerId, string? JobId, string? Address, int Port, string? Error)> 
        CreateGameServerAsync(
            int placeId, 
            string placeFilePath, 
            int maxPlayers,
            int creatorId,
            string? customMachineAddress = null)
    {
        if (!_enabled)
        {
            _logger.LogWarning("RCCService is disabled. Simulating server creation.");
            return SimulateServerCreation(placeId, maxPlayers);
        }

        try
        {
            var request = new RCCCreateServerRequest
            {
                PlaceId = placeId,
                PlacePath = placeFilePath,
                MaxPlayers = maxPlayers,
                CreatorId = creatorId,
                MachineAddress = customMachineAddress ?? _baseUrl
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/server/create", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RCCCreateServerResponse>();
                if (result != null && result.Success)
                {
                    _logger.LogInformation(
                        "Game server created: ServerId={ServerId}, JobId={JobId}, Address={Address}, Port={Port}", 
                        result.ServerId, result.JobId, result.Address, result.Port);
                    
                    return (true, result.ServerId, result.JobId, result.Address, result.Port, null);
                }
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create game server. Status: {Status}, Error: {Error}", 
                response.StatusCode, error);
            
            return (false, null, null, null, 0, error);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error creating game server for place {PlaceId}. RCCService may be unavailable.", placeId);
            return SimulateServerCreation(placeId, maxPlayers);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout creating game server for place {PlaceId}", placeId);
            return (false, null, null, null, 0, "Request timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game server for place {PlaceId}", placeId);
            return (false, null, null, null, 0, ex.Message);
        }
    }

    /// <summary>
    /// Simulates server creation when RCCService is unavailable.
    /// </summary>
    private (bool Success, string? ServerId, string? JobId, string? Address, int Port, string? Error) 
        SimulateServerCreation(int placeId, int maxPlayers)
    {
        var serverId = Guid.NewGuid().ToString();
        var port = 53640 + (placeId % 1000);
        
        _logger.LogInformation(
            "Simulated server creation: ServerId={ServerId}, Port={Port}, MaxPlayers={MaxPlayers}",
            serverId, port, maxPlayers);
        
        return (true, serverId, $"sim_{serverId}", "127.0.0.1", port, null);
    }

    /// <summary>
    /// Stops a running game server.
    /// </summary>
    public async Task<(bool Success, string? Error)> StopGameServerAsync(string serverId, string? jobId)
    {
        if (!_enabled)
        {
            _logger.LogInformation("RCCService is disabled. Simulating server stop for {ServerId}", serverId);
            return (true, null);
        }

        try
        {
            var request = new RCCStopServerRequest 
            { 
                ServerId = serverId,
                JobId = jobId ?? serverId
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/server/stop", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Game server stopped: {ServerId}", serverId);
                return (true, null);
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to stop game server {ServerId}: {Error}", serverId, error);
            return (false, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping game server {ServerId}", serverId);
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Gets the status of a game server.
    /// </summary>
    public async Task<(bool Success, RCCServerStatus? Status, string? Error)> GetServerStatusAsync(string serverId)
    {
        if (!_enabled)
        {
            return (true, new RCCServerStatus 
            { 
                ServerId = serverId,
                Status = "Running",
                CurrentPlayers = 0,
                MaxPlayers = 50,
                CpuUsage = 0,
                MemoryUsageMb = 0
            }, null);
        }

        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/server/status/{serverId}");

            if (response.IsSuccessStatusCode)
            {
                var status = await response.Content.ReadFromJsonAsync<RCCServerStatus>();
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

    #endregion

    #region Script Execution

    /// <summary>
    /// Executes a Lua script on a specific game server.
    /// </summary>
    public async Task<(bool Success, string? Result, string? Error)> ExecuteScriptAsync(
        string serverId, 
        string luaScript,
        bool isDatastoreScript = false)
    {
        if (!_enabled)
        {
            _logger.LogWarning("RCCService is disabled. Script execution simulated for server {ServerId}", serverId);
            return (true, "{\"status\": \"simulated\"}", null);
        }

        try
        {
            var request = new RCCExecuteScriptRequest
            {
                ServerId = serverId,
                Script = luaScript,
                Target = isDatastoreScript ? "datastore" : "script"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/script/execute", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RCCExecuteScriptResponse>();
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
    /// Sends a heartbeat to a game server to keep it alive.
    /// </summary>
    public async Task<bool> SendHeartbeatAsync(string serverId)
    {
        if (!_enabled) return true;

        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/server/heartbeat/{serverId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending heartbeat to server {ServerId}", serverId);
            return false;
        }
    }

    #endregion

    #region Connection Info

    /// <summary>
    /// Gets server connection information for a player.
    /// </summary>
    public async Task<(bool Success, RCCConnectionInfo? Info, string? Error)> GetConnectionInfoAsync(string serverId)
    {
        if (!_enabled)
        {
            return (true, new RCCConnectionInfo
            {
                ServerId = serverId,
                Address = "127.0.0.1",
                Port = 53640,
                Ticket = GenerateTicket(serverId)
            }, null);
        }

        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/server/connect/{serverId}");

            if (response.IsSuccessStatusCode)
            {
                var info = await response.Content.ReadFromJsonAsync<RCCConnectionInfo>();
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

    /// <summary>
    /// Generates an authentication ticket for a player.
    /// </summary>
    public string GenerateTicket(string serverId, int? userId = null)
    {
        var ticketData = new
        {
            serverId,
            userId,
            issued = DateTime.UtcNow.Ticks,
            expires = DateTime.UtcNow.AddHours(2).Ticks
        };
        
        var json = JsonSerializer.Serialize(ticketData);
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Validates an authentication ticket.
    /// </summary>
    public (bool Valid, string? ServerId, int? UserId) ValidateTicket(string ticket)
    {
        try
        {
            var bytes = Convert.FromBase64String(ticket);
            var json = Encoding.UTF8.GetString(bytes);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var expires = root.GetProperty("expires").GetInt64();
            if (DateTime.UtcNow.Ticks > expires)
                return (false, null, null);

            var serverId = root.GetProperty("serverId").GetString();
            int? userId = null;
            if (root.TryGetProperty("userId", out var userIdElement))
            {
                userId = userIdElement.GetInt32();
            }

            return (true, serverId, userId);
        }
        catch
        {
            return (false, null, null);
        }
    }

    #endregion

    #region Health Check

    /// <summary>
    /// Checks if RCCService is healthy.
    /// </summary>
    public async Task<(bool Healthy, string? Message)> HealthCheckAsync()
    {
        if (!_enabled)
        {
            return (true, "RCCService is disabled (simulation mode)");
        }

        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/health");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return (true, content);
            }
            
            return (false, $"Health check failed with status {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"Health check error: {ex.Message}");
        }
    }

    #endregion
}

#region RCCService Request/Response Models

/// <summary>
/// Request model for creating a game server via RCCService.
/// </summary>
public class RCCCreateServerRequest
{
    [JsonPropertyName("placeId")]
    public int PlaceId { get; set; }

    [JsonPropertyName("placePath")]
    public string PlacePath { get; set; } = string.Empty;

    [JsonPropertyName("maxPlayers")]
    public int MaxPlayers { get; set; }

    [JsonPropertyName("creatorId")]
    public int CreatorId { get; set; }

    [JsonPropertyName("machineAddress")]
    public string MachineAddress { get; set; } = string.Empty;
}

/// <summary>
/// Response model from RCCService server creation.
/// </summary>
public class RCCCreateServerResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("serverId")]
    public string? ServerId { get; set; }

    [JsonPropertyName("jobId")]
    public string? JobId { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }
}

/// <summary>
/// Request model for stopping a game server.
/// </summary>
public class RCCStopServerRequest
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; } = string.Empty;

    [JsonPropertyName("jobId")]
    public string JobId { get; set; } = string.Empty;
}

/// <summary>
/// Server status response from RCCService.
/// </summary>
public class RCCServerStatus
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("currentPlayers")]
    public int CurrentPlayers { get; set; }

    [JsonPropertyName("maxPlayers")]
    public int MaxPlayers { get; set; }

    [JsonPropertyName("cpuUsage")]
    public int CpuUsage { get; set; }

    [JsonPropertyName("memoryUsageMb")]
    public long MemoryUsageMb { get; set; }

    [JsonPropertyName("uptime")]
    public long Uptime { get; set; }
}

/// <summary>
/// Request model for executing Lua scripts.
/// </summary>
public class RCCExecuteScriptRequest
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; } = string.Empty;

    [JsonPropertyName("script")]
    public string Script { get; set; } = string.Empty;

    [JsonPropertyName("target")]
    public string Target { get; set; } = "script";
}

/// <summary>
/// Response model from script execution.
/// </summary>
public class RCCExecuteScriptResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
/// Connection info for joining a game server.
/// </summary>
public class RCCConnectionInfo
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("ticket")]
    public string? Ticket { get; set; }

    [JsonPropertyName("ping")]
    public int Ping { get; set; }
}

#endregion