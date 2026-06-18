using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Decidi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewerRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewerRole",
                table: "Reviews",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewerRole",
                table: "Reviews");
        }
    }
}
