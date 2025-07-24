using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsThumbnailPropertyOnImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "Images",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                comment: "Alternative text for accessibility",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true,
                oldComment: "Alternative text for accessibility");

            migrationBuilder.AddColumn<bool>(
                name: "IsThumbnail",
                table: "Images",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Defines whether the image is a thumbnail (the image will be used for product listings)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsThumbnail",
                table: "Images");

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "Images",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                comment: "Alternative text for accessibility",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldComment: "Alternative text for accessibility");
        }
    }
}
