using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inyama_Yethu.Migrations
{
    /// <inheritdoc />
    public partial class FixRelatedEntityType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 3, 0, 4, 408, DateTimeKind.Local).AddTicks(8541));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 3, 0, 4, 408, DateTimeKind.Local).AddTicks(8559));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 3, 0, 4, 408, DateTimeKind.Local).AddTicks(8561));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 3, 0, 4, 408, DateTimeKind.Local).AddTicks(8563));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 3, 0, 4, 408, DateTimeKind.Local).AddTicks(8564));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
