// MedicalBookingService.Client/Services/AuthenticationHandler.cs
using Microsoft.AspNetCore.Components; // Required for NavigationManager
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net; // Required for HttpStatusCode
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalBookingService.Client.Services
{
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly NavigationManager _navigationManager;
        private readonly LoadingService _loadingService;

        // Constructor to inject NavigationManager, which allows programmatic navigation
        public AuthenticationHandler(NavigationManager navigationManager, LoadingService loadingService)
        {
            _navigationManager = navigationManager;
            _loadingService = loadingService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                _loadingService.Show(); // Show loading spinner before request

                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                var response = await base.SendAsync(request, cancellationToken);

                // Redirect if authentication fails
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    var currentUri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
                    if (!currentUri.LocalPath.EndsWith("/login", StringComparison.OrdinalIgnoreCase) &&
                        !request.RequestUri.LocalPath.Contains("/api/auth/login", StringComparison.OrdinalIgnoreCase))
                    {
                        _navigationManager.NavigateTo("/login", forceLoad: true);
                    }
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                throw;
            }
            finally
            {
                _loadingService.Hide(); // Hide loading spinner after request completes
            }
        }
    }
}
