using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMSSolution.Core.Migrations
{
    /// <inheritdoc />
    public partial class updateinbound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "wcs_key",
                table: "IntegrationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "creator",
                table: "inbound_receipt",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "is_stored",
                table: "inbound_receipt",
                type: "boolean",
                nullable: true);


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "wcs_key",
                table: "IntegrationHistories");

            migrationBuilder.DropColumn(
                name: "is_stored",
                table: "inbound_receipt");

            migrationBuilder.AlterColumn<string>(
                name: "creator",
                table: "inbound_receipt",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
