using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalBookingService.Server.Migrations
{
    /// <inheritdoc />
    public partial class BoxTokenAddingNewColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                schema: "dbo",
                table: "BoxTokens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                schema: "dbo",
                table: "BoxTokens");
        }
    }
}
