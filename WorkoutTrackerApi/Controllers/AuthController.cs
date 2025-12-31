using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerApi.DTO.Auth;
using WorkoutTrackerApi.DTO.Global;
using WorkoutTrackerApi.Services.Interfaces;
using WorkoutTrackerApi.Extensions;
using WorkoutTrackerApi.Services.Results;

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

            return HandleRefreshToken(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            return HandleRefreshToken(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _authService.LogoutAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            DeleteRefreshTokenCookie();

            return result.ToActionResult();
        }

        [Authorize]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "You are authenticated", user = $"{User.Identity!.Name}" });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RotateAuthTokens()
        {

            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return BadRequest(ApiResponse.Failure(Error.Auth.JwtError("Refresh token cookie does not exist")));
            }
            
            var result = await _authService.RotateAuthTokens(refreshToken!);
            
            return HandleRefreshToken(result);
        }

        private ActionResult HandleRefreshToken(ServiceResult<AuthResponseDto> result)
        {
            if(!result.IsSucceeded)
            {
                return new ObjectResult(ApiResponse.Failure(result.Errors.First()));
            }

            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            var accessToken = result.Payload!.AccessToken;
            var refreshToken = result.Payload!.RefreshToken;
            var user = result.Payload.User;
            
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            if (user is null)
            {
                return new OkObjectResult
                    (ApiResponse<string>.Success("Tokens are regenerated successfully", accessToken));
            }

            return new OkObjectResult(ApiResponse<AuthResponseDto>.Success("Tokens are regenerated successfully", result.Payload));

        }

        private void DeleteRefreshTokenCookie()
        {
            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            
            Response.Cookies.Delete("refreshToken", cookieOptions);
        }

    }
}
