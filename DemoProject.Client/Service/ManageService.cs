using DemoProject.DataModels.Dto.Request;
using DemoProject.DataModels.Dto.Response;
using System.Net.Http.Json;

namespace DemoProject.Client.Service
{
    public class ManageService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ManageService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ResponseDto<bool>> ChangePasswordAsync(ChangePasswordRequestDto changePasswordCommandDto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ManageApi");
                var response = await client.PostAsJsonAsync("changepassword", changePasswordCommandDto);
                response.EnsureSuccessStatusCode();
                var changePasswordResponse = await response.Content.ReadFromJsonAsync<ResponseDto<bool>>();
                return changePasswordResponse;
            }
            catch (Exception ex)
            {
                return ResponseDto<bool>.Failure("An error occurred while changing password: " + ex.Message);
            }
        }


    }
}
