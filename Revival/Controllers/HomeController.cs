using Microsoft.AspNetCore.Mvc;
using Revival.Services;

namespace Revival.Controllers;

/// <summary>
/// Home controller for the main page and game listing.
/// </summary>
public class HomeController : Controller
{
    private readonly GameService _gameService;
    private readonly AuthService _authService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(GameService gameService, AuthService authService, ILogger<HomeController> logger)
    {
        _gameService = gameService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the main home page with game listings.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var games = await _gameService.GetPublicGamesAsync();
        return View(games);
    }

    /// <summary>
    /// Displays details for a specific game.
    /// </summary>
    public async Task<IActionResult> GameDetails(int id)
    {
        var game = await _gameService.GetGameByIdAsync(id);
        if (game == null)
        {
            return NotFound();
        }
        return View("GameDetails", game);
    }

    /// <summary>
    /// Displays the games directory page.
    /// </summary>
    public async Task<IActionResult> Games()
    {
        var games = await _gameService.GetPublicGamesAsync();
        return View(games);
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("/health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}