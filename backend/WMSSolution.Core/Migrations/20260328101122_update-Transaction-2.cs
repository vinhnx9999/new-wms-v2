using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updateTransaction2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StockTransactionEntity",
                table: "StockTransactionEntity");

            migrationBuilder.RenameTable(
                name: "StockTransactionEntity",
                newName: "stock_transaction");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stock_transaction",
                table: "stock_transaction",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_stock_transaction",
                table: "stock_transaction");

            migrationBuilder.RenameTable(
                name: "stock_transaction",
                newName: "StockTransactionEntity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockTransactionEntity",
                table: "StockTransactionEntity",
                column: "id");
        }
    }
}
