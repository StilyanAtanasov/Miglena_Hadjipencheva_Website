using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeProductThumbnailOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                WITH [RankedThumbnails] AS (
                    SELECT *,
                           ROW_NUMBER() OVER(PARTITION BY [ProductId] ORDER BY [ImageId]) AS [rn]
                    FROM [ProductsThumbnails]
                )
                DELETE FROM [RankedThumbnails]
                WHERE [rn] > 1
            ");

            migrationBuilder.DropIndex(
                name: "IX_ProductsThumbnails_ProductId",
                table: "ProductsThumbnails");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsThumbnails_ProductId",
                table: "ProductsThumbnails",
                column: "ProductId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductsThumbnails_ProductId",
                table: "ProductsThumbnails");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsThumbnails_ProductId",
                table: "ProductsThumbnails",
                column: "ProductId");
        }
    }
}
