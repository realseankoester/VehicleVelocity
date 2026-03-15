using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleVelocity.Common.Migrations
{
    /// <inheritdoc />
    public partial class ForceLowercaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    make = table.Column<string>(type: "text", nullable: true),
                    model = table.Column<string>(type: "text", nullable: true),
                    year = table.Column<int>(type: "integer", nullable: false),
                    mileage = table.Column<int>(type: "integer", nullable: false),
                    inspection_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    location_id = table.Column<string>(type: "text", nullable: true),
                    deployment_phase = table.Column<int>(type: "integer", nullable: false),
                    quality_score = table.Column<int>(type: "integer", nullable: false),
                    priority_level = table.Column<int>(type: "integer", nullable: false),
                    is_high_priority_audit = table.Column<bool>(type: "boolean", nullable: false),
                    audit_recommendation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    shadow_action = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ai_audit_notes = table.Column<string>(type: "text", nullable: true),
                    risk_reason = table.Column<string>(type: "text", nullable: true),
                    is_lead_override = table.Column<bool>(type: "boolean", nullable: false),
                    override_reason = table.Column<string>(type: "text", nullable: true),
                    reviewed_by = table.Column<string>(type: "text", nullable: true),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicles", x => x.vin);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicles");
        }
    }
}
