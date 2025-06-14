using MedicalBookingService.Shared.Models;
using System.Net.Http.Json;

namespace MedicalBookingService.Client.Services
{
    public class AuthClientService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        public async Task<UserInfo?> GetCurrentUserAsync()
        {
            return await _http.GetFromJsonAsync<UserInfo>("/api/auth/me");
        }
    }
}
