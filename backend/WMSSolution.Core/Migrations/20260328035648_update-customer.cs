using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updatecustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AddColumn<string>(
                name: "customer_code",
                table: "customer",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_number",
                table: "customer",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "customer_code",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "tax_number",
                table: "customer");

        }
    }
}
