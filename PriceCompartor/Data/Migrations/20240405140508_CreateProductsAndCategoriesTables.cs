using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceCompartor.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateProductsAndCategoriesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categiries_CategoryId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categiries",
                table: "Categiries");

            migrationBuilder.RenameTable(
                name: "Categiries",
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

        /// <inheritdoc />
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
                newName: "Categiries");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categiries",
                table: "Categiries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categiries_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categiries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
