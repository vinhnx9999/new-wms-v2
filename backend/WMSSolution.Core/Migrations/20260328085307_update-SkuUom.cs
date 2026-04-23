using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updateSkuUom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConversionRate",
                table: "sku_uom");

            migrationBuilder.DropColumn(
                name: "IsBaseUnit",
                table: "sku_uom");

            migrationBuilder.DropColumn(
                name: "Operator",
                table: "sku_uom");

            migrationBuilder.AddColumn<int>(
                name: "ConversionRate",
                table: "sku_uom_link",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBaseUnit",
                table: "sku_uom_link",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "sku_uom",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConversionRate",
                table: "sku_uom_link");

            migrationBuilder.DropColumn(
                name: "IsBaseUnit",
                table: "sku_uom_link");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "sku_uom");

            migrationBuilder.AddColumn<int>(
                name: "ConversionRate",
                table: "sku_uom",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBaseUnit",
                table: "sku_uom",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Operator",
                table: "sku_uom",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
