using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeProductAttributeDefinitionDataTypeOfDataTypePropertyToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DataType",
                table: "ProductAttributeDefinitions",
                type: "int",
                maxLength: 30,
                nullable: false,
                comment: "UI data type (e.g., number, date)",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldComment: "UI data type (e.g., number, date)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DataType",
                table: "ProductAttributeDefinitions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                comment: "UI data type (e.g., number, date)",
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 30,
                oldComment: "UI data type (e.g., number, date)");
        }
    }
}
