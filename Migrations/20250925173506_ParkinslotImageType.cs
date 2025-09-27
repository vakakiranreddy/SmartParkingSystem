using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParkingSystem.Migrations
{
    /// <inheritdoc />
    public partial class ParkinslotImageType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlotImageUrl",
                table: "ParkingSlots");

            migrationBuilder.AddColumn<byte[]>(
                name: "SlotImage",
                table: "ParkingSlots",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlotImage",
                table: "ParkingSlots");

            migrationBuilder.AddColumn<string>(
                name: "SlotImageUrl",
                table: "ParkingSlots",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
