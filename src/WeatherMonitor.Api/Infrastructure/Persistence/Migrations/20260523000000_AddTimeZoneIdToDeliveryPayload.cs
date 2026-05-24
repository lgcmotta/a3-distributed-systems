using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherMonitor.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeZoneIdToDeliveryPayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TimeZoneId is stored inside the 'payload' jsonb column.
            // No DDL change is required; only the EF Core model snapshot is updated.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No DDL change to revert.
        }
    }
}
