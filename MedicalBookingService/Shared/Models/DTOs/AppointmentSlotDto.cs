namespace MedicalBookingService.Shared.Models.DTOs
{
    public class AppointmentSlotDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int BookedCount { get; set; }
        public int MaxCapacity { get; set; }
        public bool IsBlocked { get; set; } // true if admin blocked
    }
}
