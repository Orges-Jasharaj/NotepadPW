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

        public async Task<string> GetNoteByUrl(string url)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("NoteApi");

                var response = await client.GetAsync($"{url}");

                response.EnsureSuccessStatusCode();

                var noteResponse = await response.Content.ReadFromJsonAsync<ResponseDto<string>>();

                //var note = JsonSerializer.Deserialize<ResponseDto<string>>(noteResponse);

                if(noteResponse.Success)
                {
                    return noteResponse.Data;
                }
                return string.Empty;
                
            }
            catch(Exception ex)
            {
                return string.Empty;
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


        public async Task<bool> CreateOrEditNoteAsync(string url, string content)
        {
            try
            {
                var dto = new CreateNoteRequestDto
                {
                    Url = url,
                    Content = content
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


    }
}
