using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalBookingService.Migrations
{
    /// <inheritdoc />
    public partial class CreateDepartmentScheduleConfigTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfficeId = table.Column<int>(type: "int", nullable: false),
                    WorkStart = table.Column<TimeSpan>(type: "time", nullable: false),
                    WorkEnd = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleConfigs_Offices_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleConfigs_OfficeId",
                table: "ScheduleConfigs",
                column: "OfficeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleConfigs");
        }
    }
}
