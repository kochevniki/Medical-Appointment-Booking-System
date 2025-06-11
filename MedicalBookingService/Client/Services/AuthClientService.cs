using MedicalBookingService.Shared.Models;
using System.Net.Http.Json;

namespace MedicalBookingService.Client.Services
{
    public class AuthClientService
    {
        private readonly HttpClient _http;

        public AuthClientService(HttpClient http)
        {
            _http = http;
        }

        public async Task<UserInfo?> GetCurrentUserAsync()
        {
            return await _http.GetFromJsonAsync<UserInfo>("/api/auth/me");
        }
    }
}
