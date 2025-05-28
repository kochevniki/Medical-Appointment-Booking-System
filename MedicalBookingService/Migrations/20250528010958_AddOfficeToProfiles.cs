using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalBookingService.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficeToProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OfficeId",
                table: "DoctorProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OfficeId",
                table: "AdminProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProfiles_OfficeId",
                table: "DoctorProfiles",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminProfiles_OfficeId",
                table: "AdminProfiles",
                column: "OfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminProfiles_Offices_OfficeId",
                table: "AdminProfiles",
                column: "OfficeId",
                principalTable: "Offices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorProfiles_Offices_OfficeId",
                table: "DoctorProfiles",
                column: "OfficeId",
                principalTable: "Offices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminProfiles_Offices_OfficeId",
                table: "AdminProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorProfiles_Offices_OfficeId",
                table: "DoctorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_DoctorProfiles_OfficeId",
                table: "DoctorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AdminProfiles_OfficeId",
                table: "AdminProfiles");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "AdminProfiles");
        }
    }
}
