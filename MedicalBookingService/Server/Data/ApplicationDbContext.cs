using MedicalBookingService.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedicalBookingService.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

        public DbSet<Office> Offices { get; set; }
        public DbSet<PatientProfile> PatientProfiles { get; set; }
        public DbSet<DoctorProfile> DoctorProfiles { get; set; }
        public DbSet<AdminProfile> AdminProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PatientProfile>()
                .HasOne(p => p.AppUser)
                .WithOne(u => u.PatientProfile)
                .HasForeignKey<PatientProfile>(p => p.AppUserId);

            builder.Entity<DoctorProfile>()
                .HasOne(p => p.AppUser)
                .WithOne(u => u.DoctorProfile)
                .HasForeignKey<DoctorProfile>(p => p.AppUserId);

            builder.Entity<AdminProfile>()
                .HasOne(p => p.AppUser)
                .WithOne(u => u.AdminProfile)
                .HasForeignKey<AdminProfile>(p => p.AppUserId);

            builder.Entity<AppUser>()
                .HasOne(u => u.Office)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
