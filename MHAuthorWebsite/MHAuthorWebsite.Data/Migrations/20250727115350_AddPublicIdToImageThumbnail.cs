using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicIdToImageThumbnail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPublicId",
                table: "Images",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                comment: "The publicId for thumbnail in Cloudinary");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailPublicId",
                table: "Images");
        }
    }
}
