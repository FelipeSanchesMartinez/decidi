using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Decidi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Sprint7_IndexesAndUtcConverters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_AcceptedFreelancerId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ClientId",
                table: "Projects");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_FreelancerId_Status",
                table: "Proposals",
                columns: new[] { "FreelancerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_AcceptedFreelancerId_Status",
                table: "Projects",
                columns: new[] { "AcceptedFreelancerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ClientId_Status",
                table: "Projects",
                columns: new[] { "ClientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status_CategoryId_CreatedAt",
                table: "Projects",
                columns: new[] { "Status", "CategoryId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ClientId_Status",
                table: "Payments",
                columns: new[] { "ClientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_FreelancerId_Status_ReleasedAt",
                table: "Payments",
                columns: new[] { "FreelancerId", "Status", "ReleasedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_FreelancerId_Status",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Projects_AcceptedFreelancerId_Status",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ClientId_Status",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Status_CategoryId_CreatedAt",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ClientId_Status",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_FreelancerId_Status_ReleasedAt",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_AcceptedFreelancerId",
                table: "Projects",
                column: "AcceptedFreelancerId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ClientId",
                table: "Projects",
                column: "ClientId");
        }
    }
}
