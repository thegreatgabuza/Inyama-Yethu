using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inyama_Yethu.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultValuesToStringFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 0, 1, 9, 680, DateTimeKind.Local).AddTicks(9287));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 0, 1, 9, 680, DateTimeKind.Local).AddTicks(9304));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 0, 1, 9, 680, DateTimeKind.Local).AddTicks(9305));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 0, 1, 9, 680, DateTimeKind.Local).AddTicks(9306));

            migrationBuilder.UpdateData(
                table: "FeedInventory",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastUpdated",
                value: new DateTime(2025, 4, 13, 0, 1, 9, 680, DateTimeKind.Local).AddTicks(9307));
        }
    }
}
