using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherMonitor.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "weather_monitor_configuration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    weather_condition = table.Column<string>(type: "text", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    monitor_location_city_code = table.Column<int>(type: "integer", nullable: false),
                    monitor_location_city_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    monitor_location_state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    webhook_settings_access_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    webhook_settings_schedule_for = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    webhook_settings_time_zone_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    webhook_settings_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weather_monitor_configuration", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "webhook_delivery",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_for = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    delivered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    job_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    payload = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_delivery", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_webhook_delivery_status_scheduled_for",
                table: "webhook_delivery",
                columns: new[] { "status", "scheduled_for" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "weather_monitor_configuration");

            migrationBuilder.DropTable(
                name: "webhook_delivery");
        }
    }
}
