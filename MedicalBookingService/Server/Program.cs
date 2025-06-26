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
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                // Optional: Configure Identity password/user requirements here
                options.SignIn.RequireConfirmedAccount = false; // Example: Don't require email confirmation for sign-in
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure Application Cookie to return 401/403 for API requests instead of redirecting
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true; // Cookie is only accessible by the server
                options.ExpireTimeSpan = TimeSpan.FromDays(7); // Cookie expires in 7 days
                options.SlidingExpiration = true; // Renew cookie if it's nearing expiration

                // Event to handle redirection when an unauthenticated user tries to access an authorized resource
                options.Events.OnRedirectToLogin = context =>
                {
                    // Check if the request is for an API endpoint
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized; // Return 401 Unauthorized
                        // Clear the Location header to prevent browser redirection, important for SPA APIs
                        context.Response.Headers["Location"] = "/";
                        return Task.CompletedTask;
                    }
                    // For non-API requests (e.g., if you had Razor Pages, it would redirect to the configured login path)
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };

                // Event to handle redirection when an authorized user tries to access a forbidden resource
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden; // Return 403 Forbidden
                        context.Response.Headers["Location"] = "/"; // Clear the Location header
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            // Configure CORS policy for the frontend client
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("https://localhost:8080") // Specify the exact origin of your Blazor client
                          .AllowAnyHeader() // Allow all headers
                          .AllowAnyMethod() // Allow all HTTP methods (GET, POST, PUT, DELETE, etc.)
                          .AllowCredentials(); // IMPORTANT: This allows cookies to be sent with cross-origin requests
                });
            });

            // Register application services
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<FileService>();

            // Allow API controllers to be discovered and mapped
            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure Middleware Pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error"); // Centralized error handling
                app.UseHsts(); // Enforce HTTPS for a specified duration
            }

            app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS (should be early in the pipeline)
            app.UseCors("AllowFrontend"); // Enable CORS policy (must be before UseAuthentication/UseAuthorization)
            app.UseAuthentication();    // Identifies who the user is (must be before UseAuthorization)
            app.UseAuthorization();     // Determines if the user can access a resource

            app.MapControllers(); // Maps API endpoints to their respective controllers




            //using (var scope = app.Services.CreateScope())
            //{
            //    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            //    // Optional: ensure DB is created
            //    context.Database.EnsureCreated();

            //    // Check if data already exists
            //    if (!context.BoxTokens.Any())
            //    {
            //        var token = new BoxToken
            //        {
            //            RefreshToken = null,
            //            AccessToken = "a9NheJ4hNHoBSLc1jDD86CzsqKKKrJin",
            //            UpdatedAt = new DateTime(2025, 6, 25, 21, 45, 0)
            //        };

            //        context.BoxTokens.Add(token);
            //        context.SaveChanges();
            //    }
            //}





            app.Run(); // Runs the application
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
