using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StarConflictsRevolt.Server.Core;

namespace StarConflictsRevolt.Server.Datastore;

public class GameDbContext(DbContextOptions<GameDbContext> options, IEnumerable<IInterceptor> interceptors) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .AddInterceptors(interceptors.ToArray())
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            ;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<World>()
            .HasKey(w => w.Id);

        modelBuilder.Entity<Galaxy>()
            .HasKey(g => g.Id);
        
        modelBuilder.Entity<StarSystem>()
            .HasKey(s => s.Id);
        
        modelBuilder.Entity<Planet>()
            .HasKey(p => p.Id);
        
        modelBuilder.Entity<Session>()
            .HasKey(s => s.Id);
    }

    public DbSet<Session> Sessions { get; set; }
    
    public DbSet<World> Worlds { get; set; }
    public DbSet<Galaxy> Galaxies { get; set; }
    public DbSet<StarSystem> StarSystems { get; set; }
    public DbSet<Planet> Planets { get; set; }
}
