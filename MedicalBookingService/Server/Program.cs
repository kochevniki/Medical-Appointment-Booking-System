using MedicalBookingService.Server.Data;
using MedicalBookingService.Server.Models;
using MedicalBookingService.Server.Services;
using MedicalBookingService.Shared.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedicalBookingService.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load configuration from appsettings.json
            var configuration = builder.Configuration;

            // Add Database (SQL Server)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Configure Identity for authentication
            builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("https://localhost:8080") // Change for production!
                          .AllowCredentials()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Register application services
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<FileService>();

            // Allow API controllers
            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure Middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseCors("AllowFrontend");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();  // Maps API endpoints
            app.Run();
        }

        static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "Doctor", "Patient" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        static async Task SeedDepartmentsAsync(ApplicationDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var departments = new[] { "Primary Care", "Pediatrics", "Dental", "Physical Therapy", "Eye Exam" };
            var random = new Random();
            var workStart = new TimeSpan(9, 0, 0); // 9:00 AM
            var workEnd = new TimeSpan(17, 0, 0);  // 5:00 PM

            foreach (var dept in departments)
            {
                // Create Office if not exists
                var office = await context.Offices.FirstOrDefaultAsync(o => o.Name == dept);
                if (office == null)
                {
                    office = new Office { Name = dept };
                    context.Offices.Add(office);
                    await context.SaveChangesAsync();
                }

                // Create DepartmentScheduleConfig if not exists
                var existingSchedule = await context.ScheduleConfigs.FirstOrDefaultAsync(s => s.OfficeId == office.Id);
                if (existingSchedule == null)
                {
                    var schedule = new DepartmentScheduleConfig
                    {
                        OfficeId = office.Id,
                        WorkStart = workStart,
                        WorkEnd = workEnd
                    };
                    context.ScheduleConfigs.Add(schedule);
                    await context.SaveChangesAsync();
                }

                // Admin
                var adminEmail = $"{dept.Replace(" ", "").ToLower()}admin@clinic.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var adminUser = new AppUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        Role = "Admin",
                        OfficeId = office.Id
                    };

                    await userManager.CreateAsync(adminUser, "Admin123!");
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    var adminProfile = new AdminProfile
                    {
                        AppUserId = adminUser.Id,
                        FirstName = "Admin",
                        LastName = dept,
                        OfficeId = office.Id
                    };

                    context.AdminProfiles.Add(adminProfile);
                    await context.SaveChangesAsync();
                }

                // 3 Doctors
                for (int i = 1; i <= 3; i++)
                {
                    var docEmail = $"{dept.Replace(" ", "").ToLower()}doc{i}@clinic.com";
                    if (await userManager.FindByEmailAsync(docEmail) == null)
                    {
                        var docUser = new AppUser
                        {
                            UserName = docEmail,
                            Email = docEmail,
                            EmailConfirmed = true,
                            Role = "Doctor",
                            OfficeId = office.Id
                        };

                        await userManager.CreateAsync(docUser, "Doctor123!");
                        await userManager.AddToRoleAsync(docUser, "Doctor");

                        var doctorProfile = new DoctorProfile
                        {
                            AppUserId = docUser.Id,
                            FirstName = $"Dr{i}",
                            LastName = dept.Replace(" ", ""),
                            Specialty = dept,
                            OfficeId = office.Id
                        };

                        context.DoctorProfiles.Add(doctorProfile);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }


    }
}
