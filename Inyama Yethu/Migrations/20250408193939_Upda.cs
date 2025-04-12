using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inyama_Yethu.Migrations
{
    /// <inheritdoc />
    public partial class Upda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Births_Animals_AnimalId",
                table: "Births");

            migrationBuilder.RenameColumn(
                name: "NumberOfOffspring",
                table: "Births",
                newName: "MotherAnimalId");

            migrationBuilder.RenameColumn(
                name: "NumberAlive",
                table: "Births",
                newName: "LiveBorn");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Births",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "AnimalId",
                table: "Births",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "FatherAnimalId",
                table: "Births",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LitterSize",
                table: "Births",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 8, 21, 39, 35, 232, DateTimeKind.Local).AddTicks(2606));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 8, 21, 39, 35, 232, DateTimeKind.Local).AddTicks(2642));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 8, 21, 39, 35, 232, DateTimeKind.Local).AddTicks(2646));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 8, 21, 39, 35, 232, DateTimeKind.Local).AddTicks(2649));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 8, 21, 39, 35, 232, DateTimeKind.Local).AddTicks(2653));

            migrationBuilder.CreateIndex(
                name: "IX_Births_FatherAnimalId",
                table: "Births",
                column: "FatherAnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_Births_MotherAnimalId",
                table: "Births",
                column: "MotherAnimalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Births_Animals_AnimalId",
                table: "Births",
                column: "AnimalId",
                principalTable: "Animals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Births_Animals_FatherAnimalId",
                table: "Births",
                column: "FatherAnimalId",
                principalTable: "Animals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Births_Animals_MotherAnimalId",
                table: "Births",
                column: "MotherAnimalId",
                principalTable: "Animals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Births_Animals_AnimalId",
                table: "Births");

            migrationBuilder.DropForeignKey(
                name: "FK_Births_Animals_FatherAnimalId",
                table: "Births");

            migrationBuilder.DropForeignKey(
                name: "FK_Births_Animals_MotherAnimalId",
                table: "Births");

            migrationBuilder.DropIndex(
                name: "IX_Births_FatherAnimalId",
                table: "Births");

            migrationBuilder.DropIndex(
                name: "IX_Births_MotherAnimalId",
                table: "Births");

            migrationBuilder.DropColumn(
                name: "FatherAnimalId",
                table: "Births");

            migrationBuilder.DropColumn(
                name: "LitterSize",
                table: "Births");

            migrationBuilder.RenameColumn(
                name: "MotherAnimalId",
                table: "Births",
                newName: "NumberOfOffspring");

            migrationBuilder.RenameColumn(
                name: "LiveBorn",
                table: "Births",
                newName: "NumberAlive");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Births",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<int>(
                name: "AnimalId",
                table: "Births",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 8, 21, 5, 24, 27, DateTimeKind.Local).AddTicks(5879));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 8, 21, 5, 24, 27, DateTimeKind.Local).AddTicks(5907));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 8, 21, 5, 24, 27, DateTimeKind.Local).AddTicks(5910));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 8, 21, 5, 24, 27, DateTimeKind.Local).AddTicks(5912));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 8, 21, 5, 24, 27, DateTimeKind.Local).AddTicks(5913));

            migrationBuilder.AddForeignKey(
                name: "FK_Births_Animals_AnimalId",
                table: "Births",
                column: "AnimalId",
                principalTable: "Animals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
