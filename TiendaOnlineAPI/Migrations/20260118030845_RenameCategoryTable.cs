using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaOnlineAPI.Migrations
{
    public partial class RenameCategoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Catgories_CategoryId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Catgories",
                table: "Catgories");

            migrationBuilder.RenameTable(
                name: "Catgories",
                newName: "Categories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Catgories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Catgories",
                table: "Catgories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Catgories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Catgories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
