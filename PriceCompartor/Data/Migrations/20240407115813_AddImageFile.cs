using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceCompartor.Migrations
{
    /// <inheritdoc />
    public partial class AddImageFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageMimeType",
                table: "Products",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageMimeType",
                table: "Platforms",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "PhotoFile",
                table: "Platforms",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageMimeType",
                table: "Categories",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "PhotoFile",
                table: "Categories",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageMimeType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageMimeType",
                table: "Platforms");

            migrationBuilder.DropColumn(
                name: "PhotoFile",
                table: "Platforms");

            migrationBuilder.DropColumn(
                name: "ImageMimeType",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "PhotoFile",
                table: "Categories");
        }
    }
}
