using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhlgPortalWebsite.Data.Migrations
{
    public partial class AddTableToTrackAllCsvFileDownloads : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastDownloaded",
                table: "CsvFileDownloads",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AuditDownloads",
                columns: table => new
                {
                    CustodianCode = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    UserEmail = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditDownloads", x => new { x.CustodianCode, x.Year, x.Month, x.UserEmail, x.Timestamp });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditDownloads");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastDownloaded",
                table: "CsvFileDownloads",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
