using DemoProject.DataModels.Dto.Request;
using DemoProject.DataModels.Dto.Response;
using System.Net.Http.Json;
using System.Text.Json;

namespace DemoProject.Client.Service
{
    public class NoteService
    {
        private readonly IHttpClientFactory _httpClientFactory;


        public NoteService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetNoteByUrlDto?> GetNoteByUrl(string url,string password = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("NoteApi");
                var body = new GetNoteByUrlRequestDto
                {
                    Url = url,
                    Passwrod = password
                };

                var response = await client.PostAsJsonAsync("GetNoteByUrl", body);

                response.EnsureSuccessStatusCode();

                var noteResponse = await response.Content.ReadFromJsonAsync<ResponseDto<GetNoteByUrlDto>>();

                //var note = JsonSerializer.Deserialize<ResponseDto<string>>(noteResponse);

                if (noteResponse.Success)
                {
                    return noteResponse.Data;
                }
                return null;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> ChangeUrlAsync(string oldUrl, string newUrl)
        {
            try
            {
                var dto = new ChangeUrlRequestDto
                {
                    OldUrl = oldUrl,
                    NewUrl = newUrl
                };
                var client = _httpClientFactory.CreateClient("NoteApi");
                var response = await client.PutAsJsonAsync("", dto);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadFromJsonAsync<ResponseDto<bool>>();
                if (responseBody.Success)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<bool> CreateOrEditNoteAsync(string url, string content,string password)
        {
            try
            {
                var dto = new CreateNoteRequestDto
                {
                    Url = url,
                    Content = content,
                    Password = password
                };
                var client = _httpClientFactory.CreateClient("NoteApi");



                var response = await client.PostAsJsonAsync("", dto);

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadFromJsonAsync<ResponseDto<bool>>();

                if (responseBody.Success)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<GetUserNoteResponseDto> GetUserNotes()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("NoteApi");



                var response = await client.GetAsync("GetUserNotes");

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadFromJsonAsync<ResponseDto<GetUserNoteResponseDto>>();

                if (responseBody.Success)
                {
                    return responseBody.Data;
                }
                return new GetUserNoteResponseDto();
            }
            catch (Exception ex)
            {
                return new GetUserNoteResponseDto();
            }
        }

        public async Task<bool> SetPassword(string url, string password, bool proctecme=false)
        {

            try
            {
                var dto = new SetPasswordDto
                {
                    Url = url,
                    Password = password,
                    ProtectForMe = proctecme
                };
                var client = _httpClientFactory.CreateClient("NoteApi");
                var response = await client.PostAsJsonAsync("SetPassword", dto);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadFromJsonAsync<ResponseDto<bool>>();
                if (responseBody.Success)
                {
                    return true;
                }
                return false;


            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
