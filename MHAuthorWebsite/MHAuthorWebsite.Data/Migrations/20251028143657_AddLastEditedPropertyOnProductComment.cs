using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLastEditedPropertyOnProductComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastEdited",
                table: "ProductComments",
                type: "datetime2",
                nullable: true,
                comment: "Last edited at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastEdited",
                table: "ProductComments");
        }
    }
}
