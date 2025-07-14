using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StarConflictsRevolt.Server.WebApi.Datastore.Entities;
using StarConflictsRevolt.Server.WebApi.Datastore.SeedData;

namespace StarConflictsRevolt.Server.WebApi.Datastore;

public class GameDbContext(DbContextOptions<GameDbContext> options, IEnumerable<IInterceptor> interceptors) : DbContext(options)
{
    public DbSet<Session> Sessions { get; set; }

    public DbSet<World> Worlds { get; set; }
    public DbSet<Galaxy> Galaxies { get; set; }
    public DbSet<StarSystem> StarSystems { get; set; }
    public DbSet<Planet> Planets { get; set; }
    public DbSet<Fleet> Fleets { get; set; }
    public DbSet<Ship> Ships { get; set; }
    public DbSet<Structure> Structures { get; set; }
    public DbSet<PlayerStats> PlayerStats { get; set; }
    public DbSet<Client> Clients { get; set; }

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
            .HasKey(s => s.Id)
            ;
        modelBuilder.Entity<StarSystem>()
            .Property(s => s.Coordinates)
            .HasConversion(
                v => v.ToString(),
                v => ParseVector2(v));

        modelBuilder.Entity<Planet>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Session>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<Session>()
            .HasOne(s => s.Client)
            .WithMany(c => c.Sessions)
            .HasForeignKey(s => s.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Client>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Client>()
            .Property(c => c.Id)
            .IsRequired()
            .HasMaxLength(100)
            .ValueGeneratedNever();


        // --- Static Data Seeding ---
        modelBuilder.Entity<Ship>().HasData(
            new List<Ship>(new ShipCollection())
        );
        modelBuilder.Entity<Fleet>().HasData(
            new List<Fleet>(new FleetCollection())
        );
        modelBuilder.Entity<Structure>().HasData(
            new List<Structure>(new StructureCollection())
        );
    }

    private static Vector2 ParseVector2(string s)
    {
        var parts = s.Trim('(', ')').Split(',');
        if (parts.Length != 2)
            throw new FormatException($"Invalid Vector2 format: {s}");

        return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
    }
}