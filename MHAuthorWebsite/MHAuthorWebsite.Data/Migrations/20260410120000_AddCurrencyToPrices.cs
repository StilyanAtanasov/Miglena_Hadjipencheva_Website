using MHAuthorWebsite.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260410120000_AddCurrencyToPrices")]
    public partial class AddCurrencyToPrices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Products",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "BGN");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "ProductDiscounts",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "BGN");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "CartItems",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "BGN");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "OrdersProducts",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "BGN");

            migrationBuilder.Sql(@"
                UPDATE Products
                SET Price = ROUND(Price / 1.95583, 2),
                    Currency = 'EUR'
                WHERE Currency = 'BGN';
            ");

            migrationBuilder.Sql(@"
                UPDATE ProductDiscounts
                SET NewPrice = ROUND(NewPrice / 1.95583, 3),
                    Currency = 'EUR'
                WHERE Currency = 'BGN';
            ");

            migrationBuilder.Sql(@"
                UPDATE CartItems
                SET Price = ROUND(Price / 1.95583, 2),
                    Currency = 'EUR'
                WHERE Currency = 'BGN';
            ");

            migrationBuilder.Sql(@"
                UPDATE OrdersProducts
                SET Currency = 'BGN'
                WHERE Currency IS NULL OR Currency = '';
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Products
                SET Price = ROUND(Price * 1.95583, 2),
                    Currency = 'BGN'
                WHERE Currency = 'EUR';
            ");

            migrationBuilder.Sql(@"
                UPDATE ProductDiscounts
                SET NewPrice = ROUND(NewPrice * 1.95583, 3),
                    Currency = 'BGN'
                WHERE Currency = 'EUR';
            ");

            migrationBuilder.Sql(@"
                UPDATE CartItems
                SET Price = ROUND(Price * 1.95583, 2),
                    Currency = 'BGN'
                WHERE Currency = 'EUR';
            ");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "ProductDiscounts");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "OrdersProducts");
        }
    }
}
