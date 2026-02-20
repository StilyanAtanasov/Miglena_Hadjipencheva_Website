using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImproveAnnouncements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAcceptedPrivacyPolicy",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMarketingSubscribed",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarketingSubscribedOn",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarketingUnsubscribeToken",
                table: "AspNetUsers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarketingUnsubscribeTokenCreatedOn",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarketingUnsubscribedOn",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrivacyPolicyAcceptedOn",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyPolicyVersion",
                table: "AspNetUsers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AnnouncementEmailDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnnouncementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RecipientSource = table.Column<int>(type: "int", nullable: false),
                    DeliveryStatus = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DeliveredOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementEmailDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnnouncementEmailDeliveries_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnnouncementEmailDeliveries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementEmailDeliveries_AnnouncementId_Email",
                table: "AnnouncementEmailDeliveries",
                columns: new[] { "AnnouncementId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementEmailDeliveries_UserId",
                table: "AnnouncementEmailDeliveries",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnnouncementEmailDeliveries");

            migrationBuilder.DropColumn(
                name: "HasAcceptedPrivacyPolicy",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsMarketingSubscribed",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MarketingSubscribedOn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MarketingUnsubscribeToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MarketingUnsubscribeTokenCreatedOn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MarketingUnsubscribedOn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicyAcceptedOn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicyVersion",
                table: "AspNetUsers");
        }
    }
}
