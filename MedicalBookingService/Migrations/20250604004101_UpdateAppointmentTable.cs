using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalBookingService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppointmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByAdminId",
                table: "Appointments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ApprovedByAdminId",
                table: "Appointments",
                column: "ApprovedByAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AspNetUsers_ApprovedByAdminId",
                table: "Appointments",
                column: "ApprovedByAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AspNetUsers_ApprovedByAdminId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ApprovedByAdminId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ApprovedByAdminId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Appointments");
        }
    }
}
