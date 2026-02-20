using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seems.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Themes",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Themes");
        }
    }
}
