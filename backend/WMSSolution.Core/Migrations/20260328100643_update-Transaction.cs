using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updateTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "current_conversion_rate",
                table: "StockTransactionEntity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "quantity",
                table: "StockTransactionEntity",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ref_receipt",
                table: "StockTransactionEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sku_id",
                table: "StockTransactionEntity",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sku_uom_id",
                table: "StockTransactionEntity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "supplier_id",
                table: "StockTransactionEntity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "transaction_date",
                table: "StockTransactionEntity",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "unit_name",
                table: "StockTransactionEntity",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "current_conversion_rate",
                table: "StockTransactionEntity");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "StockTransactionEntity");

            migrationBuilder.DropColumn(
                name: "ref_receipt",
                table: "StockTransactionEntity");

            migrationBuilder.DropColumn(
                name: "sku_id",
                table: "StockTransactionEntity");

            migrationBuilder.DropColumn(
                name: "sku_uom_id",
                table: "StockTransactionEntity");

            migrationBuilder.DropColumn(
                name: "supplier_id",
                table: "StockTransactionEntity");

            migrationBuilder.DropColumn(
                name: "transaction_date",
                table: "StockTransactionEntity");

            migrationBuilder.DropColumn(
                name: "unit_name",
                table: "StockTransactionEntity");
        }
    }
}
