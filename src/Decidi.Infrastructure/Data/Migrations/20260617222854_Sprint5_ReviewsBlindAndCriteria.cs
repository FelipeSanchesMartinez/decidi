using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Decidi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Sprint5_ReviewsBlindAndCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_ClientId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_FreelancerId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ProjectId_ClientId",
                table: "Reviews");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewerRole",
                table: "Reviews",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "RatingCommunication",
                table: "Reviews",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RatingDeadline",
                table: "Reviews",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RatingQuality",
                table: "Reviews",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleasedAt",
                table: "Reviews",
                type: "timestamp with time zone",
                nullable: true);

            // Reviews antigas são tratadas como já liberadas para não quebrar GET público
            // após a migration. Novas reviews começam Pending (default no domain).
            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                table: "Reviews",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Released");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ClientId_Visibility",
                table: "Reviews",
                columns: new[] { "ClientId", "Visibility" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_FreelancerId_Visibility",
                table: "Reviews",
                columns: new[] { "FreelancerId", "Visibility" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProjectId_ReviewerRole",
                table: "Reviews",
                columns: new[] { "ProjectId", "ReviewerRole" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_ClientId_Visibility",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_FreelancerId_Visibility",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ProjectId_ReviewerRole",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RatingCommunication",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RatingDeadline",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RatingQuality",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "ReleasedAt",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Reviews");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewerRole",
                table: "Reviews",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ClientId",
                table: "Reviews",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_FreelancerId",
                table: "Reviews",
                column: "FreelancerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProjectId_ClientId",
                table: "Reviews",
                columns: new[] { "ProjectId", "ClientId" },
                unique: true);
        }
    }
}
