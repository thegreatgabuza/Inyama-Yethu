using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inyama_Yethu.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsReconciled = table.Column<bool>(type: "bit", nullable: false),
                    RecordedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 1, 14, 44, 611, DateTimeKind.Local).AddTicks(2169));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 1, 14, 44, 611, DateTimeKind.Local).AddTicks(2183));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 1, 14, 44, 611, DateTimeKind.Local).AddTicks(2185));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 1, 14, 44, 611, DateTimeKind.Local).AddTicks(2186));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 1, 14, 44, 611, DateTimeKind.Local).AddTicks(2187));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 0, 2, 59, 251, DateTimeKind.Local).AddTicks(5473));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 0, 2, 59, 251, DateTimeKind.Local).AddTicks(5491));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 0, 2, 59, 251, DateTimeKind.Local).AddTicks(5493));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 0, 2, 59, 251, DateTimeKind.Local).AddTicks(5494));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 0, 2, 59, 251, DateTimeKind.Local).AddTicks(5495));
        }
    }
}
