using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCloud3.Services.Etc.TempData.Migrations
{
    /// <inheritdoc />
    public partial class CreateContentLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentEditLock",
                columns: table => new
                {
                    ObjectType = table.Column<int>(type: "INTEGER", nullable: false),
                    ObjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStamp = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentEditLock", x => new { x.ObjectType, x.ObjectId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentEditLock");
        }
    }
}
