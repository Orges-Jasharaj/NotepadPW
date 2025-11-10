using BCrypt.Net;
using DemoProject.API.Data;
using DemoProject.API.Data.Models;
using DemoProject.API.Services.Interface;
using DemoProject.DataModels.Dto.Request;
using DemoProject.DataModels.Dto.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Text;
using System.Text.RegularExpressions;

namespace DemoProject.API.Services.Implementation
{
    public class NoteService : INoteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NoteService> _logger;
        private readonly IClaimsService _claimsService;
        private readonly IChatClient _chatClient;
        private readonly IPdfService _pdfService;


        public NoteService(ApplicationDbContext context, ILogger<NoteService> logger, IClaimsService claimsService, IChatClient chatClient, IPdfService pdfService)
        {
            _context = context;
            _logger = logger;
            _claimsService = claimsService;
            _chatClient = chatClient;
            _pdfService = pdfService;
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

            if (existingNote.IsSecure)
            {
                if (existingNote.UserId == userId)
                {
                    existingNote.Content = createNoteRequestDto.Content;
                    existingNote.UpdatedAt = DateTime.UtcNow;
                    existingNote.ShortDescription = GenerateShortDescription(createNoteRequestDto.Content);
                    _context.Notes.Update(existingNote);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Note updated with URL: {Url}", createNoteRequestDto.Url);
                    return ResponseDto<bool>.SuccessResponse(true, "Note updated successfully.");
                }
                else if (await VerifyPassword(existingNote.PasswordHash, createNoteRequestDto.Password))
                {

                    existingNote.Content = createNoteRequestDto.Content;
                    existingNote.UpdatedAt = DateTime.UtcNow;
                    existingNote.ShortDescription = GenerateShortDescription(createNoteRequestDto.Content);
                    _context.Notes.Update(existingNote);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Note updated with URL: {Url}", createNoteRequestDto.Url);
                    return ResponseDto<bool>.SuccessResponse(true, "Note updated successfully.");
                }
                else
                {
                    return ResponseDto<bool>.Failure("Cannot update secure note without proper authorization.");
                }

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
                else if (!string.IsNullOrEmpty(getNoteByUrlRequestDto.Passwrod))
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

        public async Task<bool> VerifyPassword(string? hashedPassword, string? password)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(password))
            {
                return false;
            }
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public async Task<ResponseDto<string>> SummarizeNoteAsync(string url)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(x => x.Url == url);

            if (note == null || string.IsNullOrEmpty(note.Content))
            {
                return ResponseDto<string>.Failure("");
            }

            string plainText = Regex.Replace(note.Content, "<.*?>", string.Empty);
            plainText = System.Net.WebUtility.HtmlDecode(plainText).Trim();

            string systemPrompt = @"
You are a professional writing assistant that summarizes notes into concise, clear, and informative short descriptions.
Focus on main idea and key details. Keep summary under 30 words.
Do not add extra commentary — only return the summary text.";

            var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, systemPrompt),
            new ChatMessage(ChatRole.User, plainText)
        };

            var responseText = "";
            await foreach (var update in _chatClient.GetStreamingResponseAsync(messages))
            {
                var chunk = $"data: {update.Text}\n\n";
                responseText += update.Text;
            }

            return ResponseDto<string>.SuccessResponse(responseText);
        }

        public async Task<byte[]> GeneratePdf(string url)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(x => x.Url == url);

            if(note is null)
            {
                return null;    
            }

            var pdfBytes = _pdfService.GeneratePdf(note.Content, note.Url);
            return pdfBytes;
        }
    }
}
