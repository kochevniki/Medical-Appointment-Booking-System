namespace MedicalBookingService.Server.Models
{
    public class PatientProfile
    {
        public int Id { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string SSN { get; set; }
        public string Address { get; set; }
        public string? GovernmentIdUrl { get; set; }
        public string? InsuranceCardUrl { get; set; }
    }
}
