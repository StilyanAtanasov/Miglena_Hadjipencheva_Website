using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImageURLMaxLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ProductsImages",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ProductsImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048);
        }
    }
}
