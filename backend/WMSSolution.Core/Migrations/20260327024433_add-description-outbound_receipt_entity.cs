using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class adddescriptionoutbound_receipt_entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "outbound_receipt",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "expected_ship_date",
                table: "outbound_receipt",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_picking_time",
                table: "outbound_receipt",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "wcs_key",
                table: "IntegrationOutbounds",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "wcs_key",
                table: "IntegrationInbounds",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "IntegrationHistories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 2147483647);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "description",
                table: "outbound_receipt");

            migrationBuilder.DropColumn(
                name: "expected_ship_date",
                table: "outbound_receipt");

            migrationBuilder.DropColumn(
                name: "start_picking_time",
                table: "outbound_receipt");

            migrationBuilder.DropColumn(
                name: "wcs_key",
                table: "IntegrationOutbounds");

            migrationBuilder.DropColumn(
                name: "wcs_key",
                table: "IntegrationInbounds");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "IntegrationHistories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 2147483647);
        }
    }
}
