using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeImageThumbnailPublicIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ThumbnailPublicId",
                table: "Images",
                type: "nvarchar(max)",
                nullable: true,
                comment: "The publicId for thumbnail in Cloudinary",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldComment: "The publicId for thumbnail in Cloudinary");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ThumbnailPublicId",
                table: "Images",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                comment: "The publicId for thumbnail in Cloudinary",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "The publicId for thumbnail in Cloudinary");
        }
    }
}
