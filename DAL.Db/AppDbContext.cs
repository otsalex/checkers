using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class AppDbContext : DbContext
{
    public DbSet<CheckersGame> CheckersGames { get; set; } = default!;
    public DbSet<CheckersOption> CheckersOptions { get; set; } = default!;
    public DbSet<CheckersGameState> CheckersGameStates { get; set; } = default!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CheckersGame>()
            .HasOne(g => g.CheckersOption)
            .WithMany(go => go.CheckersGames)
            .HasForeignKey(g => g.CheckersOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}