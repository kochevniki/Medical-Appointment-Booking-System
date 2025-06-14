using MedicalBookingService.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using System.Net.Http;

namespace MedicalBookingService.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // Register the root component (App.razor)
            builder.RootComponents.Add<Components.App>("#app");

            // Register our custom HTTP message handler for authentication as a scoped service
            builder.Services.AddScoped<AuthenticationHandler>();

            // Configure a named HttpClient that targets your server API
            builder.Services.AddHttpClient("ServerApi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7136");
            })
            .AddHttpMessageHandler<AuthenticationHandler>();

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerApi"));

            builder.Services.AddSingleton<LoadingService>();
            // Register Radzen services if you use Radzen components
            builder.Services.AddScoped<DialogService>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<TooltipService>();
            builder.Services.AddScoped<ContextMenuService>();

            // Ensure AuthClientService also uses the HttpClient configured above
            builder.Services.AddScoped<AuthClientService>();

            await builder.Build().RunAsync();
        }
    }
}