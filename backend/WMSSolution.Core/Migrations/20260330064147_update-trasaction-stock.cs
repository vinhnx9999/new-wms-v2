using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updatetrasactionstock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "customer_id",
                table: "stock_transaction",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "customer_name",
                table: "stock_transaction",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "customer_id",
                table: "stock_transaction");

            migrationBuilder.DropColumn(
                name: "customer_name",
                table: "stock_transaction");
        }
    }
}
