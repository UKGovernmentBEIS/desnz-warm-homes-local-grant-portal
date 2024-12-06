using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HerPortal.Data.Migrations
{
    public partial class AddConsortiumModelAndUserListOfConsortiums : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Consortia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConsortiumCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consortia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsortiumUser",
                columns: table => new
                {
                    ConsortiaId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsortiumUser", x => new { x.ConsortiaId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ConsortiumUser_Consortia_ConsortiaId",
                        column: x => x.ConsortiaId,
                        principalTable: "Consortia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsortiumUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsortiumUser_UsersId",
                table: "ConsortiumUser",
                column: "UsersId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsortiumUser");

            migrationBuilder.DropTable(
                name: "Consortia");
        }
    }
}
