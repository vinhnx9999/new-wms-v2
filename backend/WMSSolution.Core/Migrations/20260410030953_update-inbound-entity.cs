using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updateinboundentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "source_number",
                table: "outbound_receipt_details",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_number",
                table: "inbound_receipt_details",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source_number",
                table: "outbound_receipt_details");

            migrationBuilder.DropColumn(
                name: "source_number",
                table: "inbound_receipt_details");
        }
    }
}
