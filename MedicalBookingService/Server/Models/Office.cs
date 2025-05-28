namespace MedicalBookingService.Server.Models
{
    public class Office
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<AppUser> Users { get; set; } = new();
    }
}
