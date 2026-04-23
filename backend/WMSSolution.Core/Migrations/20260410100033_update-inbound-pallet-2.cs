using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updateinboundpallet2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InboundPalletDetail_inbound_pallet_inbound_pallet_id",
                table: "InboundPalletDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InboundPalletDetail",
                table: "InboundPalletDetail");

            migrationBuilder.RenameTable(
                name: "InboundPalletDetail",
                newName: "inbound_pallet_detail");

            migrationBuilder.RenameIndex(
                name: "IX_InboundPalletDetail_inbound_pallet_id",
                table: "inbound_pallet_detail",
                newName: "IX_inbound_pallet_detail_inbound_pallet_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inbound_pallet_detail",
                table: "inbound_pallet_detail",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_inbound_pallet_detail_inbound_pallet_inbound_pallet_id",
                table: "inbound_pallet_detail",
                column: "inbound_pallet_id",
                principalTable: "inbound_pallet",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_inbound_pallet_detail_inbound_pallet_inbound_pallet_id",
                table: "inbound_pallet_detail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_inbound_pallet_detail",
                table: "inbound_pallet_detail");

            migrationBuilder.RenameTable(
                name: "inbound_pallet_detail",
                newName: "InboundPalletDetail");

            migrationBuilder.RenameIndex(
                name: "IX_inbound_pallet_detail_inbound_pallet_id",
                table: "InboundPalletDetail",
                newName: "IX_InboundPalletDetail_inbound_pallet_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InboundPalletDetail",
                table: "InboundPalletDetail",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_InboundPalletDetail_inbound_pallet_inbound_pallet_id",
                table: "InboundPalletDetail",
                column: "inbound_pallet_id",
                principalTable: "inbound_pallet",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
