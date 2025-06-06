namespace MedicalBookingService.Server.Models
{
    public class DepartmentScheduleConfig
    {
        public int Id { get; set; }

        public int OfficeId { get; set; }
        public Office Office { get; set; } = default!;

        public TimeSpan WorkStart { get; set; } = new TimeSpan(9, 0, 0);  // 9:00 AM
        public TimeSpan WorkEnd { get; set; } = new TimeSpan(17, 0, 0);   // 5:00 PM
    }

}
