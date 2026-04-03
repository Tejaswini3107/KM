using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KacharaManagement.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddTruckMovementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TruckLatitude",
                table: "SensorHistories",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TruckLocation",
                table: "SensorHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TruckLongitude",
                table: "SensorHistories",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TruckMoving",
                table: "SensorHistories",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TruckReached",
                table: "SensorHistories",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TruckStarted",
                table: "SensorHistories",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TruckState",
                table: "SensorHistories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TruckLatitude",
                table: "SensorHistories");

            migrationBuilder.DropColumn(
                name: "TruckLocation",
                table: "SensorHistories");

            migrationBuilder.DropColumn(
                name: "TruckLongitude",
                table: "SensorHistories");

            migrationBuilder.DropColumn(
                name: "TruckMoving",
                table: "SensorHistories");

            migrationBuilder.DropColumn(
                name: "TruckReached",
                table: "SensorHistories");

            migrationBuilder.DropColumn(
                name: "TruckStarted",
                table: "SensorHistories");

            migrationBuilder.DropColumn(
                name: "TruckState",
                table: "SensorHistories");
        }
    }
}
