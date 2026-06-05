using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Revival.Data;
using Revival.Models;

namespace Revival.Services;

/// <summary>
/// Service for managing user authentication and session tickets.
/// Handles user registration, login, logout, and session management.
/// Uses BCrypt for password hashing.
/// </summary>
public class AuthService
{
    private readonly RevivalDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(RevivalDbContext context, ILogger<AuthService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    #region User Registration & Management

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    public async Task<(bool Success, string? Error, User? User)> RegisterAsync(
        string username, 
        string email, 
        string password)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username is required", null);
            
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required", null);
            
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required", null);

            // Check password requirements
            var securitySettings = _configuration.GetSection("Security");
            var minLength = securitySettings.GetValue<int>("PasswordMinLength", 6);
            if (password.Length < minLength)
                return (false, $"Password must be at least {minLength} characters", null);

            // Check if username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
                return (false, "Username already exists", null);

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
                return (false, "Email already exists", null);

            // Create user
            var user = new User
            {
                Username = username.Trim(),
                Email = email.Trim().ToLower(),
                PasswordHash = HashPassword(password),
                DisplayName = username.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} registered successfully with ID {UserId}", username, user.Id);
            return (true, null, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user {Username}", username);
            return (false, "An error occurred during registration", null);
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
    /// Gets user by username.
    /// </summary>
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    /// <summary>
    /// Updates user profile information.
    /// </summary>
    public async Task<bool> UpdateProfileAsync(
        int userId, 
        string? displayName = null, 
        string? description = null,
        string? location = null,
        string? website = null,
        int? avatarAssetId = null)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (displayName != null) user.DisplayName = displayName.Trim();
            if (description != null) user.Description = description.Trim();
            if (location != null) user.Location = location.Trim();
            if (website != null) user.Website = website.Trim();
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

    #endregion

    #region Authentication

    /// <summary>
    /// Authenticates a user and creates a new session.
    /// </summary>
    public async Task<(bool Success, string? Error, UserSession? Session, User? User)> LoginAsync(
        string usernameOrEmail, 
        string password,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
                return (false, "Username or email is required", null, null);

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required", null, null);

            // Find user by username or email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => 
                    u.Username.ToLower() == usernameOrEmail.ToLower() || 
                    u.Email.ToLower() == usernameOrEmail.ToLower());

            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Username} not found", usernameOrEmail);
                return (false, "Invalid username or password", null, null);
            }

            // Check if account is locked
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                var remainingTime = (user.LockoutEnd.Value - DateTime.UtcNow).Minutes;
                _logger.LogWarning("Login failed: User {Username} is locked for {Minutes} more minutes", 
                    usernameOrEmail, remainingTime);
                return (false, $"Account is locked. Try again in {remainingTime} minutes.", null, null);
            }

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash))
            {
                // Increment failed login attempts
                user.FailedLoginAttempts++;
                
                var securitySettings = _configuration.GetSection("Security");
                var maxAttempts = securitySettings.GetValue<int>("MaxLoginAttempts", 5);
                var lockoutMinutes = securitySettings.GetValue<int>("LockoutDurationMinutes", 15);

                if (user.FailedLoginAttempts >= maxAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);
                    _logger.LogWarning("User {Username} has been locked out due to {Attempts} failed attempts",
                        usernameOrEmail, maxAttempts);
                }

                await _context.SaveChangesAsync();
                _logger.LogWarning("Login failed: Invalid password for user {Username}", usernameOrEmail);
                return (false, "Invalid username or password", null, null);
            }

            // Check if banned
            if (user.IsBanned)
            {
                _logger.LogWarning("Login failed: User {Username} is banned. Reason: {Reason}", 
                    usernameOrEmail, user.BanReason ?? "No reason provided");
                return (false, "This account has been banned", null, null);
            }

            // Reset failed login attempts on successful login
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            user.LastLoginAt = DateTime.UtcNow;
            user.LoginCount++;
            user.UpdatedAt = DateTime.UtcNow;

            // Create session
            var session = await CreateSessionAsync(user.Id, ipAddress, userAgent);

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} logged in successfully from IP {IpAddress}", 
                usernameOrEmail, ipAddress);
            
            return (true, null, session, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", usernameOrEmail);
            return (false, "An error occurred during login", null, null);
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
                session.Status = "LoggedOut";
                session.LastAccessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("User logged out successfully. Session: {SessionId}", session.Id);
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
    /// Validates a session token and returns the associated user.
    /// </summary>
    public async Task<(bool Valid, UserSession? Session, User? User)> ValidateSessionAsync(string sessionToken)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionToken))
                return (false, null, null);

            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

            if (session == null)
                return (false, null, null);

            // Check if session is expired
            if (!session.IsActive || session.ExpiresAt < DateTime.UtcNow)
            {
                session.IsActive = false;
                session.Status = "Expired";
                await _context.SaveChangesAsync();
                return (false, null, null);
            }

            // Check if user is banned
            if (session.User?.IsBanned == true)
            {
                session.IsActive = false;
                session.Status = "Banned";
                await _context.SaveChangesAsync();
                return (false, null, null);
            }

            // Update last accessed time
            session.LastAccessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, session, session.User);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating session");
            return (false, null, null);
        }
    }

    #endregion

    #region Session Management

    /// <summary>
    /// Creates an authentication session for a user.
    /// </summary>
    public async Task<UserSession> CreateSessionAsync(
        int userId, 
        string? ipAddress = null, 
        string? userAgent = null,
        bool isPersistent = false)
    {
        var sessionToken = GenerateSessionToken();
        var expirationHours = _configuration.GetValue<int>("Authentication:SessionExpirationHours", 24);
        
        var session = new UserSession
        {
            SessionToken = sessionToken,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceType = ParseDeviceType(userAgent),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
            LastAccessedAt = DateTime.UtcNow,
            IsActive = true,
            IsPersistent = isPersistent,
            Status = "Active"
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Session created for user {UserId}: {Token}...", userId, sessionToken.Substring(0, 8));
        return session;
    }

    /// <summary>
    /// Generates a secure session token.
    /// </summary>
    private string GenerateSessionToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var token = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
        return token;
    }

    /// <summary>
    /// Parses device type from User-Agent string.
    /// </summary>
    private string? ParseDeviceType(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "Unknown";

        if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase))
            return "Mobile";
        if (userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase))
            return "Tablet";
        if (userAgent.Contains("Roblox", StringComparison.OrdinalIgnoreCase))
            return "RobloxClient";
        
        return "Desktop";
    }

    #endregion

    #region Password Management

    /// <summary>
    /// Hashes a password using BCrypt.
    /// </summary>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    /// <summary>
    /// Verifies a password against a stored hash.
    /// </summary>
    public bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Changes user password.
    /// </summary>
    public async Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return (false, "User not found");

            if (!VerifyPassword(currentPassword, user.PasswordHash))
                return (false, "Current password is incorrect");

            var securitySettings = _configuration.GetSection("Security");
            var minLength = securitySettings.GetValue<int>("PasswordMinLength", 6);
            if (newPassword.Length < minLength)
                return (false, $"Password must be at least {minLength} characters");

            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Invalidate all existing sessions
            var sessions = await _context.Sessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
                session.Status = "PasswordChanged";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password changed for user {UserId}", userId);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return (false, "An error occurred while changing password");
        }
    }

    /// <summary>
    /// Initiates password reset process.
    /// </summary>
    public async Task<string?> InitiatePasswordResetAsync(string email)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
                return null; // Don't reveal if email exists

            var resetToken = GenerateSessionToken();
            user.PasswordResetToken = resetToken;
            user.PasswordResetExpiresAt = DateTime.UtcNow.AddHours(1);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password reset initiated for user {UserId}", user.Id);
            return resetToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating password reset for {Email}", email);
            return null;
        }
    }

    /// <summary>
    /// Resets password using token.
    /// </summary>
    public async Task<(bool Success, string? Error)> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => 
                    u.PasswordResetToken == token && 
                    u.PasswordResetExpiresAt > DateTime.UtcNow);

            if (user == null)
                return (false, "Invalid or expired reset token");

            var securitySettings = _configuration.GetSection("Security");
            var minLength = securitySettings.GetValue<int>("PasswordMinLength", 6);
            if (newPassword.Length < minLength)
                return (false, $"Password must be at least {minLength} characters");

            user.PasswordHash = HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            // Invalidate all existing sessions
            var sessions = await _context.Sessions
                .Where(s => s.UserId == user.Id && s.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
                session.Status = "PasswordReset";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password reset completed for user {UserId}", user.Id);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return (false, "An error occurred while resetting password");
        }
    }

    #endregion

    #region Session Cleanup

    /// <summary>
    /// Cleans up expired sessions.
    /// </summary>
    public async Task<int> CleanupExpiredSessionsAsync()
    {
        try
        {
            var expiredSessions = await _context.Sessions
                .Where(s => s.ExpiresAt < DateTime.UtcNow || !s.IsActive)
                .ToListAsync();

            _context.Sessions.RemoveRange(expiredSessions);
            await _context.SaveChangesAsync();

            if (expiredSessions.Count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
            }

            return expiredSessions.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired sessions");
            return 0;
        }
    }

    #endregion
}