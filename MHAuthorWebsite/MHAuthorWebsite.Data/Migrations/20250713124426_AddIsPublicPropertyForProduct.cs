using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPublicPropertyForProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "Soft delete flag");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Determines if the product should be visible for basic users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Products");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "bit",
                nullable: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");
        }
    }
}
