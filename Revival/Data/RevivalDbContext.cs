using Microsoft.EntityFrameworkCore;
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

    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Place> Places { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<GameServer> GameServers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasIndex(e => e.SessionToken).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
            
            entity.HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Game configuration
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasOne(g => g.Creator)
                .WithMany(u => u.CreatedGames)
                .HasForeignKey(g => g.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Place configuration
        modelBuilder.Entity<Place>(entity =>
        {
            entity.HasOne(p => p.Game)
                .WithMany(g => g.Places)
                .HasForeignKey(p => p.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GameServer configuration
        modelBuilder.Entity<GameServer>(entity =>
        {
            entity.HasIndex(e => e.ServerId).IsUnique();
            entity.HasIndex(e => e.RCCJobId);
            
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
        });
    }
}