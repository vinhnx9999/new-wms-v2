using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updateskuoumlinkentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsBaseUnit",
                table: "sku_uom_link",
                newName: "is_base_unit");

            migrationBuilder.RenameColumn(
                name: "ConversionRate",
                table: "sku_uom_link",
                newName: "conversion_rate");

            migrationBuilder.AddColumn<string>(
                name: "supplier_name",
                table: "stock_transaction",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "supplier_name",
                table: "stock_transaction");

            migrationBuilder.RenameColumn(
                name: "is_base_unit",
                table: "sku_uom_link",
                newName: "IsBaseUnit");

            migrationBuilder.RenameColumn(
                name: "conversion_rate",
                table: "sku_uom_link",
                newName: "ConversionRate");
        }
    }
}
