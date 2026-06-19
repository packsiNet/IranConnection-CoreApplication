using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IranConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "Users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeviceUser",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "DeviceId", "IsDeviceUser" },
                values: new object[] { null, false });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DeviceId",
                table: "Users",
                column: "DeviceId",
                unique: true,
                filter: "\"DeviceId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_DeviceId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeviceUser",
                table: "Users");
        }
    }
}
