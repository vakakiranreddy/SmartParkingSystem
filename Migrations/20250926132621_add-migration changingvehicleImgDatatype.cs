using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParkingSystem.Migrations
{
    /// <inheritdoc />
    public partial class addmigrationchangingvehicleImgDatatype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleImageUrl",
                table: "Vehicles");

            migrationBuilder.AddColumn<byte[]>(
                name: "VehicleImage",
                table: "Vehicles",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleImage",
                table: "Vehicles");

            migrationBuilder.AddColumn<string>(
                name: "VehicleImageUrl",
                table: "Vehicles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
