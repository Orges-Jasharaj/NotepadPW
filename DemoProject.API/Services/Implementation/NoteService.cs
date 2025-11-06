using DemoProject.API.Data;
using DemoProject.API.Data.Models;
using DemoProject.API.Services.Interface;
using DemoProject.DataModels.Dto.Request;
using DemoProject.DataModels.Dto.Response;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Text.RegularExpressions;

namespace DemoProject.API.Services.Implementation
{
    public class NoteService : INoteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NoteService> _logger;
        private readonly IClaimsService _claimsService;


        public NoteService(ApplicationDbContext context, ILogger<NoteService> logger, IClaimsService claimsService)
        {
            _context = context;
            _logger = logger;
            _claimsService = claimsService;
        }


        public async Task<ResponseDto<bool>> CreateNote(CreateNoteRequestDto createNoteRequestDto)
        {
            string userId = _claimsService.GetCurrentUserId();

            var existingNote = await _context.Notes
                .Where(n => n.Url == createNoteRequestDto.Url).FirstOrDefaultAsync();

            if (existingNote == null)
            {
                var newNote = new Note
                {
                    Content = createNoteRequestDto.Content,
                    Url = createNoteRequestDto.Url,
                    IsSecure = false,
                    CreatedAt = DateTime.UtcNow,
                    ShortDescription = GenerateShortDescription(createNoteRequestDto.Content),
                    UserId = userId
                };
                _context.Notes.Add(newNote);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Note created with URL: {Url}", createNoteRequestDto.Url);

                return ResponseDto<bool>.SuccessResponse(true, "Note created successfully.");
            }
            existingNote.Content = createNoteRequestDto.Content;
            existingNote.UpdatedAt = DateTime.UtcNow;
            existingNote.ShortDescription = GenerateShortDescription(createNoteRequestDto.Content);

            _context.Notes.Update(existingNote);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Note updated with URL: {Url}", createNoteRequestDto.Url);
            return ResponseDto<bool>.SuccessResponse(true, "Note updated successfully.");
        }

        //return responsedto from one class that contians string and bool
        public async Task<ResponseDto<GetNoteByUrlDto>> GetNoteByUrl(GetNoteByUrlRequestDto getNoteByUrlRequestDto)
        {
            var userId = _claimsService.GetCurrentUserId();

            var existingNote = await _context.Notes
                .Where(n => n.Url == getNoteByUrlRequestDto.Url).FirstOrDefaultAsync();
            if (existingNote == null)
            {
                return ResponseDto<GetNoteByUrlDto>.Failure("Note not found.");
            }
            if (existingNote.IsSecure)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    if (userId == existingNote.UserId)
                    {
                        return ResponseDto<GetNoteByUrlDto>.SuccessResponse(
                              new GetNoteByUrlDto
                              {
                                  Content = existingNote.Content,
                                  Url = existingNote.Url,
                                  IsSecure = existingNote.IsSecure
                              },
                              "Note retrieved successfully.");
                    }
                }
                else if(!string.IsNullOrEmpty(getNoteByUrlRequestDto.Passwrod))
                {

                    var isPasswordValid = await VerifyPassword(existingNote.PasswordHash, getNoteByUrlRequestDto.Passwrod);
                    if (isPasswordValid)
                    {
                        return ResponseDto<GetNoteByUrlDto>.SuccessResponse(
                           new GetNoteByUrlDto
                           {
                               Content = existingNote.Content,
                               Url = existingNote.Url,
                               IsSecure = existingNote.IsSecure
                           },
                           "Note retrieved successfully.");
                    }


                }
            }
            else
            {
                return ResponseDto<GetNoteByUrlDto>.SuccessResponse(
                   new GetNoteByUrlDto
                   {
                       Content = existingNote.Content,
                       Url = existingNote.Url,
                       IsSecure = existingNote.IsSecure
                   },
                   "Note retrieved successfully.");
            }

            return ResponseDto<GetNoteByUrlDto>.SuccessResponse(
                             new GetNoteByUrlDto
                             {
                                 Content = null,
                                 Url = existingNote.Url,
                                 IsSecure = existingNote.IsSecure
                             },
                             "Note retrieved successfully.");
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
            _logger.LogInformation("Note URL changed from {OldUrl} to {NewUrl}", changeUrlDto.OldUrl, changeUrlDto.NewUrl);
            return ResponseDto<bool>.SuccessResponse(true, "URL changed successfully.");
        }

        private string GenerateShortDescription(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
                return string.Empty;

            // Remove HTML tags
            string plainText = Regex.Replace(htmlContent, "<.*?>", string.Empty);

            // Decode HTML entities (like &amp;, &lt;, etc.)
            plainText = System.Net.WebUtility.HtmlDecode(plainText);

            // Trim whitespace
            plainText = plainText.Trim();

            // Take only first 10 characters
            string shortText = plainText.Length <= 10 ? plainText : plainText.Substring(0, 10);

            // Optionally add "..." if truncated
            if (plainText.Length > 10)
                shortText += "...";

            return shortText;
        }

        public async Task<ResponseDto<GetUserNoteResponseDto>> GetUserNotes()
        {
            var userId = _claimsService.GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return ResponseDto<GetUserNoteResponseDto>.Failure("User not authenticated.");
            }

            var notes = await _context.Notes
                .Where(n => n.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(n => new UserNoteDto
                {
                    Url = n.Url,
                    ShortDescription = n.ShortDescription
                })
                .Take(10)
                .ToListAsync();

            GetUserNoteResponseDto responseDto = new GetUserNoteResponseDto
            {
                UserNoteDtos = notes
            };
            return ResponseDto<GetUserNoteResponseDto>.SuccessResponse(responseDto, "User notes retrieved successfully.");
        }

        public async Task<ResponseDto<bool>> SetPassword(SetPasswordDto setPasswordDto)
        {
            var existNote = await _context.Notes
            .FirstOrDefaultAsync(x => x.Url == setPasswordDto.Url);

            if (existNote == null)
            {
                return ResponseDto<bool>.Failure("Note not found");
            }

    

            if (!string.IsNullOrEmpty(setPasswordDto.Password))
            {
                existNote.PasswordHash = BCrypt.Net.BCrypt.HashPassword(setPasswordDto.Password);
                existNote.IsSecure = true;
            }
            else if (setPasswordDto.ProtectForMe)
            {
                existNote.IsSecure = true;
            }


             _context.Notes.Update(existNote);
            await _context.SaveChangesAsync();

            return ResponseDto<bool>.SuccessResponse(true, "Password set successfully");



        }

        public async Task<bool> VerifyPassword(string hashedPassword, string password)
        {

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

    }
}
