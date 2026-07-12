using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ingweland.Fog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WoaPlayerStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "woa_player_stats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContributionPoints = table.Column<int>(type: "int", nullable: false),
                    ContributionPointsGainedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HealingDone = table.Column<int>(type: "int", nullable: false),
                    InGameEventId = table.Column<int>(type: "int", nullable: false),
                    LostAttacks = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    RepairsStarted = table.Column<int>(type: "int", nullable: false),
                    VictoryPoints = table.Column<int>(type: "int", nullable: false),
                    WonAttacks = table.Column<int>(type: "int", nullable: false),
                    WonDefenses = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_woa_player_stats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_woa_player_stats_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_woa_player_stats_PlayerId_InGameEventId",
                table: "woa_player_stats",
                columns: new[] { "PlayerId", "InGameEventId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "woa_player_stats");
        }
    }
}
