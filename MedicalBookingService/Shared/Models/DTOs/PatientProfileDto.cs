using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalBookingService.Shared.Models.DTOs
{
    public class PatientProfileDto
    {
        public int Id { get; set; }
        public string AppUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string SSN { get; set; } // Consider masking or omitting for security
        public string Address { get; set; }
        public string? GovernmentIdUrl { get; set; }
        public string? InsuranceCardUrl { get; set; }
    }
}
