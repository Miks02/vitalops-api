using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using NuGet.Protocol;
using VitalOps.API.DTO.Auth;
using VitalOps.API.Services.Interfaces;
using VitalOps.API.Services.Results;
using VitalOps.API.Extensions;

namespace VitalOps.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(
            RegisterRequestDto request,
            CancellationToken cancellationToken = default
            )
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);

            return HandleRefreshToken(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(
            LoginRequestDto request,
            CancellationToken cancellationToken = default
            )
        {
            var result = await _authService.LoginAsync(request, cancellationToken);

            return HandleRefreshToken(result);
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout(CancellationToken cancellationToken = default)
        {
            var result = await _authService.LogoutAsync(GetRefreshToken(), cancellationToken);

            DeleteRefreshTokenCookie();

            return result.ToActionResult();
        }

        [Authorize]
        [HttpGet("test")]
        public ActionResult Test()
        {
            return Ok(new { message = "You are authenticated", userId = $"{CurrentUserId}"});
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RotateAuthTokens(CancellationToken cancellationToken = default)
        {
            string refreshToken = GetRefreshToken();

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(Error.Auth.JwtError());
            }

            var result = await _authService.RotateAuthTokens(refreshToken, cancellationToken);

            return HandleRefreshToken(result);
        }

        [HttpPost("password")]
        public async Task<ActionResult> UpdatePassword(UpdatePasswordDto request, CancellationToken cancellationToken = default)
        {
            var result = await _authService.UpdatePasswordAsync(CurrentUserId, request, cancellationToken);
            return result.ToActionResult();
        }

        private ActionResult HandleRefreshToken(Result<AuthResponseDto> result)
        {
            if(!result.IsSucceeded)
                return new ObjectResult(result.Errors[0]);

            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            };

            var refreshToken = result.Payload!.RefreshToken;

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            if (result.Payload.User is null)
                return Ok(result.Payload.AccessToken.ToJson());

            return Ok(result.Payload);

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
