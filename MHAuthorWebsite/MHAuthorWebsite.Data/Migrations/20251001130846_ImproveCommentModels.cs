using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImproveCommentModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.CreateTable(
                name: "ProductComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Primary key"),
                    Rating = table.Column<short>(type: "smallint", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false, comment: "User comment"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Created at"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false, comment: "Foreign key to User"),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Foreign key to Product"),
                    ParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VerifiedPurchase = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, comment: "Soft delete flag")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductComments_ProductComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "ProductComments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductComments_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductCommentsImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Primary key"),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "URL path to the image"),
                    PublicId = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "The publicId in Cloudinary"),
                    AltText = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, comment: "Alternative text for accessibility"),
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Foreign key to Comment")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCommentsImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCommentsImages_ProductComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "ProductComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductCommentsReactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Reaction = table.Column<int>(type: "int", nullable: false, comment: "Reaction type"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Time when the reaction was made")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCommentsReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCommentsReactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductCommentsReactions_ProductComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "ProductComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductComments_ParentCommentId",
                table: "ProductComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComments_ProductId",
                table: "ProductComments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComments_UserId",
                table: "ProductComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCommentsImages_CommentId",
                table: "ProductCommentsImages",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCommentsReactions_CommentId",
                table: "ProductCommentsReactions",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCommentsReactions_UserId",
                table: "ProductCommentsReactions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCommentsImages");

            migrationBuilder.DropTable(
                name: "ProductCommentsReactions");

            migrationBuilder.DropTable(
                name: "ProductComments");

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Primary key"),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Foreign key to Product"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false, comment: "Foreign key to User"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Timestamp"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, comment: "Soft delete flag"),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false, comment: "User comment")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ProductId",
                table: "Comments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");
        }
    }
}
