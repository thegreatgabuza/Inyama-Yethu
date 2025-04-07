using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inyama_Yethu.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnimalSales_AnimalId",
                table: "AnimalSales");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNumber",
                table: "AnimalSales",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "AnimalId1",
                table: "AnimalSales",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnimalSales_AnimalId",
                table: "AnimalSales",
                column: "AnimalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnimalSales_AnimalId1",
                table: "AnimalSales",
                column: "AnimalId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalSales_Animals_AnimalId1",
                table: "AnimalSales",
                column: "AnimalId1",
                principalTable: "Animals",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalSales_Animals_AnimalId1",
                table: "AnimalSales");

            migrationBuilder.DropIndex(
                name: "IX_AnimalSales_AnimalId",
                table: "AnimalSales");

            migrationBuilder.DropIndex(
                name: "IX_AnimalSales_AnimalId1",
                table: "AnimalSales");

            migrationBuilder.DropColumn(
                name: "AnimalId1",
                table: "AnimalSales");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNumber",
                table: "AnimalSales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_AnimalSales_AnimalId",
                table: "AnimalSales",
                column: "AnimalId");
        }
    }
}
