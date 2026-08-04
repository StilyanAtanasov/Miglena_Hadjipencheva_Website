using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MHAuthorWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShipmentNumber",
                table: "Shipments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                comment: "Unique identifier for the shipment",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AwbUrl",
                table: "Shipments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                comment: "The URL for the air waybill document",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true,
                oldComment: "URL to the Air Waybill (AWB) document");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldComment: "Name of product type");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ProductTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Primary key")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageOriginalId",
                table: "ProductsThumbnails",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Foreign key to the original image");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "ProductsThumbnails",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Foreign key to Product");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                table: "ProductsThumbnails",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Foreign key to ProductImage");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "ProductsImages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Foreign key to Product");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ProductsImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldComment: "URL path to the image");

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "ProductsImages",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldComment: "Alternative text for accessibility");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ProductsImages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Primary key");

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "Products",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldComment: "Product's weight");

            migrationBuilder.AlterColumn<int>(
                name: "StockQuantity",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Quantity in stock");

            migrationBuilder.AlterColumn<int>(
                name: "ProductTypeId",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Foreign key to ProductType");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "Unit price");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldComment: "Product name");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Determines if the product should be visible for basic users");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(max)",
                maxLength: 20000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 20000,
                oldComment: "Product description");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Primary key");

            migrationBuilder.AlterColumn<int>(
                name: "Reaction",
                table: "ProductCommentsReactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Reaction type");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductCommentsReactions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComment: "Time when the reaction was made");

            migrationBuilder.AlterColumn<string>(
                name: "PreviewUrl",
                table: "ProductCommentsImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldComment: "URL path to the image preview");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ProductCommentsImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldComment: "URL path to the image");

            migrationBuilder.AlterColumn<Guid>(
                name: "CommentId",
                table: "ProductCommentsImages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Foreign key to Comment");

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "ProductCommentsImages",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldComment: "Alternative text for accessibility");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ProductCommentsImages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Primary key");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ProductComments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldComment: "Foreign key to User");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "ProductComments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldComment: "User comment");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "ProductComments",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Foreign key to Product");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastEdited",
                table: "ProductComments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Last edited at");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ProductComments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "ProductComments",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComment: "Created at");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ProductComments",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Primary key");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "ProductAttributes",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true,
                oldComment: "Value of the attribute");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "ProductAttributes",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Foreign key to Product");

            migrationBuilder.AlterColumn<int>(
                name: "ProductAttributeOptionsId",
                table: "ProductAttributes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldComment: "Foreign key to Product Attribute Options");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "ProductAttributes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldComment: "Name of the attribute (e.g., ISBN)");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayPosition",
                table: "ProductAttributes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0,
                oldComment: "Defines in which section of the product's details page will it appear");

            migrationBuilder.AlterColumn<int>(
                name: "AttributeDefinitionId",
                table: "ProductAttributes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Foreign key to Product Attribute Definition");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ProductAttributes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Primary key")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "AttributeDefinitionId",
                table: "ProductAttributeOptions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "FK to attribute definition this value belongs to");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ProductAttributeOptions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Primary key")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "ProductTypeId",
                table: "ProductAttributeDefinitions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Foreign key to ProductType");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "ProductAttributeDefinitions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldComment: "Attribute key (e.g., ISBN)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "ProductAttributeDefinitions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "Indicates if the attribute is mandatory");

            migrationBuilder.AlterColumn<bool>(
                name: "HasPredefinedValue",
                table: "ProductAttributeDefinitions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "Indicates if the attribute can choose from options");

            migrationBuilder.AlterColumn<int>(
                name: "DataType",
                table: "ProductAttributeDefinitions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "UI data type (e.g., number, date)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ProductAttributeDefinitions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Primary key")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldComment: "Foreign key to User");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComment: "Order timestamp");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Primary key");

            migrationBuilder.CreateTable(
                name: "ScheduledNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    NotificationStatus = table.Column<int>(type: "int", nullable: false),
                    NotificationTemplate = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledNotifications_AspNetUsers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledNotifications_RecipientId",
                table: "ScheduledNotifications",
                column: "RecipientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledNotifications");

            migrationBuilder.AlterColumn<string>(
                name: "ShipmentNumber",
                table: "Shipments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "Unique identifier for the shipment");

            migrationBuilder.AlterColumn<string>(
                name: "AwbUrl",
                table: "Shipments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                comment: "URL to the Air Waybill (AWB) document",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true,
                oldComment: "The URL for the air waybill document");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                comment: "Name of product type",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ProductTypes",
                type: "int",
                nullable: false,
                comment: "Primary key",
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageOriginalId",
                table: "ProductsThumbnails",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Foreign key to the original image",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "ProductsThumbnails",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Foreign key to Product",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                table: "ProductsThumbnails",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Foreign key to ProductImage",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "ProductsImages",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Foreign key to Product",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ProductsImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                comment: "URL path to the image",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "ProductsImages",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                comment: "Alternative text for accessibility",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ProductsImages",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Primary key",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "Products",
                type: "decimal(18,3)",
                nullable: false,
                comment: "Product's weight",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<int>(
                name: "StockQuantity",
                table: "Products",
                type: "int",
                nullable: false,
                comment: "Quantity in stock",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ProductTypeId",
                table: "Products",
                type: "int",
                nullable: false,
                comment: "Foreign key to ProductType",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                comment: "Unit price",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                comment: "Product name",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Determines if the product should be visible for basic users",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(max)",
                maxLength: 20000,
                nullable: false,
                comment: "Product description",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 20000);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Primary key",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "Reaction",
                table: "ProductCommentsReactions",
                type: "int",
                nullable: false,
                comment: "Reaction type",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductCommentsReactions",
                type: "datetime2",
                nullable: false,
                comment: "Time when the reaction was made",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "PreviewUrl",
                table: "ProductCommentsImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                comment: "URL path to the image preview",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ProductCommentsImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                comment: "URL path to the image",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<Guid>(
                name: "CommentId",
                table: "ProductCommentsImages",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Foreign key to Comment",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "ProductCommentsImages",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                comment: "Alternative text for accessibility",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ProductCommentsImages",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Primary key",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ProductComments",
                type: "nvarchar(450)",
                nullable: false,
                comment: "Foreign key to User",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "ProductComments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                comment: "User comment",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "ProductComments",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Foreign key to Product",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastEdited",
                table: "ProductComments",
                type: "datetime2",
                nullable: true,
                comment: "Last edited at",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ProductComments",
                type: "bit",
                nullable: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "ProductComments",
                type: "datetime2",
                nullable: false,
                comment: "Created at",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ProductComments",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Primary key",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "ProductAttributes",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "Value of the attribute",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "ProductAttributes",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Foreign key to Product",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "ProductAttributeOptionsId",
                table: "ProductAttributes",
                type: "int",
                nullable: true,
                comment: "Foreign key to Product Attribute Options",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "ProductAttributes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                comment: "Name of the attribute (e.g., ISBN)",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayPosition",
                table: "ProductAttributes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Defines in which section of the product's details page will it appear",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "AttributeDefinitionId",
                table: "ProductAttributes",
                type: "int",
                nullable: false,
                comment: "Foreign key to Product Attribute Definition",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ProductAttributes",
                type: "int",
                nullable: false,
                comment: "Primary key",
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "AttributeDefinitionId",
                table: "ProductAttributeOptions",
                type: "int",
                nullable: false,
                comment: "FK to attribute definition this value belongs to",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ProductAttributeOptions",
                type: "int",
                nullable: false,
                comment: "Primary key",
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "ProductTypeId",
                table: "ProductAttributeDefinitions",
                type: "int",
                nullable: false,
                comment: "Foreign key to ProductType",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "ProductAttributeDefinitions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                comment: "Attribute key (e.g., ISBN)",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "ProductAttributeDefinitions",
                type: "bit",
                nullable: false,
                comment: "Indicates if the attribute is mandatory",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "HasPredefinedValue",
                table: "ProductAttributeDefinitions",
                type: "bit",
                nullable: false,
                comment: "Indicates if the attribute can choose from options",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DataType",
                table: "ProductAttributeDefinitions",
                type: "int",
                nullable: false,
                comment: "UI data type (e.g., number, date)",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ProductAttributeDefinitions",
                type: "int",
                nullable: false,
                comment: "Primary key",
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                comment: "Foreign key to User",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                comment: "Order timestamp",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Primary key",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
