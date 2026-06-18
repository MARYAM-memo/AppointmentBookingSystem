using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_appointment_date_service",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_AppointmentDate_StartTime_EndTime",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_CreatedAt",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_NoDoubleBooking",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Localization_TimeZone",
                table: "BusinessProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_Date_Created",
                table: "Appointments",
                columns: new[] { "AppointmentDate", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Service_Date",
                table: "Appointments",
                columns: new[] { "ServiceId", "AppointmentDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Date_Created",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Service_Date",
                table: "Appointments");

            migrationBuilder.AddColumn<string>(
                name: "Localization_TimeZone",
                table: "BusinessProfiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_date_service",
                table: "Appointments",
                columns: new[] { "AppointmentDate", "ServiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentDate_StartTime_EndTime",
                table: "Appointments",
                columns: new[] { "AppointmentDate", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CreatedAt",
                table: "Appointments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_NoDoubleBooking",
                table: "Appointments",
                columns: new[] { "ServiceId", "AppointmentDate", "StartTime" },
                unique: true);
        }
    }
}
