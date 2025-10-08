using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImproveProductImageStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductsImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Primary key"),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "URL path to the image"),
                    PublicId = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "The publicId in Cloudinary"),
                    AltText = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, comment: "Alternative text for accessibility"),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Foreign key to Product")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductsImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductsImages_ProductId",
                table: "ProductsImages",
                column: "ProductId");

            migrationBuilder.AddColumn<Guid>(
                name: "ThumbnailImageId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"
                INSERT INTO [ProductsImages] ([Id], [ImageUrl], [PublicId], [AltText], [ProductId])
                SELECT [Id], [ImageUrl], [PublicId], [AltText], [ProductId]
                FROM [Images]
            ");

            migrationBuilder.Sql(@"
                UPDATE [p]
                SET [ThumbnailImageId] = [pi].[Id]
                FROM [Products] [p]
                INNER JOIN [ProductsImages] [pi]
                    ON [pi].[ProductId] = [p].[Id]
                WHERE [pi].[Id] IN (SELECT [Id] FROM [Images] WHERE [IsThumbnail] = 1)
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductsImages_ThumbnailImageId",
                table: "Products",
                column: "ThumbnailImageId",
                principalTable: "ProductsImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AlterColumn<Guid>(
                name: "ThumbnailImageId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ThumbnailImageId",
                table: "Products",
                column: "ThumbnailImageId",
                unique: true);

            migrationBuilder.DropTable(
                name: "Images");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Primary key"),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Foreign key to Product"),
                    AltText = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, comment: "Alternative text for accessibility"),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "URL path to the image"),
                    IsThumbnail = table.Column<bool>(type: "bit", nullable: false, comment: "Defines whether the image is a thumbnail (the image will be used for product listings)"),
                    PublicId = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "The publicId in Cloudinary"),
                    ThumbnailPublicId = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "The publicId for thumbnail in Cloudinary"),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, comment: "URL path to the thumbnail")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_ProductId",
                table: "Images",
                column: "ProductId");

            migrationBuilder.Sql(@"
                INSERT INTO [Images] ([Id], [ImageUrl], [PublicId], [AltText], [ProductId], [IsThumbnail])
                SELECT [Id], [ImageUrl], [PublicId], [AltText], [ProductId],
                       CASE WHEN [Id] = [p].[ThumbnailImageId] THEN 1 ELSE 0 END
                FROM [ProductsImages] [pi]
                INNER JOIN [Products] [p] ON [p].[ThumbnailImageId] = [pi].[Id]
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductsImages_ThumbnailImageId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ThumbnailImageId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ThumbnailImageId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductsImages");
        }

    }
}