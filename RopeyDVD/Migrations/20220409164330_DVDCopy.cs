using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RopeyDVD.Migrations
{
    public partial class DVDCopy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DVDCopy",
                columns: table => new
                {
                    CopyNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DVDNumber = table.Column<int>(type: "int", nullable: false),
                    DatePurchased = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DVDCopy", x => x.CopyNumber);
                    table.ForeignKey(
                        name: "FK_DVDCopy_DVDTitle_DVDNumber",
                        column: x => x.DVDNumber,
                        principalTable: "DVDTitle",
                        principalColumn: "DVDNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DVDCopy_DVDNumber",
                table: "DVDCopy",
                column: "DVDNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DVDCopy");
        }
    }
}
