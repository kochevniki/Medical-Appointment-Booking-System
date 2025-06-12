using MedicalBookingService.Client.Components;
using MedicalBookingService.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

namespace MedicalBookingService.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // Register the root component (App.razor)
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7136"),
                DefaultRequestHeaders = { { "Cookie", "include" } } // Ensures cookies are sent
            });

            // Register Radzen services if you use Radzen components
            builder.Services.AddScoped<DialogService>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<TooltipService>();
            builder.Services.AddScoped<ContextMenuService>();

            builder.Services.AddScoped<AuthClientService>();

            await builder.Build().RunAsync();

        }
    }
}