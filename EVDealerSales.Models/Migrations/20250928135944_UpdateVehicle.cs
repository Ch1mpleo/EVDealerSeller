using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVDealerSales.Models.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BatteryCapacity",
                table: "Vehicles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ChargingTime",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RangeKM",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TopSpeed",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatteryCapacity",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ChargingTime",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "RangeKM",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TopSpeed",
                table: "Vehicles");
        }
    }
}
