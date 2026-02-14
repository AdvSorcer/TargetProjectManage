using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TargetProjectManage.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleStageType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StageType",
                table: "Schedules",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StageType",
                table: "Schedules");
        }
    }
}
