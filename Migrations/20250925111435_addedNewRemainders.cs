using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParkingSystem.Migrations
{
    /// <inheritdoc />
    public partial class addedNewRemainders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReminderSent",
                table: "ParkingSessions",
                newName: "OverdueReminderSent");

            migrationBuilder.AddColumn<bool>(
                name: "EntryReminderSent",
                table: "ParkingSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExitReminderSent",
                table: "ParkingSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntryReminderSent",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "ExitReminderSent",
                table: "ParkingSessions");

            migrationBuilder.RenameColumn(
                name: "OverdueReminderSent",
                table: "ParkingSessions",
                newName: "ReminderSent");
        }
    }
}
