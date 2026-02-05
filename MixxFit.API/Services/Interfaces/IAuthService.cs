using MixxFit.API.DTO.Auth;
using MixxFit.API.Services.Results;
using MixxFit.API.DTO.User;
using MixxFit.API.Models;

namespace MixxFit.API.Services.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);

    Task<Result> UpdatePasswordAsync(string userId, UpdatePasswordDto request, CancellationToken cancellationToken);
    
    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken);
    
    Task<Result<AuthResponseDto>> RotateAuthTokens(string refreshToken, CancellationToken cancellationToken);
}
