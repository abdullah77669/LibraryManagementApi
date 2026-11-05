using LMS_APIs.Models;
using LMS_APIs.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUser _userService;

        public UsersController(IUser userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var result = await _userService.LoginAsync(request);
            if (result == null)
                return Unauthorized("Invalid UserID or password.");

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<Borrowers>> Register(RegisterRequest request)
        {
            var result = await _userService.RegisterAsync(request);
            if (!result.Succeeded)
                return StatusCode(result.ErrorStatus, result.Error);

            return StatusCode(StatusCodes.Status201Created, result.Value);
        }
    }
}
