using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Models;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(string id);

    Task<User?> GetUserByUserNameAsync(string username);

    Task<User?> GetUserByEmailAsync(string email);

    Task<UserWithRolesDto> GetUserWithRolesAsync(string username);

    Task<ServiceResult> DeleteUserAsync(User user);

    Task<ServiceResult> DeleteUserAsync(string id);
    
}