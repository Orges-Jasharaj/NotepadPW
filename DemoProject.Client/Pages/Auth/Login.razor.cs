using Blazored.LocalStorage;
using DemoProject.Client.Service;
using DemoProject.DataModels.Dto.Request;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;

namespace DemoProject.Client.Pages.Auth
{

    public partial class Login
    {
        private string? errorMessage;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        [Inject]
        public AuthService AuthService { get; set; } = default!;
        [Inject]
        protected IJSRuntime JSRuntime { get; set; } = default!;
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        private ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject]
        private JwtAuthenticationStateProvider JwtAuthenticationStateProvider { get; set; } = default!;


        private string? returnUrl;


        protected override Task OnInitializedAsync()
        {
            var uri = new Uri(NavigationManager.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            returnUrl = query["returnUrl"];
            return base.OnInitializedAsync();
        }
        public async Task LoginUser()
        {

            var data = new LoginRequestDto
            {
                Email = Input.Email,
                Password = Input.Password
            };

            var result = await AuthService.LoginAsync(data);
            if (result.Success)
            {
                await LocalStorage.SetItemAsync("authToken", result.Data.AccessToken);
                await LocalStorage.SetItemAsync("refreshToken", result.Data.RefreshToken);

                // Notify the CustomAuthStateProvider
                JwtAuthenticationStateProvider.NotifyUserAuthentication(result.Data.AccessToken);
                NavigationManager.NavigateTo(returnUrl ?? "/");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", result.Errors.Select(x => x.Message).FirstOrDefault());
            }
        }
        private sealed class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";
        }
    }
}
