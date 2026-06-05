using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Revival.Models;

namespace Revival.Data;

/// <summary>
/// Provides initial seed data for the database.
/// Creates default admin user and sample game data.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Initializes the database with seed data if empty.
    /// </summary>
    public static async Task InitializeAsync(RevivalDbContext context, ILogger logger)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if we already have data
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Database already contains data. Skipping seed.");
            return;
        }

        logger.LogInformation("Seeding database with initial data...");

        try
        {
            // Create admin user
            var adminUser = new User
            {
                Username = "Admin",
                Email = "admin@revival.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                DisplayName = "Administrator",
                Description = "Roblox Revival Administrator",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailVerified = true,
                Reputation = 100
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            // Create sample user
            var sampleUser = new User
            {
                Username = "PlayerOne",
                Email = "player@revival.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Player123!"),
                DisplayName = "Player One",
                Description = "A sample player account for testing",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailVerified = true,
                Reputation = 10
            };
            context.Users.Add(sampleUser);
            await context.SaveChangesAsync();

            // Create sample game
            var sampleGame = new Game
            {
                Name = "Welcome to Roblox Revival",
                Description = "Welcome to the Roblox Revival platform! This is a sample game that demonstrates the game server functionality. Create your own games and invite friends to play!",
                CreatorId = adminUser.Id,
                IsPublic = true,
                IsActive = true,
                IsFeatured = true,
                TotalPlays = 0,
                MonthlyPlays = 0,
                ActivePlayers = 0,
                MaxPlayers = 50,
                Genre = "Adventure",
                PlayingMode = "OnlineMultiplayer",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow,
                Rating = 4.5m,
                Votes = 0,
                Likes = 0,
                Dislikes = 0
            };
            context.Games.Add(sampleGame);
            await context.SaveChangesAsync();

            // Create primary place for the sample game
            var primaryPlace = new Place
            {
                Name = "Main Game",
                GameId = sampleGame.Id,
                Description = "The main game world",
                FilePath = "places/welcome.rbxl",
                MaxPlayers = 50,
                MinPlayers = 1,
                IsActive = true,
                IsPrimary = true,
                IsCopyable = true,
                TotalPlays = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Places.Add(primaryPlace);
            await context.SaveChangesAsync();

            // Create another sample game
            var sandboxGame = new Game
            {
                Name = "Sandbox Building",
                Description = "A creative sandbox where you can build anything you imagine!",
                CreatorId = sampleUser.Id,
                IsPublic = true,
                IsActive = true,
                TotalPlays = 0,
                MonthlyPlays = 0,
                ActivePlayers = 0,
                MaxPlayers = 20,
                Genre = "Social",
                PlayingMode = "OnlineMultiplayer",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow,
                Rating = 4.0m,
                Votes = 0,
                Likes = 0,
                Dislikes = 0
            };
            context.Games.Add(sandboxGame);
            await context.SaveChangesAsync();

            // Create place for sandbox game
            var sandboxPlace = new Place
            {
                Name = "Sandbox World",
                GameId = sandboxGame.Id,
                Description = "Your creative building space",
                FilePath = "places/sandbox.rbxl",
                MaxPlayers = 20,
                MinPlayers = 1,
                IsActive = true,
                IsPrimary = true,
                IsCopyable = true,
                TotalPlays = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Places.Add(sandboxPlace);

            await context.SaveChangesAsync();

            logger.LogInformation(
                "Database seeded successfully. Admin user: Admin/Admin123!, Sample user: PlayerOne/Player123!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding database");
            throw;
        }
    }
}