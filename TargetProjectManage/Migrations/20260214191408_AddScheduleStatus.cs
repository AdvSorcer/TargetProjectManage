using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TargetProjectManage.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Schedules",
                type: "TEXT",
                nullable: true,
                defaultValue: "未開始");

            // 既有資料預設為「未開始」
            migrationBuilder.Sql("UPDATE Schedules SET Status = '未開始' WHERE Status IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Schedules");
        }
    }
}
