using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RopeyDVD.Migrations
{
    public partial class CastMember : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CastMember",
                columns: table => new
                {
                    DVDNumber = table.Column<int>(type: "int", nullable: false),
                    ActorNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CastMember", x => new { x.DVDNumber, x.ActorNumber });
                    table.ForeignKey(
                        name: "FK_CastMember_Actors_ActorNumber",
                        column: x => x.ActorNumber,
                        principalTable: "Actors",
                        principalColumn: "ActorNumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CastMember_DVDTitle_DVDNumber",
                        column: x => x.DVDNumber,
                        principalTable: "DVDTitle",
                        principalColumn: "DVDNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CastMember_ActorNumber",
                table: "CastMember",
                column: "ActorNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CastMember");
        }
    }
}
