using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seems.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AltText",
                table: "Media",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Caption",
                table: "Media",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FolderId",
                table: "Media",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalName",
                table: "Media",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Media",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "Media",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MediaFolders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaFolders_MediaFolders_ParentId",
                        column: x => x.ParentId,
                        principalTable: "MediaFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Group = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Key);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Media_FolderId",
                table: "Media",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_OwnerId",
                table: "Media",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFolders_OwnerId",
                table: "MediaFolders",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFolders_OwnerId_ParentId_Name",
                table: "MediaFolders",
                columns: new[] { "OwnerId", "ParentId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFolders_ParentId",
                table: "MediaFolders",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_MediaFolders_FolderId",
                table: "Media",
                column: "FolderId",
                principalTable: "MediaFolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Media_MediaFolders_FolderId",
                table: "Media");

            migrationBuilder.DropTable(
                name: "MediaFolders");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropIndex(
                name: "IX_Media_FolderId",
                table: "Media");

            migrationBuilder.DropIndex(
                name: "IX_Media_OwnerId",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "AltText",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "Caption",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "OriginalName",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "Media");
        }
    }
}
