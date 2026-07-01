using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IranConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IranianAppsUpdateVersion",
                table: "AppSettings",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "1.0.0");

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "AppSettings",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "1.0.0");

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"),
                columns: new string[0],
                values: new object[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IranianAppsUpdateVersion",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AppSettings");
        }
    }
}
