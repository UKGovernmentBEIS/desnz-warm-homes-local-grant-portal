using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HerPortal.Data.Migrations
{
    public partial class TrackAllCsvFileDownloads : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CsvFileDownloads_Users_UserId",
                table: "CsvFileDownloads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CsvFileDownloads",
                table: "CsvFileDownloads");

            migrationBuilder.DropColumn(
                name: "CustodianCode",
                table: "CsvFileDownloads");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "CsvFileDownloads");

            migrationBuilder.DropColumn(
                name: "LastDownloaded",
                table: "CsvFileDownloads");

            migrationBuilder.RenameTable(
                name: "CsvFileDownloads",
                newName: "CsvFileDownload");

            migrationBuilder.RenameColumn(
                name: "Month",
                table: "CsvFileDownload",
                newName: "CsvFileId");

            migrationBuilder.RenameIndex(
                name: "IX_CsvFileDownloads_UserId",
                table: "CsvFileDownload",
                newName: "IX_CsvFileDownload_UserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "CsvFileDownload",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CsvFileDownload",
                table: "CsvFileDownload",
                columns: new[] { "CsvFileId", "UserId", "Timestamp" });

            migrationBuilder.CreateTable(
                name: "TrackedCsvFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustodianCode = table.Column<string>(type: "text", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedCsvFiles", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CsvFileDownload_TrackedCsvFiles_CsvFileId",
                table: "CsvFileDownload",
                column: "CsvFileId",
                principalTable: "TrackedCsvFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CsvFileDownload_Users_UserId",
                table: "CsvFileDownload",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CsvFileDownload_TrackedCsvFiles_CsvFileId",
                table: "CsvFileDownload");

            migrationBuilder.DropForeignKey(
                name: "FK_CsvFileDownload_Users_UserId",
                table: "CsvFileDownload");

            migrationBuilder.DropTable(
                name: "TrackedCsvFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CsvFileDownload",
                table: "CsvFileDownload");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "CsvFileDownload");

            migrationBuilder.RenameTable(
                name: "CsvFileDownload",
                newName: "CsvFileDownloads");

            migrationBuilder.RenameColumn(
                name: "CsvFileId",
                table: "CsvFileDownloads",
                newName: "Month");

            migrationBuilder.RenameIndex(
                name: "IX_CsvFileDownload_UserId",
                table: "CsvFileDownloads",
                newName: "IX_CsvFileDownloads_UserId");

            migrationBuilder.AddColumn<string>(
                name: "CustodianCode",
                table: "CsvFileDownloads",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "CsvFileDownloads",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDownloaded",
                table: "CsvFileDownloads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CsvFileDownloads",
                table: "CsvFileDownloads",
                columns: new[] { "CustodianCode", "Year", "Month", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CsvFileDownloads_Users_UserId",
                table: "CsvFileDownloads",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
