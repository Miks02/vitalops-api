using VitalOps.API.DTO.Auth;
using VitalOps.API.Services.Results;
using VitalOps.API.DTO.User;
using VitalOps.API.Models;

namespace VitalOps.API.Services.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);

    Task<Result> UpdatePasswordAsync(string userId, UpdatePasswordDto request, CancellationToken cancellationToken);
    
    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken);
    
    Task<Result<AuthResponseDto>> RotateAuthTokens(string refreshToken, CancellationToken cancellationToken);
}
