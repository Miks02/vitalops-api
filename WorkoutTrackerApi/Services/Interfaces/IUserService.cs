using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Models;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(string id);

    Task<User?> GetUserByUserNameAsync(string username);

    Task<User?> GetUserByEmailAsync(string email);
    
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);

    Task<UserDetailsDto> GetUserDetailsAsync(string id, CancellationToken cancellationToken = default);

    Task<IList<string>> GetUserRolesAsync(User user);

    Task<ServiceResult> DeleteUserAsync(User user);

    Task<ServiceResult> DeleteUserAsync(string id);
    
}