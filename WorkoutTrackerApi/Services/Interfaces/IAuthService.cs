using WorkoutTrackerApi.DTO.Auth;
using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Models;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    
    Task<ServiceResult> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    
    Task<ServiceResult<AuthResponseDto>> RotateAuthTokens(string refreshToken, CancellationToken cancellationToken = default);
}
