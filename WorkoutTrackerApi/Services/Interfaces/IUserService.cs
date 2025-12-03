using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Models;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(string id);

    Task<User?> GetUserByUserNameAsync(string username);

    Task<User?> GetUserByEmail(string email);

    Task<ServiceResult> DeleteUserAsync(User user);

    Task<ServiceResult> DeleteUserAsync(string id);
    
}