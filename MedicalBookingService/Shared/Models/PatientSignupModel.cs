using System.ComponentModel.DataAnnotations;

namespace MedicalBookingService.Shared.Models
{
    public class PatientSignupModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(11, MinimumLength = 9)]
        public string SSN { get; set; } = string.Empty;

        [Required, Compare(nameof(SSN), ErrorMessage = "SSNs do not match")]
        public string ConfirmSSN { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Address { get; set; } = string.Empty;
        [Required]
        public string GovernmentIdUrl { get; set; }
        [Required]
        public string InsuranceCardUrl { get; set; }

    }
}
