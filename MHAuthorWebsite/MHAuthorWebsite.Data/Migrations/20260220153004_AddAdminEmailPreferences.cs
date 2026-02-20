using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminEmailPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminNotificationPreferences",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceiveNewOrderEmails = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ReceiveContactRequestEmails = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ReceiveServerErrorEmails = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminNotificationPreferences", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_AdminNotificationPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminNotificationPreferences");
        }
    }
}
