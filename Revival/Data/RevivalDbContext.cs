using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using Revival.Models;

namespace Revival.Data;

/// <summary>
/// Entity Framework Core database context for Roblox Revival.
/// Configures MySQL connection and model relationships.
/// </summary>
public class RevivalDbContext : DbContext
{
    public RevivalDbContext(DbContextOptions<RevivalDbContext> options) : base(options)
    {
    }

    // DbSets - Database Tables
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserSession> Sessions { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Place> Places { get; set; } = null!;
    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<GameServer> GameServers { get; set; } = null!;
    public DbSet<GameServerPlayer> GameServerPlayers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================
        // USER CONFIGURATION
        // ============================================
        modelBuilder.Entity<User>(entity =>
        {
            // Unique indexes
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.EmailVerificationToken).IsUnique();
            entity.HasIndex(e => e.PasswordResetToken).IsUnique();

            // Default values
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        // ============================================
        // SESSION CONFIGURATION
        // ============================================
        modelBuilder.Entity<UserSession>(entity =>
        {
            // Unique session token
            entity.HasIndex(e => e.SessionToken).IsUnique();
            
            // Query performance indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => new { e.IsActive, e.ExpiresAt });

            // Relationships
            entity.HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Default values
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        // ============================================
        // GAME CONFIGURATION
        // ============================================
        modelBuilder.Entity<Game>(entity =>
        {
            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CreatorId);
            entity.HasIndex(e => e.IsPublic);
            entity.HasIndex(e => e.IsFeatured);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.TotalPlays);
            entity.HasIndex(e => new { e.IsPublic, e.IsActive });

            // Relationships
            entity.HasOne(g => g.Creator)
                .WithMany(u => u.CreatedGames)
                .HasForeignKey(g => g.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Default values
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        // ============================================
        // PLACE CONFIGURATION
        // ============================================
        modelBuilder.Entity<Place>(entity =>
        {
            // Indexes
            entity.HasIndex(e => e.GameId);
            entity.HasIndex(e => e.IsActive);

            // Relationships
            entity.HasOne(p => p.Game)
                .WithMany(g => g.Places)
                .HasForeignKey(p => p.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            // Default values
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        // ============================================
        // ASSET CONFIGURATION
        // ============================================
        modelBuilder.Entity<Asset>(entity =>
        {
            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.CreatorId);
            entity.HasIndex(e => e.IsPublic);
            entity.HasIndex(e => e.Hash);

            // Relationships
            entity.HasOne(a => a.Creator)
                .WithMany(u => u.CreatedAssets)
                .HasForeignKey(a => a.CreatorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Default values
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        // ============================================
        // GAME SERVER CONFIGURATION
        // ============================================
        modelBuilder.Entity<GameServer>(entity =>
        {
            // Unique indexes
            entity.HasIndex(e => e.ServerId).IsUnique();
            
            // Query performance indexes
            entity.HasIndex(e => e.PlaceId);
            entity.HasIndex(e => e.GameId);
            entity.HasIndex(e => e.RCCJobId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.Status, e.CurrentPlayers, e.MaxPlayers });
            entity.HasIndex(e => e.ExpiresAt);

            // Relationships
            entity.HasOne(gs => gs.Place)
                .WithMany(p => p.GameServers)
                .HasForeignKey(gs => gs.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gs => gs.Game)
                .WithMany(g => g.GameServers)
                .HasForeignKey(gs => gs.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gs => gs.Creator)
                .WithMany(u => u.GameServers)
                .HasForeignKey(gs => gs.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Default values
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        // ============================================
        // GAME SERVER PLAYER CONFIGURATION
        // ============================================
        modelBuilder.Entity<GameServerPlayer>(entity =>
        {
            // Indexes
            entity.HasIndex(e => e.ServerId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.JoinedAt);

            // Relationships
            entity.HasOne(gsp => gsp.Server)
                .WithMany()
                .HasForeignKey(gsp => gsp.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gsp => gsp.User)
                .WithMany()
                .HasForeignKey(gsp => gsp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Override SaveChanges to automatically update timestamps
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update timestamps
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Automatically update CreatedAt and UpdatedAt timestamps
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // Update UpdatedAt for all modified entities
            if (entry.State == EntityState.Modified)
            {
                var updatedProperty = entry.Properties
                    .FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                
                if (updatedProperty != null)
                {
                    updatedProperty.CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }
}

/// <summary>
/// Design-time factory for creating DbContext during migrations
/// </summary>
public class RevivalDbContextFactory : IDesignTimeDbContextFactory<RevivalDbContext>
{
    public RevivalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RevivalDbContext>();

        // Get connection string from environment or use default
        var connectionString = Environment.GetEnvironmentVariable("DefaultConnection") 
            ?? "Server=localhost;Port=3306;Database=Revival;User=root;Password=your_password;CharSet=utf8mb4;SslMode=None;";

        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new RevivalDbContext(optionsBuilder.Options);
    }
}