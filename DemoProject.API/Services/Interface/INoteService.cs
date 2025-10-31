using DemoProject.DataModels.Dto.Request;
using DemoProject.DataModels.Dto.Response;

namespace DemoProject.API.Services.Interface
{
    public interface INoteService
    {
        Task<ResponseDto<bool>> CreateNote(CreateNoteRequestDto createNoteRequestDto);
        Task<ResponseDto<string>> GetNoteByUrl(string url);
        Task<ResponseDto<bool>> ChangeUrl(ChangeUrlRequestDto changeUrlDto);

    }
}
