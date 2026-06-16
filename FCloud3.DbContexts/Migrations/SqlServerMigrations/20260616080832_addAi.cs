using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCloud3.DbContexts.Migrations.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class addAi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiConversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AiInstanceConfigId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentWikiItemId = table.Column<int>(type: "int", nullable: false),
                    MessageCount = table.Column<int>(type: "int", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiInstanceConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    ApiBaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemPrompt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    DefaultDirId = table.Column<int>(type: "int", nullable: false),
                    MaxContextMessages = table.Column<int>(type: "int", nullable: false),
                    DailyTokenLimit = table.Column<int>(type: "int", nullable: false),
                    MonthlyTokenLimit = table.Column<int>(type: "int", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiInstanceConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToolCalls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    TokenCount = table.Column<int>(type: "int", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiUsageRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AiInstanceConfigId = table.Column<int>(type: "int", nullable: false),
                    InputTokens = table.Column<int>(type: "int", nullable: false),
                    OutputTokens = table.Column<int>(type: "int", nullable: false),
                    TotalTokens = table.Column<int>(type: "int", nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    PromptSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatedWikiItemId = table.Column<int>(type: "int", nullable: false),
                    ConversationId = table.Column<int>(type: "int", nullable: true),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiUsageRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WikiToDirs_DirId_WikiId",
                table: "WikiToDirs",
                columns: new[] { "DirId", "WikiId" });

            migrationBuilder.CreateIndex(
                name: "IX_WikiToDirs_WikiId",
                table: "WikiToDirs",
                column: "WikiId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiTitleContains_ObjectId_Type",
                table: "WikiTitleContains",
                columns: new[] { "ObjectId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_WikiTitleContains_WikiId",
                table: "WikiTitleContains",
                column: "WikiId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiTemplates_CreatorUserId",
                table: "WikiTemplates",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiTemplates_Name",
                table: "WikiTemplates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_WikiSelecteds_WikiItemId",
                table: "WikiSelecteds",
                column: "WikiItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiRefs_Str",
                table: "WikiRefs",
                column: "Str");

            migrationBuilder.CreateIndex(
                name: "IX_WikiRefs_WikiId",
                table: "WikiRefs",
                column: "WikiId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiParas_ObjectId_Type",
                table: "WikiParas",
                columns: new[] { "ObjectId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_WikiParas_WikiItemId_Order",
                table: "WikiParas",
                columns: new[] { "WikiItemId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_WikiItems_LastActive",
                table: "WikiItems",
                column: "LastActive");

            migrationBuilder.CreateIndex(
                name: "IX_WikiItems_OwnerUserId",
                table: "WikiItems",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiItems_UrlPathName",
                table: "WikiItems",
                column: "UrlPathName");

            migrationBuilder.CreateIndex(
                name: "IX_UserToGroups_GroupId_UserId",
                table: "UserToGroups",
                columns: new[] { "GroupId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserToGroups_UserId",
                table: "UserToGroups",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AvatarMaterialId",
                table: "Users",
                column: "AvatarMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_OwnerUserId",
                table: "UserGroups",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConfigs_CreatorUserId_On_Type",
                table: "UserConfigs",
                columns: new[] { "CreatorUserId", "On", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_TextSections_CreatorUserId",
                table: "TextSections",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpRecords_CreatorUserId",
                table: "OpRecords",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpRecords_TargetType_ObjA",
                table: "OpRecords",
                columns: new[] { "TargetType", "ObjA" });

            migrationBuilder.CreateIndex(
                name: "IX_OpRecords_TargetType_ObjA_ObjB",
                table: "OpRecords",
                columns: new[] { "TargetType", "ObjA", "ObjB" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Receiver_Read",
                table: "Notifications",
                columns: new[] { "Receiver", "Read" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Sender",
                table: "Notifications",
                column: "Sender");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CreatorUserId",
                table: "Messages",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId_Read",
                table: "Messages",
                columns: new[] { "ReceiverId", "Read" });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Name",
                table: "Materials",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_StorePathName",
                table: "Materials",
                column: "StorePathName");

            migrationBuilder.CreateIndex(
                name: "IX_FreeTables_CreatorUserId",
                table: "FreeTables",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FileItems_Hash",
                table: "FileItems",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_FileItems_InDir",
                table: "FileItems",
                column: "InDir");

            migrationBuilder.CreateIndex(
                name: "IX_FileItems_StorePathName",
                table: "FileItems",
                column: "StorePathName");

            migrationBuilder.CreateIndex(
                name: "IX_FileDirs_AsDir",
                table: "FileDirs",
                column: "AsDir");

            migrationBuilder.CreateIndex(
                name: "IX_FileDirs_ParentDir_UrlPathName",
                table: "FileDirs",
                columns: new[] { "ParentDir", "UrlPathName" });

            migrationBuilder.CreateIndex(
                name: "IX_FileDirs_RootDir",
                table: "FileDirs",
                column: "RootDir");

            migrationBuilder.CreateIndex(
                name: "IX_FileDirs_UrlPathName",
                table: "FileDirs",
                column: "UrlPathName");

            migrationBuilder.CreateIndex(
                name: "IX_DiffSingles_DiffContentId",
                table: "DiffSingles",
                column: "DiffContentId");

            migrationBuilder.CreateIndex(
                name: "IX_DiffContents_CreatorUserId",
                table: "DiffContents",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiffContents_ObjectId_DiffType",
                table: "DiffContents",
                columns: new[] { "ObjectId", "DiffType" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CreatorUserId",
                table: "Comments",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ReplyingTo",
                table: "Comments",
                column: "ReplyingTo");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TargetType_TargetObjId",
                table: "Comments",
                columns: new[] { "TargetType", "TargetObjId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthGrants_On_OnId_Order",
                table: "AuthGrants",
                columns: new[] { "On", "OnId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthGrants_To_ToId",
                table: "AuthGrants",
                columns: new[] { "To", "ToId" });

            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_CurrentWikiItemId",
                table: "AiConversations",
                column: "CurrentWikiItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_UserId_AiInstanceConfigId",
                table: "AiConversations",
                columns: new[] { "UserId", "AiInstanceConfigId" });

            migrationBuilder.CreateIndex(
                name: "IX_AiInstanceConfigs_DefaultDirId",
                table: "AiInstanceConfigs",
                column: "DefaultDirId");

            migrationBuilder.CreateIndex(
                name: "IX_AiInstanceConfigs_GroupId",
                table: "AiInstanceConfigs",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AiMessages_ConversationId_Order",
                table: "AiMessages",
                columns: new[] { "ConversationId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_AiUsageRecords_AiInstanceConfigId",
                table: "AiUsageRecords",
                column: "AiInstanceConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_AiUsageRecords_ConversationId",
                table: "AiUsageRecords",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_AiUsageRecords_RelatedWikiItemId",
                table: "AiUsageRecords",
                column: "RelatedWikiItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AiUsageRecords_UserId_Created",
                table: "AiUsageRecords",
                columns: new[] { "UserId", "Created" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiConversations");

            migrationBuilder.DropTable(
                name: "AiInstanceConfigs");

            migrationBuilder.DropTable(
                name: "AiMessages");

            migrationBuilder.DropTable(
                name: "AiUsageRecords");

            migrationBuilder.DropIndex(
                name: "IX_WikiToDirs_DirId_WikiId",
                table: "WikiToDirs");

            migrationBuilder.DropIndex(
                name: "IX_WikiToDirs_WikiId",
                table: "WikiToDirs");

            migrationBuilder.DropIndex(
                name: "IX_WikiTitleContains_ObjectId_Type",
                table: "WikiTitleContains");

            migrationBuilder.DropIndex(
                name: "IX_WikiTitleContains_WikiId",
                table: "WikiTitleContains");

            migrationBuilder.DropIndex(
                name: "IX_WikiTemplates_CreatorUserId",
                table: "WikiTemplates");

            migrationBuilder.DropIndex(
                name: "IX_WikiTemplates_Name",
                table: "WikiTemplates");

            migrationBuilder.DropIndex(
                name: "IX_WikiSelecteds_WikiItemId",
                table: "WikiSelecteds");

            migrationBuilder.DropIndex(
                name: "IX_WikiRefs_Str",
                table: "WikiRefs");

            migrationBuilder.DropIndex(
                name: "IX_WikiRefs_WikiId",
                table: "WikiRefs");

            migrationBuilder.DropIndex(
                name: "IX_WikiParas_ObjectId_Type",
                table: "WikiParas");

            migrationBuilder.DropIndex(
                name: "IX_WikiParas_WikiItemId_Order",
                table: "WikiParas");

            migrationBuilder.DropIndex(
                name: "IX_WikiItems_LastActive",
                table: "WikiItems");

            migrationBuilder.DropIndex(
                name: "IX_WikiItems_OwnerUserId",
                table: "WikiItems");

            migrationBuilder.DropIndex(
                name: "IX_WikiItems_UrlPathName",
                table: "WikiItems");

            migrationBuilder.DropIndex(
                name: "IX_UserToGroups_GroupId_UserId",
                table: "UserToGroups");

            migrationBuilder.DropIndex(
                name: "IX_UserToGroups_UserId",
                table: "UserToGroups");

            migrationBuilder.DropIndex(
                name: "IX_Users_AvatarMaterialId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserGroups_OwnerUserId",
                table: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_UserConfigs_CreatorUserId_On_Type",
                table: "UserConfigs");

            migrationBuilder.DropIndex(
                name: "IX_TextSections_CreatorUserId",
                table: "TextSections");

            migrationBuilder.DropIndex(
                name: "IX_OpRecords_CreatorUserId",
                table: "OpRecords");

            migrationBuilder.DropIndex(
                name: "IX_OpRecords_TargetType_ObjA",
                table: "OpRecords");

            migrationBuilder.DropIndex(
                name: "IX_OpRecords_TargetType_ObjA_ObjB",
                table: "OpRecords");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Receiver_Read",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Sender",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Messages_CreatorUserId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ReceiverId_Read",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Materials_Name",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_StorePathName",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_FreeTables_CreatorUserId",
                table: "FreeTables");

            migrationBuilder.DropIndex(
                name: "IX_FileItems_Hash",
                table: "FileItems");

            migrationBuilder.DropIndex(
                name: "IX_FileItems_InDir",
                table: "FileItems");

            migrationBuilder.DropIndex(
                name: "IX_FileItems_StorePathName",
                table: "FileItems");

            migrationBuilder.DropIndex(
                name: "IX_FileDirs_AsDir",
                table: "FileDirs");

            migrationBuilder.DropIndex(
                name: "IX_FileDirs_ParentDir_UrlPathName",
                table: "FileDirs");

            migrationBuilder.DropIndex(
                name: "IX_FileDirs_RootDir",
                table: "FileDirs");

            migrationBuilder.DropIndex(
                name: "IX_FileDirs_UrlPathName",
                table: "FileDirs");

            migrationBuilder.DropIndex(
                name: "IX_DiffSingles_DiffContentId",
                table: "DiffSingles");

            migrationBuilder.DropIndex(
                name: "IX_DiffContents_CreatorUserId",
                table: "DiffContents");

            migrationBuilder.DropIndex(
                name: "IX_DiffContents_ObjectId_DiffType",
                table: "DiffContents");

            migrationBuilder.DropIndex(
                name: "IX_Comments_CreatorUserId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ReplyingTo",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_TargetType_TargetObjId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_AuthGrants_On_OnId_Order",
                table: "AuthGrants");

            migrationBuilder.DropIndex(
                name: "IX_AuthGrants_To_ToId",
                table: "AuthGrants");
        }
    }
}
