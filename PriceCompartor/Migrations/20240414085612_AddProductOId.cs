using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceCompartor.Migrations
{
    /// <inheritdoc />
    public partial class AddProductOId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OId",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OId",
                table: "Products");
        }
    }
}
