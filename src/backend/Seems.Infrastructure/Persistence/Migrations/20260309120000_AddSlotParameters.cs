using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seems.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSlotParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Parameters",
                table: "SlotMappings",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Parameters",
                table: "SlotMappings");
        }
    }
}
