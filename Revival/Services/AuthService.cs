using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Revival.Data;
using Revival.Models;

namespace Revival.Services;

/// <summary>
/// Service for managing user authentication and session tickets.
/// Handles user registration, login, logout, and session management.
/// </summary>
public class AuthService
{
    private readonly RevivalDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private const int SessionExpirationHours = 24;

    public AuthService(RevivalDbContext context, ILogger<AuthService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    public async Task<(bool Success, string? Error, User? User)> RegisterAsync(string username, string email, string password)
    {
        try
        {
            // Check if username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                return (false, "Username already exists", null);
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return (false, "Email already exists", null);
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                DisplayName = username,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} registered successfully", username);
            return (true, null, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user {Username}", username);
            return (false, "An error occurred during registration", null);
        }
    }

    /// <summary>
    /// Authenticates a user and creates a new session.
    /// </summary>
    public async Task<(bool Success, string? Error, Session? Session)> LoginAsync(string username, string password, string? ipAddress, string? userAgent)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Username} not found", username);
                return (false, "Invalid username or password", null);
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for user {Username}", username);
                return (false, "Invalid username or password", null);
            }

            if (user.IsBanned)
            {
                _logger.LogWarning("Login failed: User {Username} is banned", username);
                return (false, "This account has been banned", null);
            }

            // Create session
            var session = await CreateSessionAsync(user.Id, ipAddress, userAgent);

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} logged in successfully", username);
            return (true, null, session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", username);
            return (false, "An error occurred during login", null);
        }
    }

    /// <summary>
    /// Creates an authentication session for a user.
    /// </summary>
    public async Task<Session> CreateSessionAsync(int userId, string? ipAddress, string? userAgent)
    {
        var sessionToken = GenerateSessionToken();
        var session = new Session
        {
            SessionToken = sessionToken,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(SessionExpirationHours),
            IsActive = true
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Session created for user {UserId}: {Token}", userId, sessionToken.Substring(0, 8) + "...");
        return session;
    }

    /// <summary>
    /// Validates a session token and returns the associated user.
    /// </summary>
    public async Task<(bool Valid, Session? Session, User? User)> ValidateSessionAsync(string sessionToken)
    {
        try
        {
            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

            if (session == null)
            {
                return (false, null, null);
            }

            if (!session.IsActive || session.ExpiresAt < DateTime.UtcNow)
            {
                // Session expired, deactivate it
                session.IsActive = false;
                await _context.SaveChangesAsync();
                return (false, null, null);
            }

            if (session.User?.IsBanned == true)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
                return (false, null, null);
            }

            return (true, session, session.User);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating session");
            return (false, null, null);
        }
    }

    /// <summary>
    /// Logs out a user by deactivating their session.
    /// </summary>
    public async Task<bool> LogoutAsync(string sessionToken)
    {
        try
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

            if (session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation("User logged out successfully");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return false;
        }
    }

    /// <summary>
    /// Generates a secure session token.
    /// </summary>
    private string GenerateSessionToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    /// <summary>
    /// Hashes a password using SHA256 with salt.
    /// </summary>
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salt = Guid.NewGuid().ToString();
        var saltedPassword = salt + password;
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return salt + ":" + Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Verifies a password against a stored hash.
    /// </summary>
    public bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            var salt = parts[0];
            var hash = parts[1];
            var saltedPassword = salt + password;

            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            var computedHash = Convert.ToBase64String(bytes);

            return hash == computedHash;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets user by ID.
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    /// <summary>
    /// Updates user profile information.
    /// </summary>
    public async Task<bool> UpdateProfileAsync(int userId, string? displayName, string? description, int? avatarAssetId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (displayName != null) user.DisplayName = displayName;
            if (description != null) user.Description = description;
            if (avatarAssetId.HasValue) user.AvatarAssetId = avatarAssetId;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
            return false;
        }
    }
}