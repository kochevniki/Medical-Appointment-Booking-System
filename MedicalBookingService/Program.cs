using MedicalBookingService.Client.Components;
using MedicalBookingService.Server.Data; // Your DbContext namespace
using MedicalBookingService.Server.Models; // Your user model namespace
using MedicalBookingService.Server.Services;
using MedicalBookingService.Shared.Constants;
using MedicalBookingService.Shared.Services;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using System.Threading.Tasks;

namespace Medical_Appointment_Booking_System
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(1);
            });

            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<FileService>();

            builder.Services.Configure<CircuitOptions>(options =>
            {
                options.DetailedErrors = true;
            });


            builder.Services.AddControllers(); // <-- This registers API controllers
            builder.Services.AddHttpClient();
            builder.Services.AddRadzenComponents();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Login";
                options.AccessDeniedPath = "/AccessDenied";
                options.Cookie.HttpOnly = true;
            });

            var app = builder.Build();

            //using (var scope = app.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;
            //    var context = services.GetRequiredService<ApplicationDbContext>();
            //    var userManager = services.GetRequiredService<UserManager<AppUser>>();
            //    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            //    // Ensure DB is up to date
            //    context.Database.Migrate();

            //    await SeedRolesAsync(roleManager);
            //    await SeedDepartmentsAsync(context, userManager, roleManager);
            //}

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.UseStaticFiles();

            app.MapStaticAssets();

            //app.Use(async (context, next) =>
            //{
            //    foreach (var cookie in context.Request.Cookies.Keys)
            //    {
            //        context.Response.Cookies.Delete(cookie);
            //    }

            //    await next();
            //});


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapControllers(); // <-- This maps the controller routes

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
