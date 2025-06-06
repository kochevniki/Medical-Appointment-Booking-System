using MedicalBookingService.Server.Data;
using MedicalBookingService.Server.Models;
using MedicalBookingService.Server.Services;
using MedicalBookingService.Shared.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalBookingService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly EmailService _emailService;

        public AppointmentController(UserManager<AppUser> userManager, ApplicationDbContext db, EmailService emailService)
        {
            _userManager = userManager;
            _db = db;
            _emailService = emailService;
        }

        [HttpPost("book")]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentDto appointmentDto)
        {
            var appointment = new Appointment
            {
                Start = appointmentDto.Start,
                End = appointmentDto.End,
                Title = appointmentDto.Title,
                PatientId = appointmentDto.PatientId,
                DoctorId = appointmentDto.DoctorId,
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("slots")]
        public async Task<IActionResult> GetAvailableSlots(int officeId, DateTime date)
        {
            var schedule = await _db.ScheduleConfigs
                .FirstOrDefaultAsync(c => c.OfficeId == officeId);

            if (schedule == null)
                return BadRequest("Schedule not configured.");

            var startTime = date.Date + schedule.WorkStart;
            var endTime = date.Date + schedule.WorkEnd;

            var doctorsCount = await _db.DoctorProfiles.CountAsync(d => d.OfficeId == officeId);

            var existingAppointments = await _db.Appointments
                .Where(a => a.OfficeId == officeId &&
                            a.Start.Date == date.Date &&
                            a.IsApproved)
                .ToListAsync();

            var slots = new List<AppointmentSlotDto>();

            for (var slotStart = startTime; slotStart < endTime; slotStart = slotStart.AddMinutes(30))
            {
                var slotEnd = slotStart.AddMinutes(30);

                var overlappingAppointments = existingAppointments
                    .Where(a =>
                        a.Start < slotEnd && a.End > slotStart
                    ).ToList();

                bool isBlocked = overlappingAppointments.Any(a => a.DoctorId == null && a.Title == "Busy");

                slots.Add(new AppointmentSlotDto
                {
                    Start = slotStart,
                    End = slotEnd,
                    BookedCount = overlappingAppointments.Count(a => a.DoctorId != null),
                    MaxCapacity = doctorsCount,
                    IsBlocked = isBlocked
                });
            }

            return Ok(slots);
        }

        [HttpGet("patient/{userId}")]
        public async Task<IActionResult> GetPatientAppointments(string userId)
        {
            var appointments = await _db.Appointments
                .Where(a => a.PatientId! == userId)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Start = a.Start,
                    End = a.End
                }).ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("doctor/{userId}")]
        public async Task<IActionResult> GetDoctorAppointments(string userId)
        {
            var appointments = await _db.Appointments
                .Where(a => a.DoctorId == userId)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Start = a.Start,
                    End = a.End
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
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Start = a.Start,
                    End = a.End
                }).ToListAsync();

            return Ok(appointments);
        }


    }
}
