namespace MedicalBookingService.Shared.Models.DTOs
{
    public class FileUrlsUpdateModel
    {
        public string UserId { get; set; } = default!;
        public string? GovernmentIdUrl { get; set; }
        public string? InsuranceCardUrl { get; set; }
    }
}
