using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayPositionPropertyForProductAttributesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayPosition",
                table: "ProductAttributes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Defines in which section of the product's details page will it appear");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayPosition",
                table: "ProductAttributes");
        }
    }
}
