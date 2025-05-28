namespace MedicalBookingService.Server.Models
{
    public class DoctorProfile
    {
        public int Id { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialty { get; set; }
        public int OfficeId { get; set; }           // Optional: makes filtering easier
        public Office Office { get; set; }          // Required for navigation

    }
}
