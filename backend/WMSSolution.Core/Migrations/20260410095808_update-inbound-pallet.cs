using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updateinboundpallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbound_pallet",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pallet_code = table.Column<string>(type: "text", nullable: false),
                    pallet_rfid = table.Column<string>(type: "text", nullable: true),
                    location_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_mix = table.Column<bool>(type: "boolean", nullable: false),
                    created_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_updated_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbound_pallet", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "InboundPalletDetail",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    sku_uom_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    inbound_pallet_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboundPalletDetail", x => x.id);
                    table.ForeignKey(
                        name: "FK_InboundPalletDetail_inbound_pallet_inbound_pallet_id",
                        column: x => x.inbound_pallet_id,
                        principalTable: "inbound_pallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboundPalletDetail_inbound_pallet_id",
                table: "InboundPalletDetail",
                column: "inbound_pallet_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboundPalletDetail");

            migrationBuilder.DropTable(
                name: "inbound_pallet");
        }
    }
}
