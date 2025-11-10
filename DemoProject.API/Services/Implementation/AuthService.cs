using Azure.Core;
using DemoProject.API.Data.Models;
using DemoProject.API.Services.Interface;
using DemoProject.DataModels;
using DemoProject.DataModels.Dto.Request;
using DemoProject.DataModels.Dto.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace DemoProject.API.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailService;


        public AuthService(UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, IUserEmailStore<ApplicationUser> emailStore, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, ILogger<AuthService> logger, IConfiguration configuration, IEmailSender emailService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = emailStore;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _logger = logger;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<ResponseDto<bool>> ConfirmEmail(ConfirmEmailCommandDto request)
        {
            if (request.UserId is null || request.Code is null)
            {
                return ResponseDto<bool>.Failure("UserId and Code are required");
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
            {
                return ResponseDto<bool>.Failure("User not found");
            }
            else
            {

                var result = await _userManager.ConfirmEmailAsync(user, request.Code);
                return result.Succeeded
                    ? ResponseDto<bool>.SuccessResponse(true, "Email confirmed successfully")
                    : ResponseDto<bool>.Failure("Error confirming email");
            }
        }

        public async Task<ResponseDto<bool>> CreateUser(CreateUserRequestDto createUserDto)
        {
            try
            {
                var userExisits = await _userManager.FindByEmailAsync(createUserDto.Email);
                if (userExisits != null)
                {
                    return ResponseDto<bool>.Failure("User already exists");
                }
                var user = new ApplicationUser
                {
                    FirstName = createUserDto.FirstName,
                    LastName = createUserDto.LastName,
                    DateOfBirth = createUserDto.DateOfBirth
                };
                await _userStore.SetUserNameAsync(user, createUserDto.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, createUserDto.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, createUserDto.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, RolesTypes.User);
                    
                    _logger.LogInformation("User created successfully with email: {Email}", createUserDto.Email);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));


                    var clientBaseUrl = _configuration["ClientApp:BaseUrl"];
                    var confimPath = "auth/ConfirmEmail";
                    var confimUri = new Uri(new Uri(clientBaseUrl), confimPath).ToString();
                    var callbackUrl = QueryHelpers.AddQueryString(confimUri, new Dictionary<string, string>
      {
          { "userId", user.Id },
          { "code",code }
      });

                    await _emailService.SendAccountConfirmationEmailAsync(user.Email, callbackUrl);

                    return ResponseDto<bool>.SuccessResponse(true, "User created successfully");
                }
                var errors = result.Errors.Select(e => new ApiError
                {
                    Code = e.Code,
                    Message = e.Description
                }).ToList();


                return ResponseDto<bool>.Failure("User creation failed", errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating user with email: {Email}", createUserDto.Email);
                return ResponseDto<bool>.Failure("An error occurred while creating user");
            }
        }

        public async Task<ResponseDto<bool>> ForgotPassword(ForgotPasswordCommandDto forgotPasswordCommandDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordCommandDto.Email);
            if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return ResponseDto<bool>.SuccessResponse(true);
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var clientBaseUrl = _configuration["ClientApp:BaseUrl"];
            var resetPath = "auth/ResetPassword";
            var resetUri = new Uri(new Uri(clientBaseUrl), resetPath).ToString();
            var callbackUrl = QueryHelpers.AddQueryString(resetUri, new Dictionary<string, string>
                {
                    { "code", code },
                    { "email", user.Email }
                });

            await _emailService.SendForgotEmailAsync(user.Email, HtmlEncoder.Default.Encode(callbackUrl));

            return ResponseDto<bool>.SuccessResponse(true);

        }

        public async Task<ResponseDto<LoginResponseDto>> LoginUser(LoginRequestDto loginDto)
        {
            var userexitist = await _userManager.FindByEmailAsync(loginDto.Email);
            if (userexitist == null)
            {
                return ResponseDto<LoginResponseDto>.Failure("User does not exist");
            }
            var result = await _signInManager.PasswordSignInAsync(userexitist, loginDto.Password, false, false);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(userexitist);


                var token = _tokenService.GenerateAccessToken(userexitist, roles.ToList());
                var refreshToken = _tokenService.GenerateRrefreshToken();
                userexitist.RefreshToken = refreshToken.RefreshToken;
                userexitist.RefreshTokenExpiryTime = refreshToken.RefreshTokenExipirityDate;

                await _userManager.UpdateAsync(userexitist);

                var loginResponse = new LoginResponseDto
                {
                    DisplayName = $"{userexitist.FirstName} {userexitist.LastName}",
                    Email = userexitist.Email,
                    AccessToken = token,
                    RefreshToken = refreshToken.RefreshToken,
                    RefreshTokenExpiryTime = refreshToken.RefreshTokenExipirityDate
                };
                return ResponseDto<LoginResponseDto>.SuccessResponse(loginResponse, "Login successful");
            }

            return ResponseDto<LoginResponseDto>.Failure("Login failed, please check your credentials");
        }

        public async Task<ResponseDto<LoginResponseDto>> RefreshToken(RefreshTokenRequestDto refreshTokenDto)
        {
            var claimPrincipal = _tokenService.GetClaimsPrincipal(refreshTokenDto.AccessToken);
            if (claimPrincipal == null)
            {
                return ResponseDto<LoginResponseDto>.Failure("Invalid access token");
            }

            var userId = claimPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user.RefreshToken != refreshTokenDto.RefreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                return ResponseDto<LoginResponseDto>.Failure("Invalid or expired refresh token");
            }

            if (user == null)
            {
                return ResponseDto<LoginResponseDto>.Failure("User does not exist");
            }

            var roles = await _userManager.GetRolesAsync(user);


            var token = _tokenService.GenerateAccessToken(user, roles.ToList());
            var refreshToken = _tokenService.GenerateRrefreshToken();
            user.RefreshToken = refreshToken.RefreshToken;
            user.RefreshTokenExpiryTime = refreshToken.RefreshTokenExipirityDate;

            await _userManager.UpdateAsync(user);

            var loginResponse = new LoginResponseDto
            {
                DisplayName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                AccessToken = token,
                RefreshToken = refreshToken.RefreshToken,
                RefreshTokenExpiryTime = refreshToken.RefreshTokenExipirityDate
            };
            return ResponseDto<LoginResponseDto>.SuccessResponse(loginResponse, "Login successful");
        }

        public async Task<ResponseDto<bool>> ResetPassword(ResetPasswordCommandDto resetPasswordCommandDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordCommandDto.Email);
            if (user is null)
            {
                // Don't reveal that the user does not exist
                return ResponseDto<bool>.SuccessResponse(true);
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordCommandDto.Code, resetPasswordCommandDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new ApiError
                {
                    Code = e.Code,
                    Message = e.Description
                }).ToList();
                return ResponseDto<bool>.Failure("Password reset failed", errors);
            }
            return ResponseDto<bool>.SuccessResponse(true);
        }
    }
}
