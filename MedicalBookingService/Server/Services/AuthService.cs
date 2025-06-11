using MedicalBookingService.Server.Data;
using MedicalBookingService.Server.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedicalBookingService.Server.Services
{
    public class AuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<AppUser> userManager, ApplicationDbContext context, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<AppUser?> GetUserByIdAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.PatientProfile)
                .Include(u => u.DoctorProfile)
                .Include(u => u.AdminProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<string>> GetUserRolesAsync(AppUser user)
        {
            return (await _userManager.GetRolesAsync(user)).ToList();
        }
    }
}
