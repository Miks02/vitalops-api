using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
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
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(
            RegisterRequestDto request,
            CancellationToken cancellationToken = default
            )
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);

            return HandleRefreshToken(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
            LoginRequestDto request,
            CancellationToken cancellationToken = default
            )
        {
            var result = await _authService.LoginAsync(request, cancellationToken);

            return HandleRefreshToken(result);
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            
            var result = await _authService.LogoutAsync(GetRefreshToken());

            DeleteRefreshTokenCookie();

            return result.ToActionResult();
        }

        [Authorize]
        [HttpGet("test")]
        public ActionResult Test()
        {

            return Ok(new { message = "You are authenticated", userId = $"{User.FindFirstValue(ClaimTypes.NameIdentifier)}"});
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RotateAuthTokens(CancellationToken cancellationToken = default)
        {
            string refreshToken = GetRefreshToken();

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(ApiResponse.Failure(Error.Auth.JwtError()));
            }

            var result = await _authService.RotateAuthTokens(refreshToken, cancellationToken);

            return HandleRefreshToken(result);
        }

        private ActionResult HandleRefreshToken(ServiceResult<AuthResponseDto> result)
        {
            if(!result.IsSucceeded)
            {
                return new ObjectResult(ApiResponse.Failure(result.Errors[0]));
            }

            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
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
                SameSite = SameSiteMode.None,
                Path = "/"
            };
            
            Response.Cookies.Delete("refreshToken", cookieOptions);
        }
        
        private string GetRefreshToken()
        {
            var allCookies = new Dictionary<string, string>();
            allCookies.AddRange(Request.Cookies);

            foreach (var cookie in allCookies)
            {
                Console.WriteLine("Cookie: " + cookie);

            }
            
            return Request.Cookies["refreshToken"] ?? "";

        }

    }
}
