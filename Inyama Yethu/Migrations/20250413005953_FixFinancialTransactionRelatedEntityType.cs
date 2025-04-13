using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inyama_Yethu.Migrations
{
    /// <inheritdoc />
    public partial class FixFinancialTransactionRelatedEntityType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 2, 59, 53, 355, DateTimeKind.Local).AddTicks(3516));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 2, 59, 53, 355, DateTimeKind.Local).AddTicks(3532));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 2, 59, 53, 355, DateTimeKind.Local).AddTicks(3533));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 2, 59, 53, 355, DateTimeKind.Local).AddTicks(3534));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 2, 59, 53, 355, DateTimeKind.Local).AddTicks(3535));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
