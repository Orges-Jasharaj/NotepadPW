using Blazored.LocalStorage;
using DemoProject.Client.Service;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

namespace DemoProject.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");


            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<JwtAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
                provider.GetRequiredService<JwtAuthenticationStateProvider>());

            builder.Services.AddRadzenComponents();
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<AuthTokenHandler>();
            builder.Services.AddScoped<NoteService>(); 
            builder.Services.AddScoped<ManageService>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<NotificationServiceWrapper>();
            builder.Services.AddScoped<DialogService>();

            var baseApiUrl = "http://padzyapi.runasp.net/";
           
            if(builder.HostEnvironment.IsDevelopment())
            {
                baseApiUrl = "https://localhost:7086/";
            }
           
            
            builder.Services.AddHttpClient("AuthApi", client => client.BaseAddress = new Uri($"{baseApiUrl}api/Auth/")).AddHttpMessageHandler<AuthTokenHandler>();
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthApi"));

            builder.Services.AddHttpClient("NoteApi", client => client.BaseAddress = new Uri($"{baseApiUrl}api/Notes/")).AddHttpMessageHandler<AuthTokenHandler>();
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("NoteApi"));

            builder.Services.AddHttpClient("ManageApi", client => client.BaseAddress = new Uri($"{baseApiUrl}api/Manage/")).AddHttpMessageHandler<AuthTokenHandler>();
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ManageApi"));


            await builder.Build().RunAsync();
        }
    }
}
