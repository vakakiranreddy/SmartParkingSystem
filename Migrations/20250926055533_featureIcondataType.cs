using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParkingSystem.Migrations
{
    /// <inheritdoc />
    public partial class featureIcondataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Features_Name",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "Features");

            migrationBuilder.AddColumn<byte[]>(
                name: "IconData",
                table: "Features",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconData",
                table: "Features");

            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "Features",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Features_Name",
                table: "Features",
                column: "Name",
                unique: true);
        }
    }
}
