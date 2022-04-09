using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RopeyDVD.Migrations
{
    public partial class loan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Loan",
                columns: table => new
                {
                    LoanNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanTypeNumber = table.Column<int>(type: "int", nullable: false),
                    CopyNumber = table.Column<int>(type: "int", nullable: false),
                    MemberNumber = table.Column<int>(type: "int", nullable: false),
                    DateOut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateDue = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateReturned = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loan", x => x.LoanNumber);
                    table.ForeignKey(
                        name: "FK_Loan_DVDCopy_CopyNumber",
                        column: x => x.CopyNumber,
                        principalTable: "DVDCopy",
                        principalColumn: "CopyNumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Loan_LoanTypes_LoanTypeNumber",
                        column: x => x.LoanTypeNumber,
                        principalTable: "LoanTypes",
                        principalColumn: "LoanTypeNumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Loan_Member_MemberNumber",
                        column: x => x.MemberNumber,
                        principalTable: "Member",
                        principalColumn: "MembershipNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Loan_CopyNumber",
                table: "Loan",
                column: "CopyNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Loan_LoanTypeNumber",
                table: "Loan",
                column: "LoanTypeNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Loan_MemberNumber",
                table: "Loan",
                column: "MemberNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Loan");
        }
    }
}
