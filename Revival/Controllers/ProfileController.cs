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
    private const string SessionCookieName = "RevivalAuth";

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
        var user = await _authService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var games = await _gameService.GetGamesByCreatorAsync(user.Id);
        
        var viewModel = new ProfileViewModel
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName ?? user.Username,
            Description = user.Description,
            Location = user.Location,
            Website = user.Website,
            AvatarAssetId = user.AvatarAssetId,
            CreatedAt = user.CreatedAt,
            TotalGames = games.Count,
            TotalPlays = games.Sum(g => g.TotalPlays),
            Reputation = user.Reputation,
            FollowerCount = user.FollowerCount,
            FollowingCount = user.FollowingCount
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

        var games = await _gameService.GetGamesByCreatorAsync(user.Id);
        
        var viewModel = new ProfileViewModel
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName ?? user.Username,
            Description = user.Description,
            Location = user.Location,
            Website = user.Website,
            AvatarAssetId = user.AvatarAssetId,
            CreatedAt = user.CreatedAt,
            TotalGames = games.Count,
            TotalPlays = games.Sum(g => g.TotalPlays),
            Reputation = user.Reputation,
            FollowerCount = user.FollowerCount,
            FollowingCount = user.FollowingCount,
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

        return View(new ProfileEditViewModel
        {
            DisplayName = user.DisplayName,
            Description = user.Description,
            Location = user.Location,
            Website = user.Website,
            AvatarAssetId = user.AvatarAssetId
        });
    }

    /// <summary>
    /// POST /Profile/Edit - Processes profile updates.
    /// </summary>
    [HttpPost("/Profile/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileEditViewModel model)
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
            model.Location,
            model.Website,
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

        var games = await _gameService.GetGamesByCreatorAsync(user.Id);
        return View(games);
    }

    /// <summary>
    /// Gets the current authenticated user from session.
    /// </summary>
    private async Task<(bool IsAuthenticated, UserSession? Session, User? User)> GetCurrentUserAsync()
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
    public string? Location { get; set; }
    public string? Website { get; set; }
    public int? AvatarAssetId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalGames { get; set; }
    public long TotalPlays { get; set; }
    public int Reputation { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public bool IsOwnProfile { get; set; }
}

/// <summary>
/// View model for editing user profiles.
/// </summary>
public class ProfileEditViewModel
{
    [StringLength(100)]
    public string? DisplayName { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    [StringLength(100)]
    public string? Website { get; set; }

    public int? AvatarAssetId { get; set; }
}