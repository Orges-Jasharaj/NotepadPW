using DemoProject.API.Services.Interface;
using DemoProject.DataModels.Dto.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DemoProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NotesController(INoteService noteService)
        {
            _noteService = noteService;
        }



        [HttpPost("GetNoteByUrl")]
        public async Task<IActionResult> GetNoteByUrl([FromBody] GetNoteByUrlRequestDto dto)
        {
            return Ok(await _noteService.GetNoteByUrl(dto));
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteRequestDto createNoteRequestDto)
        {
            return Ok(await _noteService.CreateNote(createNoteRequestDto));
        }

        [HttpPut]
        public async Task<IActionResult> ChangeUrl([FromBody] ChangeUrlRequestDto changeUrlDto)
        {
            return Ok(await _noteService.ChangeUrl(changeUrlDto));
        }

        [ Authorize]
        [HttpGet("GetUserNotes")]
        public async Task<IActionResult> GetUserNotes()
        {
            return Ok(await _noteService.GetUserNotes());
        }

        [HttpPost("SetPassword")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto setPassword)
        {
            return Ok(await _noteService.SetPassword(setPassword));
        }

        [Authorize]
        [HttpGet("Summarize/{url}")]
        public async Task<IActionResult> SumurizeNoteStream(string url)
        {

          return Ok( await _noteService.SummarizeNoteAsync(url));
        }

        [Authorize]
        [HttpGet("GeneratePdf/{url}")]
        public async Task<IActionResult> GeneratePdf(string url)
        {
            var pdfStream = await _noteService.GeneratePdf(url);
            if (pdfStream == null)
            {
                return NotFound();
            }
            return File(pdfStream, "application/pdf", $"{url}.pdf");
        }



    }
}
