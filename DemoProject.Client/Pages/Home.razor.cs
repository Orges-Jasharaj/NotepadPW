using DemoProject.Client.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DemoProject.Client.Pages
{
    public partial class Home
    {
        [Inject]
        public AuthService AuthService { get; set; } = default!;
        [Inject]
        protected IJSRuntime JSRuntime { get; set; } = default!;


        protected override async Task OnInitializedAsync()
        {


            var result = await AuthService.GetAuth();

            await JSRuntime.InvokeVoidAsync("alert", result);
            await base.OnInitializedAsync();
        }
    }
}
