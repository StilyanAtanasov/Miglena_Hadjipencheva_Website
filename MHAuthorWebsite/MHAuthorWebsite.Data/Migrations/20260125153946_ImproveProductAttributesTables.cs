using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImproveProductAttributesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAttributes_ProductAttributeOptions_ProductAttributeOptionsId",
                table: "ProductAttributes");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "ProductAttributes");

            migrationBuilder.DropColumn(
                name: "HasPredefinedValue",
                table: "ProductAttributeDefinitions");

            migrationBuilder.RenameColumn(
                name: "ProductAttributeOptionsId",
                table: "ProductAttributes",
                newName: "ProductAttributeOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductAttributes_ProductAttributeOptionsId",
                table: "ProductAttributes",
                newName: "IX_ProductAttributes_ProductAttributeOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAttributes_ProductAttributeOptions_ProductAttributeOptionId",
                table: "ProductAttributes",
                column: "ProductAttributeOptionId",
                principalTable: "ProductAttributeOptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAttributes_ProductAttributeOptions_ProductAttributeOptionId",
                table: "ProductAttributes");

            migrationBuilder.RenameColumn(
                name: "ProductAttributeOptionId",
                table: "ProductAttributes",
                newName: "ProductAttributeOptionsId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductAttributes_ProductAttributeOptionId",
                table: "ProductAttributes",
                newName: "IX_ProductAttributes_ProductAttributeOptionsId");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "ProductAttributes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HasPredefinedValue",
                table: "ProductAttributeDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAttributes_ProductAttributeOptions_ProductAttributeOptionsId",
                table: "ProductAttributes",
                column: "ProductAttributeOptionsId",
                principalTable: "ProductAttributeOptions",
                principalColumn: "Id");
        }
    }
}
