using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCloud3.DbContexts.Migrations.SqliteDevMigrations
{
    /// <inheritdoc />
    public partial class sqliteDevUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Hidden",
                table: "WikiItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Hidden",
                table: "DiffContents",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hidden",
                table: "WikiItems");

            migrationBuilder.DropColumn(
                name: "Hidden",
                table: "DiffContents");
        }
    }
}
