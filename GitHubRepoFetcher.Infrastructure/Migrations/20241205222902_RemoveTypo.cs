using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitHubRepoFetcher.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CommiterEmail",
                table: "Commits",
                newName: "CommitterEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CommitterEmail",
                table: "Commits",
                newName: "CommiterEmail");
        }
    }
}
