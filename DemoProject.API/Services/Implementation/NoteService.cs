using DemoProject.API.Data;
using DemoProject.API.Data.Models;
using DemoProject.API.Services.Interface;
using DemoProject.DataModels.Dto.Request;
using DemoProject.DataModels.Dto.Response;
using Microsoft.EntityFrameworkCore;

namespace DemoProject.API.Services.Implementation
{
    public class NoteService : INoteService
    {
        private readonly ApplicationDbContext _context;


        public NoteService(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public async Task<ResponseDto<bool>> CreateNote(CreateNoteRequestDto createNoteRequestDto)
        {
            var existingNote = await _context.Notes
                .Where(n => n.Url == createNoteRequestDto.Url).FirstOrDefaultAsync();

            if (existingNote == null)
            {
                var newNote = new Note
                {
                    Content = createNoteRequestDto.Content,
                    Url = createNoteRequestDto.Url,
                    IsSecure = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notes.Add(newNote);
                await _context.SaveChangesAsync();
                return ResponseDto<bool>.SuccessResponse(true, "Note created successfully."); 
            }
            existingNote.Content = createNoteRequestDto.Content;
            existingNote.UpdatedAt = DateTime.UtcNow;

            _context.Notes.Update(existingNote);
            await _context.SaveChangesAsync();
            return ResponseDto<bool>.SuccessResponse(true, "Note updated successfully.");
        }

        public async Task<ResponseDto<string>> GetNoteByUrl(string url)
        {
            var existingNote = await _context.Notes
                .Where(n => n.Url == url).FirstOrDefaultAsync();
            if(existingNote == null)
            {
                return ResponseDto<string>.Failure("Note not found.");
            }
            return ResponseDto<string>.SuccessResponse(existingNote.Content, "Note retrieved successfully.");
        }

        public async Task<ResponseDto<bool>> ChangeUrl(ChangeUrlRequestDto changeUrlDto)
        {
            var existingNote = await _context.Notes
                .Where(n => n.Url == changeUrlDto.OldUrl).FirstOrDefaultAsync();

            if (existingNote == null)
            {
                return ResponseDto<bool>.Failure("Note with the old URL not found.");
            }

            var checkurl = await _context.Notes
                .Where(n => n.Url == changeUrlDto.NewUrl).FirstOrDefaultAsync();
            if (checkurl != null)
            {
                 return ResponseDto<bool>.Failure("The new URL is already in use. Please choose a different URL.");
            }
            existingNote.Url = changeUrlDto.NewUrl;
            existingNote.UpdatedAt = DateTime.UtcNow;

            _context.Notes.Update(existingNote);

            await _context.SaveChangesAsync();
            return ResponseDto<bool>.SuccessResponse(true, "URL changed successfully.");
        }

    }
}
