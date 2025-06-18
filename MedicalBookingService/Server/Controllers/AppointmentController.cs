using MedicalBookingService.Server.Data;
using MedicalBookingService.Server.Models;
using MedicalBookingService.Server.Services;
using MedicalBookingService.Shared.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace MedicalBookingService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly EmailService _emailService;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(UserManager<AppUser> userManager, ApplicationDbContext db, EmailService emailService, ILogger<AppointmentController> logger)
        {
            _userManager = userManager;
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost("book")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentDto appointmentDto)
        {
            var appointment = new Appointment
            {
                Start = appointmentDto.Start,
                End = appointmentDto.End,
                Title = appointmentDto.Title,
                PatientId = appointmentDto.PatientId,
                DoctorId = appointmentDto.DoctorId,
                OfficeId = appointmentDto.OfficeId,
                Notes = appointmentDto.Notes
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            // After appointment saved to DB:
            var adminEmails = await _db.Users
                .Where(u => u.AdminProfile != null && u.AdminProfile.OfficeId == appointment.OfficeId)
                .Select(u => u.Email)
                .ToListAsync();

            foreach (var email in adminEmails)
            {
                await _emailService.SendEmailAsync(email, "New Appointment Booked", 
                    $"A new appointment has been booked from {appointment.Start} to {appointment.End}. Please review and approve it.");
            }

            return Ok();
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveAppointment(int id, string adminId)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            if (appointment.IsApproved)
                return BadRequest("Appointment is already approved.");

            appointment.IsApproved = true;
            appointment.ApprovedAt = DateTime.UtcNow;
            appointment.ApprovedByAdminId = adminId;

            await _db.SaveChangesAsync();

            var officeName = await _db.Offices
                .Where(o => o.Id == appointment.OfficeId)
                .Select(o => o.Name)
                .FirstOrDefaultAsync();

            var patientEmail = await _db.Users
                .Where(u => u.Id == appointment.PatientId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
            if (patientEmail != null)
            {
                await _emailService.SendEmailAsync(patientEmail, $"Appointment Approved - {officeName}",
                    $"Your appointment from {appointment.Start} to {appointment.End} has been approved by the admin.");
            }

                return Ok();
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectAppointment(int id, string adminId)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            if (appointment.IsRejected)
                return BadRequest("Appointment is already rejected.");

            appointment.IsRejected = true;
            appointment.ApprovedAt = DateTime.UtcNow;
            appointment.ApprovedByAdminId = adminId;

            await _db.SaveChangesAsync();

            var officeName = await _db.Offices
                .Where(o => o.Id == appointment.OfficeId)
                .Select(o => o.Name)
                .FirstOrDefaultAsync();

            var patientEmail = await _db.Users
                .Where(u => u.Id == appointment.PatientId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
            if (patientEmail != null)
            {
                await _emailService.SendEmailAsync(patientEmail, $"Appointment Rejected - {officeName}",
                    $"Your appointment from {appointment.Start} to {appointment.End} has been rejected by the admin. " +
                    $"Please contact for more details.");
            }

            return Ok();
        }

        [HttpPost("admin-block")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminBlockAppointment([FromBody] AppointmentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || dto.Start == default || dto.End == default || dto.OfficeId <= 0 || string.IsNullOrWhiteSpace(dto.PatientId))
                return BadRequest("Missing required fields.");

            var appointment = new Appointment
            {
                Title = dto.Title,
                Start = dto.Start,
                End = dto.End,
                OfficeId = dto.OfficeId,
                PatientId = dto.PatientId, // Use admin's user ID
                DoctorId = null,
                Notes = null,
                IsApproved = true,
                IsBlocked = true,
                ApprovedAt = DateTime.UtcNow,
                ApprovedByAdminId = dto.PatientId // Admin's user ID
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            return Ok(new { appointment.Id });
        }

        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetDepartmentAppointments(int departmentId)
        {
            var appointments = await _db.Appointments
                .Where(a => a.OfficeId == departmentId)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Start = a.Start,
                    End = a.End,
                    Notes = a.Notes,
                    IsApproved = a.IsApproved,
                    IsRejected = a.IsRejected,
                    PatientId = a.PatientId,
                    DoctorId = a.DoctorId,
                    OfficeId = a.OfficeId
                }).ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("office/{officeId}/available-doctors")]
        public async Task<IActionResult> GetAvailableDoctors(int officeId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            // Get all doctors in the office
            var doctors = await _db.DoctorProfiles
                .Where(d => d.OfficeId == officeId)
                .Select(d => new DoctorDto
                {
                    AppUserId = d.AppUserId,
                    FirstName = d.FirstName,
                    LastName = d.LastName
                })
                .ToListAsync();

            // Get all appointments for this slot in this office that are approved and not rejected/blocked
            var overlappingAppointments = await _db.Appointments
                .Where(a => a.OfficeId == officeId &&
                            a.DoctorId != null &&
                            a.IsApproved && !a.IsRejected && !a.IsBlocked &&
                            a.Start < end && a.End > start)
                .Select(a => a.DoctorId)
                .ToListAsync();

            // Only return doctors not already booked in this slot
            var availableDoctors = doctors
                .Where(d => !overlappingAppointments.Contains(d.AppUserId))
                .ToList();

            return Ok(availableDoctors);
        }

        [HttpGet("patient/{userId}")]
        public async Task<IActionResult> GetPatientAppointments(string userId)
        {
            var appointments = await _db.Appointments
                .Where(a => a.PatientId == userId)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.DoctorProfile)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Start = a.Start,
                    End = a.End,
                    Notes = a.Notes,
                    IsApproved = a.IsApproved,
                    IsRejected = a.IsRejected,
                    PatientId = a.PatientId,
                    PatientFirstName = a.Patient.PatientProfile.FirstName,
                    PatientLastName = a.Patient.PatientProfile.LastName,
                    DoctorId = a.DoctorId,
                    DoctorFirstName = a.Doctor != null ? a.Doctor.DoctorProfile.FirstName : null,
                    DoctorLastName = a.Doctor != null ? a.Doctor.DoctorProfile.LastName : null,
                    OfficeId = a.OfficeId
                }).ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("doctor/{userId}")]
        public async Task<IActionResult> GetDoctorAppointments(string userId)
        {
            var appointments = await _db.Appointments
                .Where(a => a.DoctorId == userId && a.IsApproved)
                .Include(a => a.Patient)
                .ThenInclude(p => p.PatientProfile)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Start = a.Start,
                    End = a.End,
                    Notes = a.Notes,
                    IsApproved = a.IsApproved,
                    IsRejected = a.IsRejected,
                    PatientId = a.PatientId,
                    PatientFirstName = a.Patient.PatientProfile.FirstName,
                    PatientLastName = a.Patient.PatientProfile.LastName,
                    DoctorId = a.DoctorId,
                    DoctorFirstName = a.Doctor != null ? a.Doctor.DoctorProfile.FirstName : null,
                    DoctorLastName = a.Doctor != null ? a.Doctor.DoctorProfile.LastName : null,
                    OfficeId = a.OfficeId
                }).ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("admin/{userId}")]
        public async Task<IActionResult> GetAdminAppointments(string userId)
        {
            var adminOfficeId = await _db.AdminProfiles
                .Where(a => a.AppUserId == userId)
                .Select(a => a.OfficeId)
                .FirstOrDefaultAsync();

            var appointments = await _db.Appointments
                .Where(a => a.OfficeId == adminOfficeId)
                .Include(a => a.Patient)
                .ThenInclude(p => p.PatientProfile)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.DoctorProfile)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Start = a.Start,
                    End = a.End,
                    Notes = a.Notes,
                    IsApproved = a.IsApproved,
                    IsRejected = a.IsRejected,
                    PatientId = a.PatientId,
                    PatientFirstName = a.Patient.PatientProfile.FirstName,
                    PatientLastName = a.Patient.PatientProfile.LastName,
                    DoctorId = a.DoctorId,
                    DoctorFirstName = a.Doctor != null ? a.Doctor.DoctorProfile.FirstName : null,
                    DoctorLastName = a.Doctor != null ? a.Doctor.DoctorProfile.LastName : null,
                    OfficeId = a.OfficeId
                }).ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("admin/{userId}/office")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminOfficeDto>> GetAdminOffice(string userId)
        {
            var adminProfile = await _db.AdminProfiles
                .Where(ap => ap.AppUserId == userId)
                .Select(ap => new AdminOfficeDto { OfficeId = ap.OfficeId, OfficeName = ap.Office.Name })
                .FirstOrDefaultAsync();

            if (adminProfile == null)
            {
                return NotFound("Admin profile not found or not associated with an office.");
            }
            return Ok(adminProfile);
        }

        [HttpGet("doctor/{userId}/office")]
        public async Task<ActionResult<DoctorOfficeDto>> GetDoctorOffice(string userId)
        {
            var doctorProfile = await _db.DoctorProfiles
                .Where(dp => dp.AppUserId == userId)
                .Select(dp => new DoctorOfficeDto { OfficeId = dp.OfficeId, OfficeName = dp.Office.Name })
                .FirstOrDefaultAsync();

            if (doctorProfile == null)
            {
                return NotFound("Doctor profile not found or not associated with an office.");
            }
            return Ok(doctorProfile);
        }
    }
}
