using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updatereceiptentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "invoice_number",
                table: "outbound_receipt",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invoice_number",
                table: "inbound_receipt",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invoice_number",
                table: "outbound_receipt");

            migrationBuilder.DropColumn(
                name: "invoice_number",
                table: "inbound_receipt");
        }
    }
}
