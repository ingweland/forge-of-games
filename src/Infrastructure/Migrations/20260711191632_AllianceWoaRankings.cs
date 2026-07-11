using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ingweland.Fog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllianceWoaRankings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alliance_woa_rankings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllianceId = table.Column<int>(type: "int", nullable: false),
                    CollectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    EloDelta = table.Column<int>(type: "int", nullable: false),
                    EloRating = table.Column<int>(type: "int", nullable: false),
                    ExpectedVictoryPointsShare = table.Column<double>(type: "float", nullable: false),
                    InGameEventId = table.Column<int>(type: "int", nullable: false),
                    VictoryPoints = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alliance_woa_rankings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_alliance_woa_rankings_alliances_AllianceId",
                        column: x => x.AllianceId,
                        principalTable: "alliances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alliance_woa_rankings_AllianceId_InGameEventId_DivisionId",
                table: "alliance_woa_rankings",
                columns: new[] { "AllianceId", "InGameEventId", "DivisionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alliance_woa_rankings_DivisionId",
                table: "alliance_woa_rankings",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_alliance_woa_rankings_EloRating",
                table: "alliance_woa_rankings",
                column: "EloRating",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_alliance_woa_rankings_InGameEventId",
                table: "alliance_woa_rankings",
                column: "InGameEventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alliance_woa_rankings");
        }
    }
}
