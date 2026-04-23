using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updatesynclog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "priority",
                table: "outbound_receipt",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "trace_id",
                table: "location_sync_conflict",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PickUpDate",
                table: "IntegrationOutbounds",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PickUpDate",
                table: "IntegrationInbounds",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PickUpDate",
                table: "IntegrationHistories",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<int>(
                name: "priority",
                table: "inbound_receipt",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "location_sync_conflict_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trace_id = table.Column<string>(type: "text", nullable: false),
                    warehouse_id = table.Column<int>(type: "integer", nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    action_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    received_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    completed_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    conflict_inserted = table.Column<int>(type: "integer", nullable: false),
                    conflict_updated = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_location_sync_conflict_log", x => x.id);
                });


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "location_sync_conflict_log");

            migrationBuilder.DropColumn(
                name: "priority",
                table: "outbound_receipt");

            migrationBuilder.DropColumn(
                name: "trace_id",
                table: "location_sync_conflict");

            migrationBuilder.DropColumn(
                name: "priority",
                table: "inbound_receipt");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PickUpDate",
                table: "IntegrationOutbounds",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PickUpDate",
                table: "IntegrationInbounds",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PickUpDate",
                table: "IntegrationHistories",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
