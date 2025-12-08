using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerApi.DTO.Auth;
using WorkoutTrackerApi.Services.Interfaces;

namespace WorkoutTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {

            var result = await _authService.RegisterAsync(request);

            if (!result.IsSucceeded)
            {
                return BadRequest(new { errors = result.Errors.ToArray()});
            }

            return Ok(new { message = "Registration completed", data = result.Payload });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            
            if (!result.IsSucceeded)
            {
                return Unauthorized(new { errors = result.Errors.ToArray()});
            }

            return Ok(new { message = "Login completed", data = result.Payload });
            
        }
        
        [Authorize]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "You are authenticated", user = $"{User.Identity!.Name}" });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RotateAuthTokens([FromBody] TokenRequestDto request)
        {
            var result = await _authService.RotateAuthTokens(request.RefreshToken);

            if (!result.IsSucceeded)
            {
                foreach (var error in result.Errors)
                {
                    if (error.Code == "User.NotFound")
                        return NotFound(error.Description + request.RefreshToken);
                    if (error.Code == "Auth.JwtError")
                        return BadRequest(error.Description);
                }
                return BadRequest();
                
            }

            return Ok(result.Payload);
        }
        
    }
}
