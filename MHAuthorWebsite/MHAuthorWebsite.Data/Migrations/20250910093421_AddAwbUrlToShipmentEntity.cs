using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAwbUrlToShipmentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AwbUrl",
                table: "Shipments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                comment: "URL to the Air Waybill (AWB) document");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwbUrl",
                table: "Shipments");
        }
    }
}
