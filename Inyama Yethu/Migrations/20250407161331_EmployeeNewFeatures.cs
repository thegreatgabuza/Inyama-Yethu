using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inyama_Yethu.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeNewFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PigletsMummified",
                table: "Matings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecordedById",
                table: "Matings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordedDate",
                table: "Matings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dosage",
                table: "HealthRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Medication",
                table: "HealthRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PerformedById",
                table: "HealthRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TreatmentOutcome",
                table: "HealthRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TreatmentType",
                table: "HealthRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "CostPerKg",
                table: "Feedings",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "FeedTypeString",
                table: "Feedings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FeedingDate",
                table: "Feedings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "RecordedById",
                table: "Feedings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordedDate",
                table: "Feedings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AverageBirthWeight",
                table: "Births",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberAlive",
                table: "Births",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Matings_RecordedById",
                table: "Matings",
                column: "RecordedById");

            migrationBuilder.CreateIndex(
                name: "IX_HealthRecords_PerformedById",
                table: "HealthRecords",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_Feedings_RecordedById",
                table: "Feedings",
                column: "RecordedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedings_Employees_RecordedById",
                table: "Feedings",
                column: "RecordedById",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthRecords_Employees_PerformedById",
                table: "HealthRecords",
                column: "PerformedById",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Matings_Employees_RecordedById",
                table: "Matings",
                column: "RecordedById",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedings_Employees_RecordedById",
                table: "Feedings");

            migrationBuilder.DropForeignKey(
                name: "FK_HealthRecords_Employees_PerformedById",
                table: "HealthRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Matings_Employees_RecordedById",
                table: "Matings");

            migrationBuilder.DropIndex(
                name: "IX_Matings_RecordedById",
                table: "Matings");

            migrationBuilder.DropIndex(
                name: "IX_HealthRecords_PerformedById",
                table: "HealthRecords");

            migrationBuilder.DropIndex(
                name: "IX_Feedings_RecordedById",
                table: "Feedings");

            migrationBuilder.DropColumn(
                name: "PigletsMummified",
                table: "Matings");

            migrationBuilder.DropColumn(
                name: "RecordedById",
                table: "Matings");

            migrationBuilder.DropColumn(
                name: "RecordedDate",
                table: "Matings");

            migrationBuilder.DropColumn(
                name: "Dosage",
                table: "HealthRecords");

            migrationBuilder.DropColumn(
                name: "Medication",
                table: "HealthRecords");

            migrationBuilder.DropColumn(
                name: "PerformedById",
                table: "HealthRecords");

            migrationBuilder.DropColumn(
                name: "TreatmentOutcome",
                table: "HealthRecords");

            migrationBuilder.DropColumn(
                name: "TreatmentType",
                table: "HealthRecords");

            migrationBuilder.DropColumn(
                name: "FeedTypeString",
                table: "Feedings");

            migrationBuilder.DropColumn(
                name: "FeedingDate",
                table: "Feedings");

            migrationBuilder.DropColumn(
                name: "RecordedById",
                table: "Feedings");

            migrationBuilder.DropColumn(
                name: "RecordedDate",
                table: "Feedings");

            migrationBuilder.DropColumn(
                name: "AverageBirthWeight",
                table: "Births");

            migrationBuilder.DropColumn(
                name: "NumberAlive",
                table: "Births");

            migrationBuilder.AlterColumn<decimal>(
                name: "CostPerKg",
                table: "Feedings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }
    }
}
