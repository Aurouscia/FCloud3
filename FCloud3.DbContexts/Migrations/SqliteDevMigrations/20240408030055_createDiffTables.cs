using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCloud3.DbContexts.Migrations.SqliteDevMigrations
{
    /// <inheritdoc />
    public partial class createDiffTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiffContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiffType = table.Column<int>(type: "INTEGER", nullable: false),
                    RemovedChars = table.Column<int>(type: "INTEGER", nullable: false),
                    AddedChars = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatorUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiffContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiffSingles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiffContentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    Ori = table.Column<string>(type: "TEXT", nullable: true),
                    New = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiffSingles", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiffContents");

            migrationBuilder.DropTable(
                name: "DiffSingles");
        }
    }
}
