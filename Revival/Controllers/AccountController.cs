using Microsoft.AspNetCore.Mvc;
using Revival.Models;
using Revival.Services;

namespace Revival.Controllers;

/// <summary>
/// Controller for user authentication and account management.
/// Handles register, login, logout, and profile operations.
/// </summary>
public class AccountController : Controller
{
    private readonly AuthService _authService;
    private readonly ILogger<AccountController> _logger;
    private const string SessionCookieName = "RevivalSession";

    public AccountController(AuthService authService, ILogger<AccountController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the login page.
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Processes user login.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, error, session) = await _authService.LoginAsync(
            model.Username, 
            model.Password,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            HttpContext.Request.Headers.UserAgent.ToString());

        if (!success || session == null)
        {
            ModelState.AddModelError(string.Empty, error ?? "Login failed");
            return View(model);
        }

        // Set session cookie
        Response.Cookies.Append(SessionCookieName, session.SessionToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = session.ExpiresAt
        });

        _logger.LogInformation("User {Username} logged in", model.Username);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Displays the registration page.
    /// </summary>
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Processes new user registration.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, error, user) = await _authService.RegisterAsync(
            model.Username, 
            model.Email, 
            model.Password);

        if (!success || user == null)
        {
            ModelState.AddModelError(string.Empty, error ?? "Registration failed");
            return View(model);
        }

        _logger.LogInformation("New user registered: {Username}", model.Username);

        // Auto-login after registration
        var (loginSuccess, loginError, session) = await _authService.LoginAsync(
            model.Username,
            model.Password,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            HttpContext.Request.Headers.UserAgent.ToString());

        if (loginSuccess && session != null)
        {
            Response.Cookies.Append(SessionCookieName, session.SessionToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = session.ExpiresAt
            });
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var sessionToken = Request.Cookies[SessionCookieName];
        
        if (!string.IsNullOrEmpty(sessionToken))
        {
            await _authService.LogoutAsync(sessionToken);
            Response.Cookies.Delete(SessionCookieName);
        }

        _logger.LogInformation("User logged out");
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Logs out user (GET version for convenience).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LogoutGet()
    {
        return await Logout();
    }

    /// <summary>
    /// Displays the user profile page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var (isAuthenticated, session, user) = await GetCurrentUserAsync();
        
        if (!isAuthenticated || user == null)
        {
            return RedirectToAction("Login");
        }

        return View(user);
    }

    /// <summary>
    /// Displays the profile edit page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var (isAuthenticated, session, user) = await GetCurrentUserAsync();
        
        if (!isAuthenticated || user == null)
        {
            return RedirectToAction("Login");
        }

        return View(new EditProfileViewModel
        {
            DisplayName = user.DisplayName,
            Description = user.Description
        });
    }

    /// <summary>
    /// Processes profile updates.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        var (isAuthenticated, session, user) = await GetCurrentUserAsync();
        
        if (!isAuthenticated || user == null)
        {
            return RedirectToAction("Login");
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
            return RedirectToAction("Profile");
        }

        ModelState.AddModelError(string.Empty, "Failed to update profile");
        return View(model);
    }

    /// <summary>
    /// API endpoint: Checks if user is logged in.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckAuth()
    {
        var (isAuthenticated, session, user) = await GetCurrentUserAsync();
        
        return Json(new 
        { 
            authenticated = isAuthenticated,
            userId = user?.Id,
            username = user?.Username
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
            return (false, null, null);
        }

        return await _authService.ValidateSessionAsync(sessionToken);
    }
}

// View Models

public class LoginViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class EditProfileViewModel
{
    [StringLength(255)]
    public string? DisplayName { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public int? AvatarAssetId { get; set; }
}