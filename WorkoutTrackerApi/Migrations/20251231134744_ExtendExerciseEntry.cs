using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class ExtendExerciseEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CardioType",
                table: "ExerciseEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntervalsCount",
                table: "ExerciseEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxHeartRate",
                table: "ExerciseEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PaceMinPerKm",
                table: "ExerciseEntries",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "RestIntervalSec",
                table: "ExerciseEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkIntervalSec",
                table: "ExerciseEntries",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardioType",
                table: "ExerciseEntries");

            migrationBuilder.DropColumn(
                name: "IntervalsCount",
                table: "ExerciseEntries");

            migrationBuilder.DropColumn(
                name: "MaxHeartRate",
                table: "ExerciseEntries");

            migrationBuilder.DropColumn(
                name: "PaceMinPerKm",
                table: "ExerciseEntries");

            migrationBuilder.DropColumn(
                name: "RestIntervalSec",
                table: "ExerciseEntries");

            migrationBuilder.DropColumn(
                name: "WorkIntervalSec",
                table: "ExerciseEntries");
        }
    }
}
