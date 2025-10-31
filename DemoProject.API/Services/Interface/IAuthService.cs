using DemoProject.DataModels.Dto.Request;
using DemoProject.DataModels.Dto.Response;

namespace DemoProject.API.Services.Interface
{
    public interface IAuthService
    {
        Task<ResponseDto<bool>> CreateUser(CreateUserRequestDto createUserDto);

        Task<ResponseDto<LoginResponseDto>> LoginUser(LoginRequestDto loginDto);

        Task<ResponseDto<LoginResponseDto>> RefreshToken(RefreshTokenRequestDto refreshTokenDto);
    }
}
