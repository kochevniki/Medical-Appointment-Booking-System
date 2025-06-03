using MedicalBookingService.Server.Data;
using MedicalBookingService.Server.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedicalBookingService.Shared.Services
{
    public class AuthService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        public bool IsInitialized { get; private set; } = false;
        public event Action? OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();
        public bool IsAuthenticated { get; private set; }
        public AppUser? CurrentUser { get; private set; }
        public List<string> UserRoles { get; private set; } = new();
        private readonly ILogger<AuthService> _logger;

        public AuthService(AuthenticationStateProvider authStateProvider, UserManager<AppUser> userManager, ILogger<AuthService> logger, ApplicationDbContext context)
        {
            _authStateProvider = authStateProvider;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        public async Task InitializeAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            IsAuthenticated = user.Identity?.IsAuthenticated ?? false;

            if (IsAuthenticated)
            {
                var userId = _userManager.GetUserId(user);

                if (!string.IsNullOrEmpty(userId))
                {
                    //CurrentUser = await _userManager.FindByIdAsync(userId);
                    CurrentUser = await _context.Users
                      .Include(u => u.PatientProfile)
                      .Include(u => u.DoctorProfile)
                      .Include(u => u.AdminProfile)
                      .FirstOrDefaultAsync(u => u.Id == userId);
                }

                if (CurrentUser != null)
                {
                    UserRoles = (await _userManager.GetRolesAsync(CurrentUser)).ToList();
                }
            }
            IsInitialized = true;
            NotifyStateChanged();
        }
    }

}
