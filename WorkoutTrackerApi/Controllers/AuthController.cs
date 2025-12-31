using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerApi.DTO.Auth;
using WorkoutTrackerApi.DTO.Global;
using WorkoutTrackerApi.Services.Interfaces;
using WorkoutTrackerApi.Extensions;

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

            return result.ToActionResult();

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            return result.ToActionResult();

        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _authService.LogoutAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            return result.ToActionResult();
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

            return result.ToActionResult();
        }
        
    }
}
