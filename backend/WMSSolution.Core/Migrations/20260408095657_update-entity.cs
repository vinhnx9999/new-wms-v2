using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updateentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "outbound_receipt_details",
                type: "numeric",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "creator",
                table: "outbound_receipt",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "consignee",
                table: "outbound_receipt",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deliverer",
                table: "outbound_receipt",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "receipt_date",
                table: "outbound_receipt",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "inbound_receipt_details",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "consignee",
                table: "inbound_receipt",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deliverer",
                table: "inbound_receipt",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "action_name",
                table: "action_log",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "planning_picking",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    picking_id = table.Column<int>(type: "integer", nullable: true),
                    receipt_id = table.Column<int>(type: "integer", nullable: true),
                    warehouse_id = table.Column<int>(type: "integer", nullable: true),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    sku_uom_id = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    goods_location_id = table.Column<int>(type: "integer", nullable: true),
                    pallet_code = table.Column<string>(type: "text", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    gateway_id = table.Column<int>(type: "integer", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planning_picking", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "planning_picking");

            migrationBuilder.DropColumn(
                name: "price",
                table: "outbound_receipt_details");

            migrationBuilder.DropColumn(
                name: "consignee",
                table: "outbound_receipt");

            migrationBuilder.DropColumn(
                name: "deliverer",
                table: "outbound_receipt");

            migrationBuilder.DropColumn(
                name: "receipt_date",
                table: "outbound_receipt");

            migrationBuilder.DropColumn(
                name: "price",
                table: "inbound_receipt_details");

            migrationBuilder.DropColumn(
                name: "consignee",
                table: "inbound_receipt");

            migrationBuilder.DropColumn(
                name: "deliverer",
                table: "inbound_receipt");

            migrationBuilder.DropColumn(
                name: "action_name",
                table: "action_log");

            migrationBuilder.AlterColumn<string>(
                name: "creator",
                table: "outbound_receipt",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
