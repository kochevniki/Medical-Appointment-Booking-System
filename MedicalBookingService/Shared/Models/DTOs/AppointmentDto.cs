namespace MedicalBookingService.Shared.Models.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Title { get; set; } = default!;
        public string? Notes { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; } // Add this flag
        public string? PatientId { get; set; }
        public string? PatientFirstName { get; set; } // Add for display
        public string? PatientLastName { get; set; }  // Add for display
        public string? DoctorId { get; set; }
        public string? DoctorFirstName { get; set; }  // Add for display
        public string? DoctorLastName { get; set; }   // Add for display
        public int OfficeId { get; set; }
    }
}
