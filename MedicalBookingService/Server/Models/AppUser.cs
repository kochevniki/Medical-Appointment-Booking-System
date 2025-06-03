using Microsoft.AspNetCore.Identity;

namespace MedicalBookingService.Server.Models
{
    public class AppUser : IdentityUser
    {
        public string Role { get; set; }
        public int? OfficeId { get; set; }
        public Office Office { get; set; }
        public PatientProfile? PatientProfile { get; set; }
        public DoctorProfile? DoctorProfile { get; set; }
        public AdminProfile? AdminProfile { get; set; }
    }
}