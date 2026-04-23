using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeginMerchandises",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: true),
                    SkuId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    ExpireDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UomId = table.Column<int>(type: "integer", nullable: true),
                    LocationId = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Creator = table.Column<string>(type: "text", nullable: false),
                    IsPutaway = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginMerchandises", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "location_sync_conflict",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    warehouse_id = table.Column<int>(type: "integer", nullable: false),
                    location_id = table.Column<int>(type: "integer", nullable: false),
                    location_name = table.Column<string>(type: "text", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    wcs_status = table.Column<byte>(type: "smallint", nullable: false),
                    wms_has_pallet = table.Column<bool>(type: "boolean", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    created_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    resolved_by = table.Column<string>(type: "text", nullable: true),
                    resolved_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    resolution_note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_location_sync_conflict", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeginMerchandises");

            migrationBuilder.DropTable(
                name: "location_sync_conflict");
        }
    }
}
