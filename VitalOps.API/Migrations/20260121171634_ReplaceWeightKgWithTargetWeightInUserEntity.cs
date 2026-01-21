using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitalOps.API.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceWeightKgWithTargetWeightInUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivityLevel",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "WeightKg",
                table: "Users",
                newName: "TargetWeight");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TargetWeight",
                table: "Users",
                newName: "WeightKg");

            migrationBuilder.AddColumn<int>(
                name: "ActivityLevel",
                table: "Users",
                type: "int",
                nullable: true);
        }
    }
}
