using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdornmeStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedProductsAndCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Products",
                newName: "IsVisible");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Categories",
                newName: "IsVisible");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "IsVisible",
                table: "Products",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "IsVisible",
                table: "Categories",
                newName: "IsActive");
        }
    }
}
