using Microsoft.AspNetCore.Identity;
using WorkoutTrackerApi.DTO.User;
using WorkoutTrackerApi.Models;
using WorkoutTrackerApi.Services.Interfaces;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Implementations;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<User?> GetUserByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<User?> GetUserByUserNameAsync(string username)
    {
        return await _userManager.FindByNameAsync(username);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<ServiceResult> DeleteUserAsync(User user)
    {
        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            var identityErrors = result.Errors.Select(e => new Error(e.Code, e.Description));
            return ServiceResult.Failure(identityErrors.ToArray());
        }
        
        return ServiceResult.Success();

    }

    public async Task<ServiceResult> DeleteUserAsync(string id)
    {
        var user = await GetUserByIdAsync(id);
        
        if(user is null)
            return ServiceResult.Failure(Error.User.NotFound());

        return await DeleteUserAsync(user);
    }
}