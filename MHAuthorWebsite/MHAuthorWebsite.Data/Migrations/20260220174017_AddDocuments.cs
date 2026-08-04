using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LegalDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NodesJson = table.Column<string>(type: "nvarchar(max)", maxLength: 500000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdminId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalDocuments_AspNetUsers_AdminId",
                        column: x => x.AdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserLegalAgreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    DocumentVersion = table.Column<int>(type: "int", nullable: false),
                    AgreedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLegalAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLegalAgreements_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserLegalAgreements_LegalDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "LegalDocuments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocuments_AdminId",
                table: "LegalDocuments",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocuments_DocumentType_Version",
                table: "LegalDocuments",
                columns: new[] { "DocumentType", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLegalAgreements_DocumentId",
                table: "UserLegalAgreements",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLegalAgreements_UserId_DocumentType_DocumentVersion",
                table: "UserLegalAgreements",
                columns: new[] { "UserId", "DocumentType", "DocumentVersion" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLegalAgreements");

            migrationBuilder.DropTable(
                name: "LegalDocuments");
        }
    }
}
