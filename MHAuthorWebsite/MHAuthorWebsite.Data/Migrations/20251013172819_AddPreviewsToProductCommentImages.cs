using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviewsToProductCommentImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviewPublicId",
                table: "ProductCommentsImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: string.Empty,
                comment: "The publicId for the image preview in Cloudinary");

            migrationBuilder.AddColumn<string>(
                name: "PreviewUrl",
                table: "ProductCommentsImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: string.Empty,
                comment: "URL path to the image preview");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewPublicId",
                table: "ProductCommentsImages");

            migrationBuilder.DropColumn(
                name: "PreviewUrl",
                table: "ProductCommentsImages");
        }
    }
}
