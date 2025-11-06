using DemoProject.API.Services.Interface;
using DemoProject.DataModels.Dto.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DemoProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ManageController : ControllerBase
    {
        private readonly IManageService _userService;
        public ManageController(IManageService userService)
        {
            _userService = userService;
        }

        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordDto)
        {
          
            return Ok(await _userService.ChangeUserPassword(changePasswordDto));
        }
    }
}
