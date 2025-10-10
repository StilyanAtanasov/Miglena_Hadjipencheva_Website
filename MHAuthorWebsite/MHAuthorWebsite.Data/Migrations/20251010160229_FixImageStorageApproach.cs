using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixImageStorageApproach : Migration
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

            migrationBuilder.Sql(@"
                INSERT INTO [ProductsImages] ([Id], [ImageUrl], [PublicId], [AltText], [ProductId])
                SELECT [Id], [ImageUrl], [PublicId], [AltText], [ProductId]
                FROM [Images]
            ");

            migrationBuilder.Sql(@"
                INSERT INTO [ProductsImages] ([Id], [ImageUrl], [PublicId], [AltText], [ProductId])
                SELECT NEWID(), [ThumbnailUrl], [ThumbnailPublicId], [AltText], [ProductId]
                FROM [Images]
                WHERE [IsThumbnail] = 1
            ");

            migrationBuilder.CreateTable(
                name: "ProductsThumbnails",
                columns: table => new
                {
                    ImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Foreign key to ProductImage"),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Foreign key to Product"),
                    ImageOriginalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Foreign key to the original image")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsThumbnails", x => new { x.ImageId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_ProductsThumbnails_ProductsImages_ImageId",
                        column: x => x.ImageId,
                        principalTable: "ProductsImages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductsThumbnails_ProductsImages_ImageOriginalId",
                        column: x => x.ImageOriginalId,
                        principalTable: "ProductsImages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductsThumbnails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                INSERT INTO [ProductsThumbnails] ([ImageId], [ProductId], [ImageOriginalId])
                SELECT [thumb].[Id], [orig].[ProductId], [orig].[Id]
                FROM [ProductsImages] AS [orig]
                INNER JOIN [Images] AS [img] ON [img].[Id] = [orig].[Id]
                INNER JOIN [ProductsImages] AS [thumb] 
                    ON [thumb].[ProductId] = [orig].[ProductId] AND [thumb].[ImageUrl] = [img].[ThumbnailUrl]
                WHERE [img].[IsThumbnail] = 1
            ");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsImages_ProductId",
                table: "ProductsImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsThumbnails_ImageOriginalId",
                table: "ProductsThumbnails",
                column: "ImageOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsThumbnails_ProductId",
                table: "ProductsThumbnails",
                column: "ProductId");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.AlterColumn<short>(
                name: "Rating",
                table: "ProductComments",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint");
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

            // Restore original images
            migrationBuilder.Sql(@"
                INSERT INTO [Images] ([Id], [ImageUrl], [PublicId], [AltText], [ProductId], [IsThumbnail])
                SELECT [orig].[Id], [orig].[ImageUrl], [orig].[PublicId], [orig].[AltText], [orig].[ProductId], 0
                FROM [ProductsImages] AS [orig]
                LEFT JOIN [ProductsThumbnails] AS [pt] ON [pt].[ImageOriginalId] = [orig].[Id]
            ");

            // Restore thumbnails info
            migrationBuilder.Sql(@"
                UPDATE [i]
                SET [i].[IsThumbnail] = 1,
                    [i].[ThumbnailUrl] = [thumb].[ImageUrl],
                    [i].[ThumbnailPublicId] = [thumb].[PublicId]
                FROM [Images] AS [i]
                INNER JOIN [ProductsThumbnails] AS [pt] ON [pt].[ImageOriginalId] = [i].[Id]
                INNER JOIN [ProductsImages] AS [thumb] ON [thumb].[Id] = [pt].[ImageId]
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Images_ProductId",
                table: "Images",
                column: "ProductId");

            migrationBuilder.DropTable(
                name: "ProductsThumbnails");

            migrationBuilder.DropTable(
                name: "ProductsImages");

            migrationBuilder.AlterColumn<short>(
                name: "Rating",
                table: "ProductComments",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);
        }
    }
}
