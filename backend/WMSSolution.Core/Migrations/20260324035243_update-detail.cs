using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updatedetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_outbound_receipt_details_outbound_receipt_ReceiptId",
                table: "outbound_receipt_details");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "outbound_receipt_details",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "SkuUomId",
                table: "outbound_receipt_details",
                newName: "sku_uom_id");

            migrationBuilder.RenameColumn(
                name: "SkuId",
                table: "outbound_receipt_details",
                newName: "sku_id");

            migrationBuilder.RenameColumn(
                name: "ReceiptId",
                table: "outbound_receipt_details",
                newName: "receipt_id");

            migrationBuilder.RenameColumn(
                name: "DispatchId",
                table: "outbound_receipt_details",
                newName: "dispatch_id");

            migrationBuilder.RenameColumn(
                name: "CreateDate",
                table: "outbound_receipt_details",
                newName: "create_date");

            migrationBuilder.RenameIndex(
                name: "IX_outbound_receipt_details_ReceiptId",
                table: "outbound_receipt_details",
                newName: "IX_outbound_receipt_details_receipt_id");

            migrationBuilder.AddForeignKey(
                name: "FK_outbound_receipt_details_outbound_receipt_receipt_id",
                table: "outbound_receipt_details",
                column: "receipt_id",
                principalTable: "outbound_receipt",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_outbound_receipt_details_outbound_receipt_receipt_id",
                table: "outbound_receipt_details");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "outbound_receipt_details",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "sku_uom_id",
                table: "outbound_receipt_details",
                newName: "SkuUomId");

            migrationBuilder.RenameColumn(
                name: "sku_id",
                table: "outbound_receipt_details",
                newName: "SkuId");

            migrationBuilder.RenameColumn(
                name: "receipt_id",
                table: "outbound_receipt_details",
                newName: "ReceiptId");

            migrationBuilder.RenameColumn(
                name: "dispatch_id",
                table: "outbound_receipt_details",
                newName: "DispatchId");

            migrationBuilder.RenameColumn(
                name: "create_date",
                table: "outbound_receipt_details",
                newName: "CreateDate");

            migrationBuilder.RenameIndex(
                name: "IX_outbound_receipt_details_receipt_id",
                table: "outbound_receipt_details",
                newName: "IX_outbound_receipt_details_ReceiptId");

            migrationBuilder.AddForeignKey(
                name: "FK_outbound_receipt_details_outbound_receipt_ReceiptId",
                table: "outbound_receipt_details",
                column: "ReceiptId",
                principalTable: "outbound_receipt",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
