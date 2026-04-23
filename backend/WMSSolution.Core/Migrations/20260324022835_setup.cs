using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class setup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "action_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    action_content = table.Column<string>(type: "text", nullable: false),
                    action_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_action_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asnmaster",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    asn_no = table.Column<string>(type: "text", nullable: false),
                    asn_batch = table.Column<string>(type: "text", nullable: false),
                    estimated_arrival_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    asn_status = table.Column<byte>(type: "smallint", nullable: false),
                    weight = table.Column<decimal>(type: "numeric", nullable: false),
                    volume = table.Column<decimal>(type: "numeric", nullable: false),
                    goods_owner_id = table.Column<int>(type: "integer", nullable: false),
                    goods_owner_name = table.Column<string>(type: "text", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    po_id = table.Column<int>(type: "integer", nullable: true),
                    warehouse_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asnmaster", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asnsort",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    asn_id = table.Column<int>(type: "integer", nullable: false),
                    sorted_qty = table.Column<int>(type: "integer", nullable: false),
                    series_number = table.Column<string>(type: "text", nullable: false),
                    putaway_qty = table.Column<int>(type: "integer", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    good_location_id = table.Column<int>(type: "integer", nullable: false),
                    pallet_id = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asnsort", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    table_name = table.Column<string>(type: "text", nullable: false),
                    record_id = table.Column<string>(type: "text", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    old_values = table.Column<string>(type: "jsonb", nullable: true),
                    new_values = table.Column<string>(type: "jsonb", nullable: true),
                    action_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_name = table.Column<string>(type: "text", nullable: false),
                    parent_id = table.Column<int>(type: "integer", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    company_name = table.Column<string>(type: "text", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    manager = table.Column<string>(type: "text", nullable: false),
                    contact_tel = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_name = table.Column<string>(type: "text", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    manager = table.Column<string>(type: "text", nullable: false),
                    contact_tel = table.Column<string>(type: "text", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dispatchlist",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dispatch_no = table.Column<string>(type: "text", nullable: false),
                    dispatch_status = table.Column<byte>(type: "smallint", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    customer_name = table.Column<string>(type: "text", nullable: false),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    qty = table.Column<int>(type: "integer", nullable: false),
                    weight = table.Column<decimal>(type: "numeric", nullable: false),
                    volume = table.Column<decimal>(type: "numeric", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    damage_qty = table.Column<int>(type: "integer", nullable: false),
                    lock_qty = table.Column<int>(type: "integer", nullable: false),
                    picked_qty = table.Column<int>(type: "integer", nullable: false),
                    intrasit_qty = table.Column<int>(type: "integer", nullable: false),
                    package_qty = table.Column<int>(type: "integer", nullable: false),
                    weighing_qty = table.Column<int>(type: "integer", nullable: false),
                    actual_qty = table.Column<int>(type: "integer", nullable: false),
                    sign_qty = table.Column<int>(type: "integer", nullable: false),
                    package_no = table.Column<string>(type: "text", nullable: false),
                    package_person = table.Column<string>(type: "text", nullable: false),
                    package_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    weighing_no = table.Column<string>(type: "text", nullable: false),
                    weighing_person = table.Column<string>(type: "text", nullable: false),
                    weighing_weight = table.Column<decimal>(type: "numeric", nullable: false),
                    waybill_no = table.Column<string>(type: "text", nullable: false),
                    carrier = table.Column<string>(type: "text", nullable: false),
                    freightfee = table.Column<decimal>(type: "numeric", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    pick_checker_id = table.Column<int>(type: "integer", nullable: false),
                    pick_checker = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispatchlist", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flowsetmain",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    menu = table.Column<string>(type: "text", nullable: false),
                    flow_name = table.Column<string>(type: "text", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flowsetmain", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "freightfee",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    carrier = table.Column<string>(type: "text", nullable: false),
                    departure_city = table.Column<string>(type: "text", nullable: false),
                    arrival_city = table.Column<string>(type: "text", nullable: false),
                    price_per_weight = table.Column<decimal>(type: "numeric", nullable: false),
                    price_per_volume = table.Column<decimal>(type: "numeric", nullable: false),
                    min_payment = table.Column<decimal>(type: "numeric", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_freightfee", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "global_unique_serial",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    table_name = table.Column<string>(type: "text", nullable: false),
                    prefix_char = table.Column<string>(type: "text", nullable: false),
                    reset_rule = table.Column<string>(type: "text", nullable: false),
                    current_no = table.Column<int>(type: "integer", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_global_unique_serial", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "goodslocation",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    warehouse_id = table.Column<int>(type: "integer", nullable: false),
                    warehouse_name = table.Column<string>(type: "text", nullable: false),
                    warehouse_area_name = table.Column<string>(type: "text", nullable: false),
                    warehouse_area_property = table.Column<byte>(type: "smallint", nullable: false),
                    location_name = table.Column<string>(type: "text", nullable: false),
                    location_length = table.Column<decimal>(type: "numeric", nullable: false),
                    location_width = table.Column<decimal>(type: "numeric", nullable: false),
                    location_heigth = table.Column<decimal>(type: "numeric", nullable: false),
                    location_volume = table.Column<decimal>(type: "numeric", nullable: false),
                    location_load = table.Column<decimal>(type: "numeric", nullable: false),
                    coordinate_X = table.Column<string>(type: "text", nullable: false),
                    coordinate_Y = table.Column<string>(type: "text", nullable: false),
                    coordinate_Z = table.Column<string>(type: "text", nullable: false),
                    location_status = table.Column<byte>(type: "smallint", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    warehouse_area_id = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    goods_location_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goodslocation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "goodsowner",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    goods_owner_name = table.Column<string>(type: "text", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    manager = table.Column<string>(type: "text", nullable: false),
                    contact_tel = table.Column<string>(type: "text", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goodsowner", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbound_receipt",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receipt_number = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true),
                    warehouse_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ref_receipt = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbound_receipt", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationHistories",
                columns: table => new
                {
                    Description = table.Column<string>(type: "text", nullable: true),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HistoryType = table.Column<int>(type: "integer", nullable: false),
                    TaskCode = table.Column<string>(type: "text", nullable: false),
                    PalletCode = table.Column<string>(type: "text", nullable: false),
                    PickUpDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FinishedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationInbounds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskCode = table.Column<string>(type: "text", nullable: false),
                    PalletCode = table.Column<string>(type: "text", nullable: false),
                    PickUpDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FinishedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationInbounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationOutbounds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GatewayId = table.Column<int>(type: "integer", nullable: false),
                    TaskCode = table.Column<string>(type: "text", nullable: false),
                    PalletCode = table.Column<string>(type: "text", nullable: false),
                    PickUpDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FinishedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationOutbounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationSwapPallets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    PalletCode = table.Column<string>(type: "text", nullable: false),
                    FromLocationId = table.Column<int>(type: "integer", nullable: false),
                    ToLocationId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TaskCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationSwapPallets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "menu",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    menu_name = table.Column<string>(type: "text", nullable: false),
                    module = table.Column<string>(type: "text", nullable: false),
                    vue_path = table.Column<string>(type: "text", nullable: false),
                    vue_path_detail = table.Column<string>(type: "text", nullable: false),
                    vue_directory = table.Column<string>(type: "text", nullable: false),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    menu_actions = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbound_gateway",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gateway_name = table.Column<string>(type: "text", nullable: false),
                    warehouse_id = table.Column<int>(type: "integer", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ref_receipt = table.Column<int>(type: "integer", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbound_gateway", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbound_receipt",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receipt_number = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    warehouse_id = table.Column<int>(type: "integer", nullable: false),
                    outbound_gateway_id = table.Column<int>(type: "integer", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    ref_receipt = table.Column<int>(type: "integer", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbound_receipt", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pallet",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PalletCode = table.Column<string>(type: "text", nullable: false),
                    PalletKey = table.Column<string>(type: "text", nullable: true),
                    PalletStatus = table.Column<int>(type: "integer", nullable: false),
                    IsFull = table.Column<bool>(type: "boolean", nullable: false),
                    IsMixed = table.Column<bool>(type: "boolean", nullable: false),
                    MaxWeight = table.Column<decimal>(type: "numeric", nullable: true),
                    CurrentWeight = table.Column<decimal>(type: "numeric", nullable: true),
                    Length = table.Column<decimal>(type: "numeric", nullable: true),
                    Width = table.Column<decimal>(type: "numeric", nullable: true),
                    Height = table.Column<decimal>(type: "numeric", nullable: true),
                    PalletType = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pallet", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "purchaseorders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    po_no = table.Column<string>(type: "text", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_name = table.Column<string>(type: "text", nullable: false),
                    order_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expected_delivery_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    po_status = table.Column<int>(type: "integer", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchaseorders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "receipt_inbound_detail_integration",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receipt_detail_id = table.Column<int>(type: "integer", nullable: false),
                    inbound_id = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receipt_inbound_detail_integration", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "receipt_outbound_detail_integration",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receipt_detail_id = table.Column<int>(type: "integer", nullable: false),
                    outbound_id = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receipt_outbound_detail_integration", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rolemenu",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userrole_id = table.Column<int>(type: "integer", nullable: false),
                    menu_id = table.Column<int>(type: "integer", nullable: false),
                    authority = table.Column<byte>(type: "smallint", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    menu_actions_authority = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rolemenu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sku_supplier",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sku_supplier", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sku_uom",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitName = table.Column<string>(type: "text", nullable: false),
                    ConversionRate = table.Column<int>(type: "integer", nullable: false),
                    IsBaseUnit = table.Column<bool>(type: "boolean", nullable: false),
                    Operator = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sku_uom", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "specification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    specification_id = table.Column<int>(type: "integer", nullable: false),
                    specification_code = table.Column<string>(type: "text", nullable: false),
                    specification_name = table.Column<string>(type: "text", nullable: false),
                    is_duplicate = table.Column<bool>(type: "boolean", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_delete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_specification", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spu",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    spu_code = table.Column<string>(type: "text", nullable: false),
                    spu_name = table.Column<string>(type: "text", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    spu_description = table.Column<string>(type: "text", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_name = table.Column<string>(type: "text", nullable: false),
                    brand = table.Column<string>(type: "text", nullable: false),
                    origin = table.Column<string>(type: "text", nullable: false),
                    length_unit = table.Column<byte>(type: "smallint", nullable: false),
                    volume_unit = table.Column<byte>(type: "smallint", nullable: false),
                    weight_unit = table.Column<byte>(type: "smallint", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stock",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    goods_location_id = table.Column<int>(type: "integer", nullable: false),
                    qty = table.Column<int>(type: "integer", nullable: false),
                    actual_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    goods_owner_id = table.Column<int>(type: "integer", nullable: false),
                    is_freeze = table.Column<bool>(type: "boolean", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    series_number = table.Column<string>(type: "text", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    putaway_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: true),
                    asn_master_id = table.Column<int>(type: "integer", nullable: true),
                    pallet_code = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stockadjust",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_code = table.Column<string>(type: "text", nullable: false),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    goods_owner_id = table.Column<int>(type: "integer", nullable: false),
                    goods_location_id = table.Column<int>(type: "integer", nullable: false),
                    qty = table.Column<int>(type: "integer", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    is_update_stock = table.Column<bool>(type: "boolean", nullable: false),
                    job_type = table.Column<byte>(type: "smallint", nullable: false),
                    source_table_id = table.Column<int>(type: "integer", nullable: false),
                    series_number = table.Column<string>(type: "text", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    putaway_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stockadjust", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stockfreeze",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_code = table.Column<string>(type: "text", nullable: false),
                    job_type = table.Column<bool>(type: "boolean", nullable: false),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    goods_owner_id = table.Column<int>(type: "integer", nullable: false),
                    goods_location_id = table.Column<int>(type: "integer", nullable: false),
                    handler = table.Column<string>(type: "text", nullable: false),
                    handle_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    series_number = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stockfreeze", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stockmove",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_code = table.Column<string>(type: "text", nullable: false),
                    move_status = table.Column<byte>(type: "smallint", nullable: false),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    orig_goods_location_id = table.Column<int>(type: "integer", nullable: false),
                    dest_googs_location_id = table.Column<int>(type: "integer", nullable: false),
                    qty = table.Column<int>(type: "integer", nullable: false),
                    goods_owner_id = table.Column<int>(type: "integer", nullable: false),
                    handler = table.Column<string>(type: "text", nullable: false),
                    handle_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    series_number = table.Column<string>(type: "text", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    putaway_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stockmove", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stockprocess",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_code = table.Column<string>(type: "text", nullable: false),
                    job_type = table.Column<bool>(type: "boolean", nullable: false),
                    process_status = table.Column<bool>(type: "boolean", nullable: false),
                    processor = table.Column<string>(type: "text", nullable: false),
                    process_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stockprocess", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stocktaking",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_code = table.Column<string>(type: "text", nullable: false),
                    job_status = table.Column<bool>(type: "boolean", nullable: false),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    goods_owner_id = table.Column<int>(type: "integer", nullable: false),
                    goods_location_id = table.Column<int>(type: "integer", nullable: false),
                    series_number = table.Column<string>(type: "text", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    putaway_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    book_qty = table.Column<int>(type: "integer", nullable: false),
                    counted_qty = table.Column<int>(type: "integer", nullable: false),
                    difference_qty = table.Column<int>(type: "integer", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    handler = table.Column<string>(type: "text", nullable: false),
                    handle_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stocktaking", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "supplier",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    supplier_name = table.Column<string>(type: "text", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    manager = table.Column<string>(type: "text", nullable: false),
                    contact_tel = table.Column<string>(type: "text", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_num = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    contact_tel = table.Column<string>(type: "text", nullable: false),
                    user_role = table.Column<string>(type: "text", nullable: false),
                    sex = table.Column<string>(type: "text", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    auth_string = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_defined_print_solution",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vue_path = table.Column<string>(type: "text", nullable: false),
                    tab_page = table.Column<string>(type: "text", nullable: false),
                    solution_name = table.Column<string>(type: "text", nullable: false),
                    config_json = table.Column<string>(type: "text", nullable: false),
                    report_length = table.Column<decimal>(type: "numeric", nullable: false),
                    report_width = table.Column<decimal>(type: "numeric", nullable: false),
                    report_direction = table.Column<string>(type: "text", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_defined_print_solution", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "userrole",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_name = table.Column<string>(type: "text", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userrole", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "warehouse",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    warehouse_name = table.Column<string>(type: "text", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    manager = table.Column<string>(type: "text", nullable: false),
                    contact_tel = table.Column<string>(type: "text", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouse", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "warehousearea",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    area_name = table.Column<string>(type: "text", nullable: false),
                    parent_id = table.Column<int>(type: "integer", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    area_property = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehousearea", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseRuleSettings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    supplier_id = table.Column<int>(type: "integer", nullable: true),
                    block_id = table.Column<int>(type: "integer", nullable: true),
                    floor_id = table.Column<int>(type: "integer", nullable: true),
                    sku_id = table.Column<int>(type: "integer", nullable: true),
                    category_id = table.Column<int>(type: "integer", nullable: true),
                    rule_settings = table.Column<string>(type: "text", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    warehouse_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseRuleSettings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asn",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    asnmaster_id = table.Column<int>(type: "integer", nullable: false),
                    asn_no = table.Column<string>(type: "text", nullable: false),
                    asn_status = table.Column<byte>(type: "smallint", nullable: false),
                    spu_id = table.Column<int>(type: "integer", nullable: false),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    asn_qty = table.Column<int>(type: "integer", nullable: false),
                    actual_qty = table.Column<int>(type: "integer", nullable: false),
                    arrival_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    unload_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    unload_person_id = table.Column<int>(type: "integer", nullable: false),
                    unload_person = table.Column<string>(type: "text", nullable: false),
                    sorted_qty = table.Column<int>(type: "integer", nullable: false),
                    shortage_qty = table.Column<int>(type: "integer", nullable: false),
                    more_qty = table.Column<int>(type: "integer", nullable: false),
                    damage_qty = table.Column<int>(type: "integer", nullable: false),
                    weight = table.Column<decimal>(type: "numeric", nullable: false),
                    volume = table.Column<decimal>(type: "numeric", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_name = table.Column<string>(type: "text", nullable: false),
                    goods_owner_id = table.Column<int>(type: "integer", nullable: false),
                    goods_owner_name = table.Column<string>(type: "text", nullable: false),
                    creator = table.Column<string>(type: "text", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    uom_id = table.Column<int>(type: "integer", nullable: true),
                    batch_number = table.Column<string>(type: "text", nullable: true),
                    asn_qty_decimal = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    actual_qty_decimal = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    goods_location_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asn", x => x.id);
                    table.ForeignKey(
                        name: "FK_asn_asnmaster_asnmaster_id",
                        column: x => x.asnmaster_id,
                        principalTable: "asnmaster",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dispatchlist_detail",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dispatchlist_id = table.Column<int>(type: "integer", nullable: false),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    sku_uom_id = table.Column<int>(type: "integer", nullable: false),
                    req_qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    allocated_qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    picked_qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispatchlist_detail", x => x.id);
                    table.ForeignKey(
                        name: "FK_dispatchlist_detail_dispatchlist_dispatchlist_id",
                        column: x => x.dispatchlist_id,
                        principalTable: "dispatchlist",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flowset",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    flowsetmain_id = table.Column<int>(type: "integer", nullable: false),
                    is_origin = table.Column<bool>(type: "boolean", nullable: false),
                    is_end = table.Column<bool>(type: "boolean", nullable: false),
                    node_guid = table.Column<string>(type: "text", nullable: false),
                    node_name = table.Column<string>(type: "text", nullable: false),
                    prev_node_guid = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flowset", x => x.id);
                    table.ForeignKey(
                        name: "FK_flowset_flowsetmain_flowsetmain_id",
                        column: x => x.flowsetmain_id,
                        principalTable: "flowsetmain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inbound_receipt_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receipt_id = table.Column<int>(type: "integer", nullable: false),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    sku_uom_id = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    goods_location_id = table.Column<int>(type: "integer", nullable: true),
                    pallet_code = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbound_receipt_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_inbound_receipt_details_inbound_receipt_receipt_id",
                        column: x => x.receipt_id,
                        principalTable: "inbound_receipt",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "outbound_receipt_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReceiptId = table.Column<int>(type: "integer", nullable: false),
                    SkuId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    SkuUomId = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    goods_location_id = table.Column<int>(type: "integer", nullable: true),
                    pallet_code = table.Column<string>(type: "text", nullable: true),
                    DispatchId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbound_receipt_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_outbound_receipt_details_outbound_receipt_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "outbound_receipt",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchaseorderdetails",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    sku_name = table.Column<string>(type: "text", nullable: false),
                    spu_id = table.Column<int>(type: "integer", nullable: false),
                    spu_name = table.Column<string>(type: "text", nullable: false),
                    qty_ordered = table.Column<int>(type: "integer", nullable: false),
                    qty_received = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric", nullable: false),
                    po_id = table.Column<int>(type: "integer", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchaseorderdetails", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchaseorderdetails_purchaseorders_po_id",
                        column: x => x.po_id,
                        principalTable: "purchaseorders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sku",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    spu_id = table.Column<int>(type: "integer", nullable: false),
                    sku_code = table.Column<string>(type: "text", nullable: false),
                    sku_name = table.Column<string>(type: "text", nullable: false),
                    bar_code = table.Column<string>(type: "text", nullable: false),
                    weight = table.Column<decimal>(type: "numeric", nullable: false),
                    lenght = table.Column<decimal>(type: "numeric", nullable: false),
                    width = table.Column<decimal>(type: "numeric", nullable: false),
                    height = table.Column<decimal>(type: "numeric", nullable: false),
                    volume = table.Column<decimal>(type: "numeric", nullable: false),
                    unit = table.Column<string>(type: "text", nullable: false),
                    cost = table.Column<decimal>(type: "numeric", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    maxQtyPerPallet = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    is_tracking = table.Column<bool>(type: "boolean", nullable: false),
                    sku_featured = table.Column<string>(type: "text", nullable: false),
                    minimum_value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sku", x => x.id);
                    table.ForeignKey(
                        name: "FK_sku_spu_spu_id",
                        column: x => x.spu_id,
                        principalTable: "spu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stockprocessdetail",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    stock_process_id = table.Column<int>(type: "integer", nullable: false),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    goods_owner_id = table.Column<int>(type: "integer", nullable: false),
                    goods_location_id = table.Column<int>(type: "integer", nullable: false),
                    qty = table.Column<int>(type: "integer", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    is_source = table.Column<bool>(type: "boolean", nullable: false),
                    is_update_stock = table.Column<bool>(type: "boolean", nullable: false),
                    series_number = table.Column<string>(type: "text", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    putaway_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stockprocessdetail", x => x.id);
                    table.ForeignKey(
                        name: "FK_stockprocessdetail_stockprocess_stock_process_id",
                        column: x => x.stock_process_id,
                        principalTable: "stockprocess",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dispatchpicklist",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dispatchlist_id = table.Column<int>(type: "integer", nullable: false),
                    dispatch_detail_id = table.Column<int>(type: "integer", nullable: true),
                    goods_owner_id = table.Column<int>(type: "integer", nullable: false),
                    goods_location_id = table.Column<int>(type: "integer", nullable: false),
                    pallet_Id = table.Column<int>(type: "integer", nullable: true),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    pick_qty = table.Column<int>(type: "integer", nullable: false),
                    picked_qty = table.Column<int>(type: "integer", nullable: false),
                    qty = table.Column<decimal>(type: "numeric", nullable: false),
                    is_update_stock = table.Column<bool>(type: "boolean", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    series_number = table.Column<string>(type: "text", nullable: false),
                    picker_id = table.Column<int>(type: "integer", nullable: false),
                    picker = table.Column<string>(type: "text", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    putaway_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispatchpicklist", x => x.id);
                    table.ForeignKey(
                        name: "FK_dispatchpicklist_dispatchlist_detail_dispatch_detail_id",
                        column: x => x.dispatch_detail_id,
                        principalTable: "dispatchlist_detail",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_dispatchpicklist_dispatchlist_dispatchlist_id",
                        column: x => x.dispatchlist_id,
                        principalTable: "dispatchlist",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flowsetfilter",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    flowset_id = table.Column<int>(type: "integer", nullable: false),
                    flowsetmain_id = table.Column<int>(type: "integer", nullable: false),
                    node_guid = table.Column<string>(type: "text", nullable: false),
                    logic = table.Column<string>(type: "text", nullable: false),
                    c1 = table.Column<string>(type: "text", nullable: false),
                    col_label = table.Column<string>(type: "text", nullable: false),
                    col_name = table.Column<string>(type: "text", nullable: false),
                    compare = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    c2 = table.Column<string>(type: "text", nullable: false),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    condition_group = table.Column<string>(type: "text", nullable: false),
                    formulas = table.Column<string>(type: "text", nullable: false),
                    assert_mode = table.Column<string>(type: "text", nullable: false),
                    table_name = table.Column<string>(type: "text", nullable: false),
                    scheme_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flowsetfilter", x => x.id);
                    table.ForeignKey(
                        name: "FK_flowsetfilter_flowset_flowset_id",
                        column: x => x.flowset_id,
                        principalTable: "flowset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flowsetusers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    flowset_id = table.Column<int>(type: "integer", nullable: false),
                    flowsetmain_id = table.Column<int>(type: "integer", nullable: false),
                    node_guid = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flowsetusers", x => x.id);
                    table.ForeignKey(
                        name: "FK_flowsetusers_flowset_flowset_id",
                        column: x => x.flowset_id,
                        principalTable: "flowset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sku_safety_stock",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    warehouse_id = table.Column<int>(type: "integer", nullable: false),
                    safety_stock_qty = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sku_safety_stock", x => x.id);
                    table.ForeignKey(
                        name: "FK_sku_safety_stock_sku_sku_id",
                        column: x => x.sku_id,
                        principalTable: "sku",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sku_uom_link",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sku_id = table.Column<int>(type: "integer", nullable: false),
                    sku_uom_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sku_uom_link", x => x.id);
                    table.ForeignKey(
                        name: "FK_sku_uom_link_sku_sku_id",
                        column: x => x.sku_id,
                        principalTable: "sku",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sku_uom_link_sku_uom_sku_uom_id",
                        column: x => x.sku_uom_id,
                        principalTable: "sku_uom",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_asn_asnmaster_id",
                table: "asn",
                column: "asnmaster_id");

            migrationBuilder.CreateIndex(
                name: "IX_dispatchlist_detail_dispatchlist_id",
                table: "dispatchlist_detail",
                column: "dispatchlist_id");

            migrationBuilder.CreateIndex(
                name: "IX_dispatchpicklist_dispatch_detail_id",
                table: "dispatchpicklist",
                column: "dispatch_detail_id");

            migrationBuilder.CreateIndex(
                name: "IX_dispatchpicklist_dispatchlist_id",
                table: "dispatchpicklist",
                column: "dispatchlist_id");

            migrationBuilder.CreateIndex(
                name: "IX_flowset_flowsetmain_id",
                table: "flowset",
                column: "flowsetmain_id");

            migrationBuilder.CreateIndex(
                name: "IX_flowsetfilter_flowset_id",
                table: "flowsetfilter",
                column: "flowset_id");

            migrationBuilder.CreateIndex(
                name: "IX_flowsetusers_flowset_id",
                table: "flowsetusers",
                column: "flowset_id");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_receipt_details_receipt_id",
                table: "inbound_receipt_details",
                column: "receipt_id");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_receipt_details_ReceiptId",
                table: "outbound_receipt_details",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_purchaseorderdetails_po_id",
                table: "purchaseorderdetails",
                column: "po_id");

            migrationBuilder.CreateIndex(
                name: "IX_sku_spu_id",
                table: "sku",
                column: "spu_id");

            migrationBuilder.CreateIndex(
                name: "IX_sku_safety_stock_sku_id",
                table: "sku_safety_stock",
                column: "sku_id");

            migrationBuilder.CreateIndex(
                name: "IX_sku_uom_link_sku_id",
                table: "sku_uom_link",
                column: "sku_id");

            migrationBuilder.CreateIndex(
                name: "IX_sku_uom_link_sku_uom_id",
                table: "sku_uom_link",
                column: "sku_uom_id");

            migrationBuilder.CreateIndex(
                name: "IX_stockprocessdetail_stock_process_id",
                table: "stockprocessdetail",
                column: "stock_process_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "action_log");

            migrationBuilder.DropTable(
                name: "asn");

            migrationBuilder.DropTable(
                name: "asnsort");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "company");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "dispatchpicklist");

            migrationBuilder.DropTable(
                name: "flowsetfilter");

            migrationBuilder.DropTable(
                name: "flowsetusers");

            migrationBuilder.DropTable(
                name: "freightfee");

            migrationBuilder.DropTable(
                name: "global_unique_serial");

            migrationBuilder.DropTable(
                name: "goodslocation");

            migrationBuilder.DropTable(
                name: "goodsowner");

            migrationBuilder.DropTable(
                name: "inbound_receipt_details");

            migrationBuilder.DropTable(
                name: "IntegrationHistories");

            migrationBuilder.DropTable(
                name: "IntegrationInbounds");

            migrationBuilder.DropTable(
                name: "IntegrationOutbounds");

            migrationBuilder.DropTable(
                name: "IntegrationSwapPallets");

            migrationBuilder.DropTable(
                name: "menu");

            migrationBuilder.DropTable(
                name: "outbound_gateway");

            migrationBuilder.DropTable(
                name: "outbound_receipt_details");

            migrationBuilder.DropTable(
                name: "pallet");

            migrationBuilder.DropTable(
                name: "purchaseorderdetails");

            migrationBuilder.DropTable(
                name: "receipt_inbound_detail_integration");

            migrationBuilder.DropTable(
                name: "receipt_outbound_detail_integration");

            migrationBuilder.DropTable(
                name: "rolemenu");

            migrationBuilder.DropTable(
                name: "sku_safety_stock");

            migrationBuilder.DropTable(
                name: "sku_supplier");

            migrationBuilder.DropTable(
                name: "sku_uom_link");

            migrationBuilder.DropTable(
                name: "specification");

            migrationBuilder.DropTable(
                name: "stock");

            migrationBuilder.DropTable(
                name: "stockadjust");

            migrationBuilder.DropTable(
                name: "stockfreeze");

            migrationBuilder.DropTable(
                name: "stockmove");

            migrationBuilder.DropTable(
                name: "stockprocessdetail");

            migrationBuilder.DropTable(
                name: "stocktaking");

            migrationBuilder.DropTable(
                name: "supplier");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "user_defined_print_solution");

            migrationBuilder.DropTable(
                name: "userrole");

            migrationBuilder.DropTable(
                name: "warehouse");

            migrationBuilder.DropTable(
                name: "warehousearea");

            migrationBuilder.DropTable(
                name: "WarehouseRuleSettings");

            migrationBuilder.DropTable(
                name: "asnmaster");

            migrationBuilder.DropTable(
                name: "dispatchlist_detail");

            migrationBuilder.DropTable(
                name: "flowset");

            migrationBuilder.DropTable(
                name: "inbound_receipt");

            migrationBuilder.DropTable(
                name: "outbound_receipt");

            migrationBuilder.DropTable(
                name: "purchaseorders");

            migrationBuilder.DropTable(
                name: "sku");

            migrationBuilder.DropTable(
                name: "sku_uom");

            migrationBuilder.DropTable(
                name: "stockprocess");

            migrationBuilder.DropTable(
                name: "dispatchlist");

            migrationBuilder.DropTable(
                name: "flowsetmain");

            migrationBuilder.DropTable(
                name: "spu");
        }
    }
}
