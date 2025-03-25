using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhlgPortalWebsite.Data.Migrations
{
    public partial class AddCsvDownloadData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CsvFileDownloads",
                columns: table => new
                {
                    CustodianCode = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LastDownloaded = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CsvFileDownloads", x => new { x.CustodianCode, x.Year, x.Month, x.UserId });
                    table.ForeignKey(
                        name: "FK_CsvFileDownloads_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CsvFileDownloads_UserId",
                table: "CsvFileDownloads",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CsvFileDownloads");
        }
    }
}
