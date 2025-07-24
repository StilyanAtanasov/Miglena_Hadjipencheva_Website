using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFKFromProductAttributesToProductAttributeDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttributeDefinitionId",
                table: "ProductAttributes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Foreign key to Product Attribute Definition");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_AttributeDefinitionId",
                table: "ProductAttributes",
                column: "AttributeDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAttributes_ProductAttributeDefinitions_AttributeDefinitionId",
                table: "ProductAttributes",
                column: "AttributeDefinitionId",
                principalTable: "ProductAttributeDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAttributes_ProductAttributeDefinitions_AttributeDefinitionId",
                table: "ProductAttributes");

            migrationBuilder.DropIndex(
                name: "IX_ProductAttributes_AttributeDefinitionId",
                table: "ProductAttributes");

            migrationBuilder.DropColumn(
                name: "AttributeDefinitionId",
                table: "ProductAttributes");
        }
    }
}
