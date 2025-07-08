using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cleanApi.Modules.Auth
{
    public class AuthController: ControllerBase
    {
        [ApiController]
    [Route("api/[controller]")]
    // public class AuthController : ControllerBase
    // {
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var result = await _userService.RegisterAsync(command);
            
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(new { Message = "User registered successfully", UserId = result.Data!.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _userService.LoginAsync(command);
            
            if (!result.IsSuccess)
                return Unauthorized(result.Error);

            // In real implementation, generate JWT token here
            return Ok(new { Message = "Login successful", UserId = result.Data!.Id });
        }
    
    }
}