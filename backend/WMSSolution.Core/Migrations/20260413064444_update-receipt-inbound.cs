using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updatereceiptinbound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "expected_delivery_date",
                table: "inbound_receipt",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expected_delivery_date",
                table: "inbound_receipt");
        }
    }
}
