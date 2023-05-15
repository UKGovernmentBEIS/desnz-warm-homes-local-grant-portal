using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HerPortal.Data.Migrations
{
    public partial class AddCsvFileDownloadData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CsvFileDownloadData",
                columns: table => new
                {
                    CustodianCode = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CsvFileDownloadData", x => new { x.CustodianCode, x.Year, x.Month });
                });

            migrationBuilder.CreateTable(
                name: "CsvFileDownload",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    CsvFileDownloadDataCustodianCode = table.Column<string>(type: "text", nullable: true),
                    CsvFileDownloadDataMonth = table.Column<int>(type: "integer", nullable: true),
                    CsvFileDownloadDataYear = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CsvFileDownload", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CsvFileDownload_CsvFileDownloadData_CsvFileDownloadDataCust~",
                        columns: x => new { x.CsvFileDownloadDataCustodianCode, x.CsvFileDownloadDataYear, x.CsvFileDownloadDataMonth },
                        principalTable: "CsvFileDownloadData",
                        principalColumns: new[] { "CustodianCode", "Year", "Month" });
                    table.ForeignKey(
                        name: "FK_CsvFileDownload_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CsvFileDownload_CsvFileDownloadDataCustodianCode_CsvFileDow~",
                table: "CsvFileDownload",
                columns: new[] { "CsvFileDownloadDataCustodianCode", "CsvFileDownloadDataYear", "CsvFileDownloadDataMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_CsvFileDownload_UserId",
                table: "CsvFileDownload",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CsvFileDownload");

            migrationBuilder.DropTable(
                name: "CsvFileDownloadData");
        }
    }
}
