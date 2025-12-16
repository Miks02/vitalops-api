using WorkoutTrackerApi.DTO.Auth;
using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Models;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto request);

    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);

    Task<ServiceResult<TokenResponseDto>> RotateAuthTokens(string refreshToken);
}