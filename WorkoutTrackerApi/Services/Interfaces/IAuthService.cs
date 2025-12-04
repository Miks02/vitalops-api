using WorkoutTrackerApi.DTO.Auth;
using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Models;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<TokenResponseDto>> LoginAsync(LoginRequestDto request);

    Task<ServiceResult<UserDto>> RegisterAsync(RegisterRequestDto request);

    Task<ServiceResult<TokenResponseDto>> RotateAuthTokens(string refreshToken);
}