using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seems.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleDescriptionAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Modules",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Modules",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Modules");
        }
    }
}
