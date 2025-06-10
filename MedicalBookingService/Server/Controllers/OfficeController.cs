using MedicalBookingService.Server.Data;
using MedicalBookingService.Server.Models;
using MedicalBookingService.Shared.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalBookingService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfficeController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _db;

        public OfficeController(UserManager<AppUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllDepartments()
        {
            var departments = await _db.Offices
                .Select(o => new DepartmentDto
                {
                    Id = o.Id,
                    Name = o.Name
                })
                .ToListAsync();

            return Ok(departments);
        }

        [HttpGet("{officeId}/available-doctors")]
        public async Task<IActionResult> GetAvailableDoctors(int officeId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            // Step 1: Get doctors assigned to this office
            var doctorsInOffice = await _userManager.Users
                .Where(u => u.DoctorProfile != null && u.DoctorProfile.OfficeId == officeId)
                .Select(u => new
                {
                    u.Id,
                    u.DoctorProfile.FirstName,
                    u.DoctorProfile.LastName,
                })
                .ToListAsync();

            // Step 2: Get IDs of doctors who already have an overlapping appointment
            var busyDoctorIds = await _db.Appointments
                .Where(a => a.OfficeId == officeId &&
                            a.DoctorId != null &&
                            a.Start < end &&
                            a.End > start)
                .Select(a => a.DoctorId!)
                .Distinct()
                .ToListAsync();

            // Step 3: Filter out busy doctors
            var availableDoctors = doctorsInOffice
                .Where(d => !busyDoctorIds.Contains(d.Id))
                .Select(d => new DoctorDto
                {
                    AppUserId = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName
                })
                .ToList();

            return Ok(availableDoctors);
        }
    }
}
