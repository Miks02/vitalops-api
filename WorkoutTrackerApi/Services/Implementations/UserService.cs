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

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<UserWithRolesDto> GetUserWithRolesAsync(string username)
    {
        var user = await GetUserByUserNameAsync(username);

        if (user is null)
            throw new ArgumentNullException(nameof(username), "User with provided username has not been found");

        var roles = await _userManager.GetRolesAsync(user);

        var dto = new UserWithRolesDto()
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName!,
            Email = user.Email!,
            Roles = roles.AsReadOnly()
        };

        return dto;

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