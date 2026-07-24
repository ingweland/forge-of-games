using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ingweland.Fog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PvpEliteRankings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pvp_elite_rankings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollectedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pvp_elite_rankings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pvp_elite_rankings_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pvp_elite_rankings_CollectedAt",
                table: "pvp_elite_rankings",
                column: "CollectedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_pvp_elite_rankings_PlayerId",
                table: "pvp_elite_rankings",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pvp_elite_rankings");
        }
    }
}
