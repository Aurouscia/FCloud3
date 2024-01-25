using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCloud3.DbContexts.Migrations.SqliteDevMigrations
{
    /// <inheritdoc />
    public partial class addUserToGroupType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarFileName",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "PwdMd5",
                table: "Users",
                newName: "PwdEncrypted");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "UserToGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AvatarMaterialId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "UserToGroups");

            migrationBuilder.DropColumn(
                name: "AvatarMaterialId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "PwdEncrypted",
                table: "Users",
                newName: "PwdMd5");

            migrationBuilder.AddColumn<string>(
                name: "AvatarFileName",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }
    }
}
