using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IranConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BandwidthLimitBytes",
                table: "WireGuardPeers",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BandwidthLimitBytes",
                table: "WireGuardPeers");
        }
    }
}
