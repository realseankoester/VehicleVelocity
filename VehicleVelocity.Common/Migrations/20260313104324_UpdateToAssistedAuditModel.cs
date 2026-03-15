using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleVelocity.Common.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToAssistedAuditModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "confidence_score",
                table: "vehicles");

            migrationBuilder.AddColumn<string>(
                name: "audit_recommendation",
                table: "vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_high_priority_audit",
                table: "vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "audit_recommendation",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "is_high_priority_audit",
                table: "vehicles");

            migrationBuilder.AddColumn<double>(
                name: "confidence_score",
                table: "vehicles",
                type: "double precision",
                nullable: true);
        }
    }
}
