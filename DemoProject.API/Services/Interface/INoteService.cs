using DemoProject.DataModels.Dto.Request;
using DemoProject.DataModels.Dto.Response;

namespace DemoProject.API.Services.Interface
{
    public interface INoteService
    {
        Task<ResponseDto<bool>> CreateNote(CreateNoteRequestDto createNoteRequestDto);
        Task<ResponseDto<GetNoteByUrlDto>> GetNoteByUrl(GetNoteByUrlRequestDto getNoteByUrlRequestDto);
        Task<ResponseDto<bool>> ChangeUrl(ChangeUrlRequestDto changeUrlDto);
        Task<ResponseDto<GetUserNoteResponseDto>> GetUserNotes();
        Task<ResponseDto<bool>> SetPassword(SetPasswordDto password);
        Task<ResponseDto<string>> SummarizeNoteAsync(string url);
        Task<byte[]> GeneratePdf(string url);

    }
}
