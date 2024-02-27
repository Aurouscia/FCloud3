using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCloud3.DbContexts.Migrations.SqliteDevMigrations
{
    /// <inheritdoc />
    public partial class createAuthGrantTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthGrants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    On = table.Column<int>(type: "INTEGER", nullable: false),
                    OnId = table.Column<int>(type: "INTEGER", nullable: false),
                    To = table.Column<int>(type: "INTEGER", nullable: false),
                    ToId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReject = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatorUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthGrants", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthGrants");
        }
    }
}
