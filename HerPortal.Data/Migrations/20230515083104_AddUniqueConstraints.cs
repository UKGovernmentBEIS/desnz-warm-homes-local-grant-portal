using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HerPortal.Data.Migrations
{
    public partial class AddUniqueConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailAddress",
                table: "Users",
                column: "EmailAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalAuthorities_CustodianCode",
                table: "LocalAuthorities",
                column: "CustodianCode",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_EmailAddress",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_LocalAuthorities_CustodianCode",
                table: "LocalAuthorities");
        }
    }
}
