using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleVelocity.Common.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehicleModelAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Vehicles",
                table: "Vehicles");

            migrationBuilder.RenameTable(
                name: "Vehicles",
                newName: "vehicles");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "vehicles",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Mileage",
                table: "vehicles",
                newName: "mileage");

            migrationBuilder.RenameColumn(
                name: "Vin",
                table: "vehicles",
                newName: "vin");

            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                table: "vehicles",
                newName: "last_updated");

            migrationBuilder.AddColumn<string>(
                name: "ai_audit_notes",
                table: "vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "inspection_notes",
                table: "vehicles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "location_id",
                table: "vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "needs_manual_review",
                table: "vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "priority_level",
                table: "vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "quality_score",
                table: "vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "risk_reason",
                table: "vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_vehicles",
                table: "vehicles",
                column: "vin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_vehicles",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "ai_audit_notes",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "inspection_notes",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "location_id",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "needs_manual_review",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "priority_level",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "quality_score",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "risk_reason",
                table: "vehicles");

            migrationBuilder.RenameTable(
                name: "vehicles",
                newName: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Vehicles",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "mileage",
                table: "Vehicles",
                newName: "Mileage");

            migrationBuilder.RenameColumn(
                name: "vin",
                table: "Vehicles",
                newName: "Vin");

            migrationBuilder.RenameColumn(
                name: "last_updated",
                table: "Vehicles",
                newName: "LastUpdated");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vehicles",
                table: "Vehicles",
                column: "Vin");
        }
    }
}
