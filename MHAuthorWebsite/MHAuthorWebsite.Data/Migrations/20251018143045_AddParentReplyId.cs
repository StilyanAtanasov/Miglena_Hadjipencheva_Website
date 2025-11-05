using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParentReplyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductComments_ProductComments_ParentCommentId",
                table: "ProductComments");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentReplyId",
                table: "ProductComments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductComments_ParentReplyId",
                table: "ProductComments",
                column: "ParentReplyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductComments_ProductComments_ParentCommentId",
                table: "ProductComments",
                column: "ParentCommentId",
                principalTable: "ProductComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductComments_ProductComments_ParentReplyId",
                table: "ProductComments",
                column: "ParentReplyId",
                principalTable: "ProductComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductComments_ProductComments_ParentCommentId",
                table: "ProductComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductComments_ProductComments_ParentReplyId",
                table: "ProductComments");

            migrationBuilder.DropIndex(
                name: "IX_ProductComments_ParentReplyId",
                table: "ProductComments");

            migrationBuilder.DropColumn(
                name: "ParentReplyId",
                table: "ProductComments");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductComments_ProductComments_ParentCommentId",
                table: "ProductComments",
                column: "ParentCommentId",
                principalTable: "ProductComments",
                principalColumn: "Id");
        }
    }
}
