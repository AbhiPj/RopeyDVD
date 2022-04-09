using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RopeyDVD.Migrations
{
    public partial class DVDTitle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DVDTitle",
                columns: table => new
                {
                    DVDNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProducerNumber = table.Column<int>(type: "int", nullable: false),
                    CategoryNumber = table.Column<int>(type: "int", nullable: false),
                    DVDCategoryCategoryNumber = table.Column<int>(type: "int", nullable: false),
                    StudioNumber = table.Column<int>(type: "int", nullable: false),
                    DateReleased = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StandardCharge = table.Column<int>(type: "int", nullable: false),
                    PenaltyCharge = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DVDTitle", x => x.DVDNumber);
                    table.ForeignKey(
                        name: "FK_DVDTitle_DVDCategory_DVDCategoryCategoryNumber",
                        column: x => x.DVDCategoryCategoryNumber,
                        principalTable: "DVDCategory",
                        principalColumn: "CategoryNumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DVDTitle_Producers_ProducerNumber",
                        column: x => x.ProducerNumber,
                        principalTable: "Producers",
                        principalColumn: "ProducerNumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DVDTitle_Studios_StudioNumber",
                        column: x => x.StudioNumber,
                        principalTable: "Studios",
                        principalColumn: "StudioNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DVDTitle_DVDCategoryCategoryNumber",
                table: "DVDTitle",
                column: "DVDCategoryCategoryNumber");

            migrationBuilder.CreateIndex(
                name: "IX_DVDTitle_ProducerNumber",
                table: "DVDTitle",
                column: "ProducerNumber");

            migrationBuilder.CreateIndex(
                name: "IX_DVDTitle_StudioNumber",
                table: "DVDTitle",
                column: "StudioNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DVDTitle");
        }
    }
}
