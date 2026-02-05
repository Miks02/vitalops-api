using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MixxFit.API.Services.Interfaces;

namespace MixxFit.API.Services.Implementations;

public class CurrentUserService : ICurrentUserService
{

    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
    {
        _http = http;
    }

    public string? UserId()
        => _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName()
        => _http.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

    public bool IsAdmin()
        => _http.HttpContext!.User.IsInRole("Admin");
}