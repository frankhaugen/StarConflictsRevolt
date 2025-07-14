#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Clients",
            table => new
            {
                Id = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: false),
                LastSeen = table.Column<DateTime>("datetime2", nullable: false),
                IsActive = table.Column<bool>("bit", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Clients", x => x.Id); });

        migrationBuilder.CreateTable(
            "Fleets",
            table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                Name = table.Column<string>("nvarchar(max)", nullable: false),
                OwnerId = table.Column<Guid>("uniqueidentifier", nullable: false),
                Status = table.Column<int>("int", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Fleets", x => x.Id); });

        migrationBuilder.CreateTable(
            "Galaxies",
            table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Galaxies", x => x.Id); });

        migrationBuilder.CreateTable(
            "PlayerStats",
            table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                PlayerId = table.Column<Guid>("uniqueidentifier", nullable: false),
                SessionId = table.Column<Guid>("uniqueidentifier", nullable: false),
                PlayerName = table.Column<string>("nvarchar(max)", nullable: false),
                FleetsOwned = table.Column<int>("int", nullable: false),
                PlanetsControlled = table.Column<int>("int", nullable: false),
                StructuresBuilt = table.Column<int>("int", nullable: false),
                BattlesWon = table.Column<int>("int", nullable: false),
                BattlesLost = table.Column<int>("int", nullable: false),
                LastUpdated = table.Column<DateTime>("datetime2", nullable: false),
                Created = table.Column<DateTime>("datetime2", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_PlayerStats", x => x.Id); });

        migrationBuilder.CreateTable(
            "Sessions",
            table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                SessionName = table.Column<string>("nvarchar(max)", nullable: false),
                Created = table.Column<DateTime>("datetime2", nullable: false),
                IsActive = table.Column<bool>("bit", nullable: false),
                Ended = table.Column<DateTime>("datetime2", nullable: true),
                SessionType = table.Column<int>("int", nullable: false),
                ClientId = table.Column<string>("nvarchar(100)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Sessions", x => x.Id);
                table.ForeignKey(
                    "FK_Sessions_Clients_ClientId",
                    x => x.ClientId,
                    "Clients",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "Ships",
            table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                Model = table.Column<string>("nvarchar(max)", nullable: false),
                IsUnderConstruction = table.Column<bool>("bit", nullable: false),
                FleetId = table.Column<Guid>("uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Ships", x => x.Id);
                table.ForeignKey(
                    "FK_Ships_Fleets_FleetId",
                    x => x.FleetId,
                    "Fleets",
                    "Id");
            });

        migrationBuilder.CreateTable(
            "StarSystems",
            table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                Name = table.Column<string>("nvarchar(max)", nullable: false),
                Coordinates = table.Column<string>("nvarchar(max)", nullable: false),
                GalaxyId = table.Column<Guid>("uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StarSystems", x => x.Id);
                table.ForeignKey(
                    "FK_StarSystems_Galaxies_GalaxyId",
                    x => x.GalaxyId,
                    "Galaxies",
                    "Id");
            });

        migrationBuilder.CreateTable(
            "Worlds",
            table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                GalaxyId = table.Column<Guid>("uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Worlds", x => x.Id);
                table.ForeignKey(
                    "FK_Worlds_Galaxies_GalaxyId",
                    x => x.GalaxyId,
                    "Galaxies",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "Planets",
            table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                Name = table.Column<string>("nvarchar(max)", nullable: false),
                Radius = table.Column<double>("float", nullable: false),
                Mass = table.Column<double>("float", nullable: false),
                RotationSpeed = table.Column<double>("float", nullable: false),
                OrbitSpeed = table.Column<double>("float", nullable: false),
                DistanceFromSun = table.Column<double>("float", nullable: false),
                StarSystemId = table.Column<Guid>("uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Planets", x => x.Id);
                table.ForeignKey(
                    "FK_Planets_StarSystems_StarSystemId",
                    x => x.StarSystemId,
                    "StarSystems",
                    "Id");
            });

        migrationBuilder.CreateTable(
            "Structures",
            table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                Variant = table.Column<int>("int", nullable: false),
                PlanetId = table.Column<Guid>("uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Structures", x => x.Id);
                table.ForeignKey(
                    "FK_Structures_Planets_PlanetId",
                    x => x.PlanetId,
                    "Planets",
                    "Id");
            });

        migrationBuilder.InsertData(
            "Fleets",
            new[] { "Id", "Name", "OwnerId", "Status" },
            new object[,]
            {
                { new Guid("00000000-0000-0000-0000-000000000001"), "Rebel Flagship", new Guid("00000000-0000-0000-0000-000000000000"), 0 },
                { new Guid("00000000-0000-0000-0000-000000000002"), "Imperial Fleet", new Guid("00000000-0000-0000-0000-000000000000"), 0 }
            });

        migrationBuilder.InsertData(
            "Ships",
            new[] { "Id", "FleetId", "IsUnderConstruction", "Model" },
            new object[,]
            {
                { new Guid("10000000-0000-0000-0000-000000000001"), null, false, "X-Wing" },
                { new Guid("10000000-0000-0000-0000-000000000002"), null, false, "Y-Wing" },
                { new Guid("10000000-0000-0000-0000-000000000003"), null, false, "TIE Fighter" },
                { new Guid("10000000-0000-0000-0000-000000000004"), null, false, "Star Destroyer" }
            });

        migrationBuilder.InsertData(
            "Structures",
            new[] { "Id", "PlanetId", "Variant" },
            new object[,]
            {
                { new Guid("00000000-0000-0000-0000-200000000001"), null, 0 },
                { new Guid("00000000-0000-0000-0000-200000000002"), null, 1 },
                { new Guid("00000000-0000-0000-0000-200000000003"), null, 2 },
                { new Guid("00000000-0000-0000-0000-200000000004"), null, 3 },
                { new Guid("00000000-0000-0000-0000-200000000005"), null, 4 },
                { new Guid("00000000-0000-0000-0000-200000000006"), null, 5 }
            });

        migrationBuilder.CreateIndex(
            "IX_Planets_StarSystemId",
            "Planets",
            "StarSystemId");

        migrationBuilder.CreateIndex(
            "IX_Sessions_ClientId",
            "Sessions",
            "ClientId");

        migrationBuilder.CreateIndex(
            "IX_Ships_FleetId",
            "Ships",
            "FleetId");

        migrationBuilder.CreateIndex(
            "IX_StarSystems_GalaxyId",
            "StarSystems",
            "GalaxyId");

        migrationBuilder.CreateIndex(
            "IX_Structures_PlanetId",
            "Structures",
            "PlanetId");

        migrationBuilder.CreateIndex(
            "IX_Worlds_GalaxyId",
            "Worlds",
            "GalaxyId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "PlayerStats");

        migrationBuilder.DropTable(
            "Sessions");

        migrationBuilder.DropTable(
            "Ships");

        migrationBuilder.DropTable(
            "Structures");

        migrationBuilder.DropTable(
            "Worlds");

        migrationBuilder.DropTable(
            "Clients");

        migrationBuilder.DropTable(
            "Fleets");

        migrationBuilder.DropTable(
            "Planets");

        migrationBuilder.DropTable(
            "StarSystems");

        migrationBuilder.DropTable(
            "Galaxies");
    }
}