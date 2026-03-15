using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleVelocity.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndLocationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "confidence_score",
                table: "vehicles",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "deployment_phase",
                table: "vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_lead_override",
                table: "vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "make",
                table: "vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "model",
                table: "vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "override_reason",
                table: "vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reviewed_by",
                table: "vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "shadow_action",
                table: "vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "year",
                table: "vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "confidence_score",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "deployment_phase",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "is_lead_override",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "make",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "model",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "override_reason",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "reviewed_by",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "shadow_action",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "year",
                table: "vehicles");
        }
    }
}
