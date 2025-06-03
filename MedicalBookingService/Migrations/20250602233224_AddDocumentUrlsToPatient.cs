using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalBookingService.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentUrlsToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GovernmentIdUrl",
                table: "PatientProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceCardUrl",
                table: "PatientProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GovernmentIdUrl",
                table: "PatientProfiles");

            migrationBuilder.DropColumn(
                name: "InsuranceCardUrl",
                table: "PatientProfiles");
        }
    }
}
