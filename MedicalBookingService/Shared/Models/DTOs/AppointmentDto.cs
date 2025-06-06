namespace MedicalBookingService.Shared.Models.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string? Notes { get; set; }
        public bool IsBlocked { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        public string? ApprovedByAdminId { get; set; }
        public string? PatientId { get; set; }
        public string? DoctorId { get; set; }
        public int OfficeId { get; set; }
    }
}
