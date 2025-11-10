using DemoProject.Client.Service;
using DemoProject.DataModels.Dto.Request;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;

namespace DemoProject.Client.Pages.Auth
{
    public partial class Register
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


        public async Task RegisterUser(EditContext editContext)
        {
            var data = new CreateUserRequestDto
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                DateOfBirth = Input.DateofBirth,
                Email = Input.Email,
                Password = Input.Password,
            };
            var result = await AuthService.RegisterAsync(data);
            if (result.Success)
            {
                NavigationManager.NavigateTo("/auth/RegisterConfirmation");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    errorMessage += $"{error.Message}<br />";
                }
            }
        }
        private sealed class InputModel
        {

            [Required]
            [Display(Name = "FirstName")]
            public string FirstName { get; set; } = "";

            [Required]
            [Display(Name = "LastName")]
            public string LastName { get; set; } = "";

            [Required]
            [Display(Name = "DateofBirth")]
            public DateTime DateofBirth { get; set; } = DateTime.Now;

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = "";

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = "";
        }

    }
}
