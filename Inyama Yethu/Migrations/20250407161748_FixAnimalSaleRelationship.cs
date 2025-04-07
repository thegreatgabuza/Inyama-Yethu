using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inyama_Yethu.Migrations
{
    /// <inheritdoc />
    public partial class FixAnimalSaleRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaleId",
                table: "Animals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SaleId",
                table: "Animals",
                type: "int",
                nullable: true);
        }
    }
}
