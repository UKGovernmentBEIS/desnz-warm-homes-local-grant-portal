using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HerPortal.Data.Migrations
{
    public partial class RemoveNameFromLocalAuthority : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "LocalAuthorities");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "LocalAuthorities",
                type: "text",
                nullable: true);
        }
    }
}
