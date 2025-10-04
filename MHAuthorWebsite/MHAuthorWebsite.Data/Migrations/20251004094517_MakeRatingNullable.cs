using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeRatingNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "Rating",
                table: "ProductComments",
                type: "smallint",
                nullable: true,
                comment: "User rating, but null since replies do not share rating!",
                oldClrType: typeof(short),
                oldType: "smallint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "Rating",
                table: "ProductComments",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true,
                oldComment: "User rating, but null since replies do not share rating!");
        }
    }
}
