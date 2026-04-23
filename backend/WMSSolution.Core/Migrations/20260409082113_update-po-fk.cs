using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updatepofk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchaseorderdetails_purchaseorders_po_id1",
                table: "purchaseorderdetails");

            migrationBuilder.DropIndex(
                name: "IX_purchaseorderdetails_po_id1",
                table: "purchaseorderdetails");

            migrationBuilder.DropColumn(
                name: "po_id1",
                table: "purchaseorderdetails");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchaseorderdetails_purchaseorders_po_id",
                table: "purchaseorderdetails");

            migrationBuilder.DropIndex(
                name: "IX_purchaseorderdetails_po_id",
                table: "purchaseorderdetails");

            migrationBuilder.AddColumn<int>(
                name: "po_id1",
                table: "purchaseorderdetails",
                type: "integer",
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
    }
}
