using Microsoft.AspNetCore.Mvc;
using Revival.Models;
using Revival.Services;

namespace Revival.Controllers;

/// <summary>
/// Controller for user profile management.
/// Provides profile viewing and editing functionality.
/// </summary>
public class ProfileController : Controller
{
    private readonly AuthService _authService;
    private readonly GameService _gameService;
    private readonly ILogger<ProfileController> _logger;
    private const string SessionCookieName = "RevivalSession";

    public ProfileController(
        AuthService authService, 
        GameService gameService,
        ILogger<ProfileController> logger)
    {
        _authService = authService;
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// GET /Profile/{username} - Displays a user's profile.
    /// </summary>
    [HttpGet("/Profile/{username}")]
    public async Task<IActionResult> ViewProfile(string username)
    {
        var user = await GetUserByUsernameAsync(username);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var viewModel = new ProfileViewModel
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName ?? user.Username,
            Description = user.Description,
            AvatarAssetId = user.AvatarAssetId,
            CreatedAt = user.CreatedAt,
            TotalGames = user.CreatedGames?.Count ?? 0
        };

        return View(viewModel);
    }

    /// <summary>
    /// GET /Profile - Displays the current user's profile.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var (isAuthenticated, session, user) = await GetCurrentUserAsync();
        
        if (!isAuthenticated || user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var viewModel = new ProfileViewModel
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName ?? user.Username,
            Description = user.Description,
            AvatarAssetId = user.AvatarAssetId,
            CreatedAt = user.CreatedAt,
            TotalGames = user.CreatedGames?.Count ?? 0,
            IsOwnProfile = true
        };

        return View("ViewProfile", viewModel);
    }

    /// <summary>
    /// GET /Profile/Edit - Displays the edit profile form.
    /// </summary>
    [HttpGet("/Profile/Edit")]
    public async Task<IActionResult> Edit()
    {
        var (isAuthenticated, session, user) = await GetCurrentUserAsync();
        
        if (!isAuthenticated || user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(new EditProfileViewModel
        {
            DisplayName = user.DisplayName,
            Description = user.Description,
            AvatarAssetId = user.AvatarAssetId
        });
    }

    /// <summary>
    /// POST /Profile/Edit - Processes profile updates.
    /// </summary>
    [HttpPost("/Profile/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        var (isAuthenticated, session, user) = await GetCurrentUserAsync();
        
        if (!isAuthenticated || user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var success = await _authService.UpdateProfileAsync(
            user.Id,
            model.DisplayName,
            model.Description,
            model.AvatarAssetId);

        if (success)
        {
            _logger.LogInformation("Profile updated for user {UserId}", user.Id);
            return RedirectToAction("ViewProfile", new { username = user.Username });
        }

        ModelState.AddModelError(string.Empty, "Failed to update profile");
        return View(model);
    }

    /// <summary>
    /// GET /Profile/Games - Displays games created by the user.
    /// </summary>
    [HttpGet("/Profile/Games")]
    public async Task<IActionResult> MyGames()
    {
        var (isAuthenticated, session, user) = await GetCurrentUserAsync();
        
        if (!isAuthenticated || user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var games = await _gameService.GetPublicGamesAsync();
        var userGames = games.Where(g => g.CreatorId == user.Id).ToList();

        return View(userGames);
    }

    /// <summary>
    /// Gets user by username.
    /// </summary>
    private async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _authService.GetUserByIdAsync(
            (await GetAllUsersAsync()).FirstOrDefault(u => u.Username == username)?.Id ?? 0);
    }

    private async Task<List<User>> GetAllUsersAsync()
    {
        // This would normally be a database query
        // For now, we'll get from auth service context
        return new List<User>();
    }

    /// <summary>
    /// Gets the current authenticated user from session.
    /// </summary>
    private async Task<(bool IsAuthenticated, Session? Session, User? User)> GetCurrentUserAsync()
    {
        var sessionToken = Request.Cookies[SessionCookieName];
        
        if (string.IsNullOrEmpty(sessionToken))
        {
            return (false, null, null);
        }

        return await _authService.ValidateSessionAsync(sessionToken);
    }
}

/// <summary>
/// View model for displaying user profiles.
/// </summary>
public class ProfileViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AvatarAssetId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalGames { get; set; }
    public bool IsOwnProfile { get; set; }
}

/// <summary>
/// View model for editing user profiles.
/// </summary>
public class EditProfileViewModel
{
    [StringLength(255)]
    public string? DisplayName { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public int? AvatarAssetId { get; set; }
}