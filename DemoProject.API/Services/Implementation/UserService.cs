using DemoProject.API.Data.Models;
using DemoProject.API.Services.Interface;
using DemoProject.DataModels.Dto.Request;
using DemoProject.DataModels.Dto.Response;
using Microsoft.AspNetCore.Identity;

namespace DemoProject.API.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailSender _emailSender;

        public UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger, IEmailSender emailSender)
        {
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;

        }

        public async Task<ResponseDto<bool>> ChangeUserPassword(ChangePasswordRequestDto changePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(changePasswordDto.UserId);
            if (user == null)
            {
                return ResponseDto<bool>.Failure("User does not exist");
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword, changePasswordDto.NewPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password changed successfully for user with ID: {UserId}", changePasswordDto.UserId);
                _emailSender.SendEmail(user.Email, "Password Changed", "Your password has been changed successfully.");
                return ResponseDto<bool>.SuccessResponse(true, "Password changed successfully");
            }
            var errors = result.Errors.Select(e => new ApiError
            {
                Code = e.Code,
                Message = e.Description
            }).ToList();
            return ResponseDto<bool>.Failure("Password change failed", errors);
        }
    }
}
