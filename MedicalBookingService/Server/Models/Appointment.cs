using MedicalBookingService.Server.Models;
using System.ComponentModel.DataAnnotations;

public class Appointment
{
    public int Id { get; set; }
    [Required]
    public DateTime Start { get; set; }
    [Required]
    public DateTime End { get; set; }
    [Required]
    public string Title { get; set; } = default!;
    public string? Notes { get; set; }
    public bool IsBlocked { get; set; } = false;
    // Approval fields
    public bool IsApproved { get; set; } = false;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByAdminId { get; set; } // FK to AppUser (Admin)
    public AppUser? ApprovedByAdmin { get; set; }
    // Relationships
    [Required]
    public string PatientId { get; set; } = default!;
    public AppUser Patient { get; set; } = default!;
    public string? DoctorId { get; set; } // optional
    public AppUser? Doctor { get; set; }
    [Required]
    public int OfficeId { get; set; }
    public Office Office { get; set; } = default!;
}
