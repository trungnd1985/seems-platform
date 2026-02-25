using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seems.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPageHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pages_Slug",
                table: "Pages");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Pages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Pages",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Pages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Populate Path from the existing Slug (old Slug was the full path),
            // then trim Slug down to the last segment only.
            // regexp_replace removes everything up to and including the last '/' (greedy .*).
            // Handles empty slugs (home page), single-segment slugs, and multi-segment slugs safely.
            migrationBuilder.Sql(@"
                UPDATE ""Pages""
                SET ""Path"" = ""Slug"",
                    ""Slug"" = regexp_replace(""Slug"", '^.*/', '');
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_ParentId_Slug",
                table: "Pages",
                columns: new[] { "ParentId", "Slug" });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_Path",
                table: "Pages",
                column: "Path",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_Pages_ParentId",
                table: "Pages",
                column: "ParentId",
                principalTable: "Pages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pages_Pages_ParentId",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_ParentId_Slug",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_Path",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Pages");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_Slug",
                table: "Pages",
                column: "Slug",
                unique: true);
        }
    }
}
