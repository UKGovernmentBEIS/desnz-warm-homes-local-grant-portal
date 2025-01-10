using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WhlgPortalWebsite.Data.Migrations
{
    public partial class AddSeparateIdColumnOnAuditDownload : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditDownloads",
                table: "AuditDownloads");

            migrationBuilder.AlterColumn<string>(
                name: "UserEmail",
                table: "AuditDownloads",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CustodianCode",
                table: "AuditDownloads",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AuditDownloads",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditDownloads",
                table: "AuditDownloads",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditDownloads",
                table: "AuditDownloads");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AuditDownloads");

            migrationBuilder.AlterColumn<string>(
                name: "UserEmail",
                table: "AuditDownloads",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustodianCode",
                table: "AuditDownloads",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditDownloads",
                table: "AuditDownloads",
                columns: new[] { "CustodianCode", "Year", "Month", "UserEmail", "Timestamp" });
        }
    }
}
