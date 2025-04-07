using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inyama_Yethu.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimalSaleRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalSales_Animals_AnimalId1",
                table: "AnimalSales");

            migrationBuilder.DropIndex(
                name: "IX_AnimalSales_AnimalId1",
                table: "AnimalSales");

            migrationBuilder.DropColumn(
                name: "AnimalId1",
                table: "AnimalSales");

            migrationBuilder.AddColumn<int>(
                name: "SaleId",
                table: "Animals",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaleId",
                table: "Animals");

            migrationBuilder.AddColumn<int>(
                name: "AnimalId1",
                table: "AnimalSales",
                type: "int",
                nullable: true);

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
    }
}
