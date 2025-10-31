using DemoProject.API.Services.Interface;
using DemoProject.DataModels.Dto.Request;
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



        [HttpGet("{url}")]
        public async Task<IActionResult> GetNoteByUrl(string url)
        {
            return Ok(await _noteService.GetNoteByUrl(url));
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


    }
}
