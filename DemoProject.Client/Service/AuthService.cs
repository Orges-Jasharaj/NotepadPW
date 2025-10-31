using Blazored.LocalStorage;
using DemoProject.DataModels.Dto.Request;
using DemoProject.DataModels.Dto.Response;
using System.Net.Http.Json;

namespace DemoProject.Client.Service
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILocalStorageService _localStorageService;

        public AuthService(IHttpClientFactory httpClientFactory, ILocalStorageService localStorageService)
        {
            _httpClientFactory = httpClientFactory;
            _localStorageService = localStorageService;
        }

        public async Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto loginDto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthApi");

                var response = await client.PostAsJsonAsync("login", loginDto);
                response.EnsureSuccessStatusCode();

                var loginResponse = await response.Content.ReadFromJsonAsync<ResponseDto<LoginResponseDto>>();
                return loginResponse;
            }
            catch (Exception ex)
            {
                return ResponseDto<LoginResponseDto>.Failure("An error occurred while logging in: " + ex.Message);
            }
        }

        public async Task<ResponseDto<bool>> RegisterAsync(CreateUserRequestDto createUserDto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthApi");

                var response = await client.PostAsJsonAsync("", createUserDto);
                response.EnsureSuccessStatusCode();

                var createResponse = await response.Content.ReadFromJsonAsync<ResponseDto<bool>>();
                return createResponse;
            }
            catch (Exception ex)
            {
                return ResponseDto<bool>.Failure("An error occurred while creating User" + ex.Message);
            }
        }
        public async Task<bool> GetAuth()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthApi");
                var response = await client.GetAsync("");
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}