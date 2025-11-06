using DemoProject.DataModels.Dto.Request;
using DemoProject.DataModels.Dto.Response;

namespace DemoProject.API.Services.Interface
{
    public interface IUserService
    {
        Task<ResponseDto<bool>> ChangeUserPassword(ChangePasswordRequestDto changePasswordDto);



    }
}
