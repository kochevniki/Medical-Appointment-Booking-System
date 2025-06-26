namespace MedicalBookingService.Server.Models
{
    public class BoxToken
    {
        public int Id { get; set; }
        public string? RefreshToken { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}
