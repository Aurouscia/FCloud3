using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCloud3.DbContexts.Migrations.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class addWikiTitleContainBlackList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BlackListed",
                table: "WikiTitleContains",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlackListed",
                table: "WikiTitleContains");
        }
    }
}
