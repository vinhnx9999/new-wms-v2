using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updatepo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchaseorderdetails_purchaseorders_po_id",
                table: "purchaseorderdetails");

            migrationBuilder.DropIndex(
                name: "IX_purchaseorderdetails_po_id",
                table: "purchaseorderdetails");

            migrationBuilder.AlterColumn<string>(
                name: "supplier_name",
                table: "purchaseorders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "supplier_id",
                table: "purchaseorders",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "expected_delivery_date",
                table: "purchaseorders",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<string>(
                name: "buyer_address",
                table: "purchaseorders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "buyer_name",
                table: "purchaseorders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "create_time",
                table: "purchaseorders",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "purchaseorders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_multi_supplier",
                table: "purchaseorders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "payment_term",
                table: "purchaseorders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "shipping_amount",
                table: "purchaseorders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_amount",
                table: "purchaseorders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "spu_name",
                table: "purchaseorderdetails",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "sku_name",
                table: "purchaseorderdetails",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "expiry_date",
                table: "purchaseorderdetails",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<int>(
                name: "po_id1",
                table: "purchaseorderdetails",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sku_uom_id",
                table: "purchaseorderdetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "supplier_id",
                table: "purchaseorderdetails",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "supplier_name",
                table: "purchaseorderdetails",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchaseorderdetails_po_id1",
                table: "purchaseorderdetails",
                column: "po_id1");

            migrationBuilder.AddForeignKey(
                name: "FK_purchaseorderdetails_purchaseorders_po_id1",
                table: "purchaseorderdetails",
                column: "po_id1",
                principalTable: "purchaseorders",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchaseorderdetails_purchaseorders_po_id1",
                table: "purchaseorderdetails");

            migrationBuilder.DropIndex(
                name: "IX_purchaseorderdetails_po_id1",
                table: "purchaseorderdetails");

            migrationBuilder.DropColumn(
                name: "buyer_address",
                table: "purchaseorders");

            migrationBuilder.DropColumn(
                name: "buyer_name",
                table: "purchaseorders");

            migrationBuilder.DropColumn(
                name: "create_time",
                table: "purchaseorders");

            migrationBuilder.DropColumn(
                name: "description",
                table: "purchaseorders");

            migrationBuilder.DropColumn(
                name: "is_multi_supplier",
                table: "purchaseorders");

            migrationBuilder.DropColumn(
                name: "payment_term",
                table: "purchaseorders");

            migrationBuilder.DropColumn(
                name: "shipping_amount",
                table: "purchaseorders");

            migrationBuilder.DropColumn(
                name: "total_amount",
                table: "purchaseorders");

            migrationBuilder.DropColumn(
                name: "po_id1",
                table: "purchaseorderdetails");

            migrationBuilder.DropColumn(
                name: "sku_uom_id",
                table: "purchaseorderdetails");

            migrationBuilder.DropColumn(
                name: "supplier_id",
                table: "purchaseorderdetails");

            migrationBuilder.DropColumn(
                name: "supplier_name",
                table: "purchaseorderdetails");

            migrationBuilder.AlterColumn<string>(
                name: "supplier_name",
                table: "purchaseorders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "supplier_id",
                table: "purchaseorders",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "expected_delivery_date",
                table: "purchaseorders",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "spu_name",
                table: "purchaseorderdetails",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "sku_name",
                table: "purchaseorderdetails",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "expiry_date",
                table: "purchaseorderdetails",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchaseorderdetails_po_id",
                table: "purchaseorderdetails",
                column: "po_id");

            migrationBuilder.AddForeignKey(
                name: "FK_purchaseorderdetails_purchaseorders_po_id",
                table: "purchaseorderdetails",
                column: "po_id",
                principalTable: "purchaseorders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
