using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inyama_Yethu.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "TaskAssignments");

            migrationBuilder.AddColumn<int>(
                name: "TaskCategoryId",
                table: "TaskAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TaskCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_TaskCategoryId",
                table: "TaskAssignments",
                column: "TaskCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_TaskCategories_TaskCategoryId",
                table: "TaskAssignments",
                column: "TaskCategoryId",
                principalTable: "TaskCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_TaskCategories_TaskCategoryId",
                table: "TaskAssignments");

            migrationBuilder.DropTable(
                name: "TaskCategories");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_TaskCategoryId",
                table: "TaskAssignments");

            migrationBuilder.DropColumn(
                name: "TaskCategoryId",
                table: "TaskAssignments");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "TaskAssignments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
