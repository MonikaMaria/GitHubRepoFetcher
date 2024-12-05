using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitHubRepoFetcher.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Commits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RepositoryName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Sha = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(90)", maxLength: 90, nullable: false),
                    CommitterName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CommiterEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commits_UserName_RepositoryName_Sha",
                table: "Commits",
                columns: new[] { "UserName", "RepositoryName", "Sha" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Commits");
        }
    }
}
