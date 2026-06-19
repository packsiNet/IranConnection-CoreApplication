using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IranConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWireGuardPeer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WireGuardPeers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PrivateKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AssignedIp = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastHandshake = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BytesReceived = table.Column<long>(type: "bigint", nullable: false),
                    BytesSent = table.Column<long>(type: "bigint", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WireGuardPeers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WireGuardPeers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WireGuardPeers_AssignedIp",
                table: "WireGuardPeers",
                column: "AssignedIp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WireGuardPeers_PublicKey",
                table: "WireGuardPeers",
                column: "PublicKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WireGuardPeers_UserId",
                table: "WireGuardPeers",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WireGuardPeers");
        }
    }
}
